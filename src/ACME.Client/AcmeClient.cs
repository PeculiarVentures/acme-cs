using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Client
{
    /// <see cref="https://tools.ietf.org/html/rfc8555"/>
    public class AcmeClient
    {
        private readonly HttpClient _http;

        private readonly ILogger _logger;

        public AsymmetricAlgorithm Key { get; private set; }

        public Protocol.Directory Directory { get; set; }

        public string Nonce { get; set; }

        public string Location { get; set; }

        private AcmeClient(HttpClient http, AsymmetricAlgorithm key, ILogger logger = null)
        {
            Key = key;
            _http = http;
            _logger = logger;
        }

        public static async Task<AcmeClient> CreateAsync(Uri rootUrl, AsymmetricAlgorithm key, ILogger logger = null)
        {
            return await CreateAsync(new HttpClient { BaseAddress = rootUrl }, key, logger);
        }

        public static async Task<AcmeClient> CreateAsync(HttpClient http, AsymmetricAlgorithm key, ILogger logger = null)
        {
            var client = new AcmeClient(http, key, logger);

            client._logger?.LogInformation($"{nameof(AcmeClient)} is starting.");

            await client.DirectoryGetAsync();
            await client.NonceGetAsync();

            client._logger?.LogInformation($"{nameof(AcmeClient)} is ready.");

            return client;
        }

        private async Task<HttpResponseMessage> Request(
            string url,
            HttpMethod method = null,
            object parameters = null,
            bool skipNonce = false,
            bool skipSigning = false,
            bool includePublicKey = false,
            HttpStatusCode correctHttpStatusCode = HttpStatusCode.OK
        )
        {
            string content = null;

            if (method == null)
            {
                method = HttpMethod.Get;
            }

            var request = new HttpRequestMessage(method, url);

            if (parameters != null)
            {
                if (skipSigning)
                {
                    content = JsonConvert.SerializeObject(parameters, Formatting.Indented);
                }
                else
                {
                    JsonWebSignature jws = new JsonWebSignature
                    {
                        Payload = "",
                    };

                    var jwsProtected = new JsonWebSignatureProtected
                    {
                        Algorithm = AlgorithmsEnum.RS256,
                        Nonce = Nonce,
                        Url = url,
                    };

                    if (includePublicKey)
                    {
                        jwsProtected.Key = new JsonWebKey(Key);
                    }
                    else
                    {
                        jwsProtected.KeyID = Location;
                    }

                    jws.SetProtected(jwsProtected);

                    if (!(parameters is string))
                    {
                        jws.SetPayload(parameters);
                    }

                    jws.Sign(Key);

                    content = JsonConvert.SerializeObject(jws, Formatting.Indented);
                }
            }

            if (!String.IsNullOrEmpty(content))
            {
                request.Content = new StringContent(content);
                request.Content.Headers.ContentType = MediaTypeHeader.JsonContentTypeHeaderValue;
            }

            _logger?.LogDebug($"{nameof(AcmeClient)} request \nParameters: \n{request} \nContent: \n{content}");

            var response = await _http.SendAsync(request);

            if (!skipNonce)
            {
                if (response.Headers.TryGetValues("replay-nonce", out var values))
                {
                    Nonce = string.Join(",", values);
                }
            }

            if (response.StatusCode != correctHttpStatusCode)
            {
                string message = null;
                Error error = null;

                if (MediaTypeHeader.ProblemJsonContentTypeHeaderValue.Equals(response.Content?.Headers?.ContentType))
                {
                    error = await Deserialize<Error>(response);
                    message = error.Detail;
                }

                if (string.IsNullOrEmpty(message))
                {
                    message = $"Unexpected response status code [{response.StatusCode}].";
                }

                var ex = new AcmeException(message, error);

                _logger?.LogError(ex, $"{nameof(AcmeClient)} request error.");

                throw ex;
            }

            return response;
        }

        private async Task<T> Request<T>(
            string url,
            HttpMethod method = null,
            object parameters = null,
            bool skipNonce = false,
            bool skipSigning = false,
            bool includePublicKey = false,
            HttpStatusCode correctHttpStatusCode = HttpStatusCode.OK
        )
        {
            return await Deserialize<T>(await Request(url, method, parameters, skipNonce, skipSigning, includePublicKey, correctHttpStatusCode));
        }

        private async Task<T> Deserialize<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            _logger?.LogDebug($"{nameof(AcmeClient)} response \nParameters: \n{response} \nContent: \n{content}");

            return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
        }

        /// <summary>
        /// Retrieves the Directory object from the target ACME CA.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.1.1"/>
        public async Task<Protocol.Directory> DirectoryGetAsync()
        {
            return Directory = await Request<Protocol.Directory>("directory", skipNonce: true);
        }

        /// <summary>
        /// Retrieves a fresh nonce to be used in subsequent communication between the client and target ACME CA.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.2"/>
        public async Task NonceGetAsync()
        {
            await Request(Directory.NewNonce, HttpMethod.Head);
        }

        /// <summary></summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.3"/>
        public async Task<Account> AccountCreateAsync(Protocol.Messages.NewAccount account)
        {
            var response = await Request(Directory.NewAccount, HttpMethod.Post, account, includePublicKey: true, correctHttpStatusCode: HttpStatusCode.Created);

            Location = response.Headers.Location?.ToString();

            if (string.IsNullOrEmpty(Location))
            {
                var ex = new AcmeException("Account creation response does not include Location header.");

                _logger?.LogError(ex, $"{nameof(AcmeClient)} request error.");

                throw ex;
            }

            return await Deserialize<Account>(response);
        }

        /// <summary>
        /// Updates existing Account information registered with the ACME CA.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.3.2"/>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.3.3"/>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.3.4"/>
        public async Task<Account> AccountUpdateAsync(Protocol.Messages.NewAccount account)
        {
            var response = await Request(Location, HttpMethod.Post, account, correctHttpStatusCode: HttpStatusCode.Created);

            Location = response.Headers.Location?.ToString();

            if (string.IsNullOrEmpty(Location))
            {
                var ex = new AcmeException("Account creation response does not include Location header.");

                _logger?.LogError(ex, $"{nameof(AcmeClient)} request error.");

                throw ex;
            }

            return await Deserialize<Account>(response);
        }

        /// <summary>
        /// Rotates the current Public key that is associated with this Account by the target ACME CA with a new Public key.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-18#section-7.3.5"/>
        public async Task<Account> AccountChangeKeyAsync(AsymmetricAlgorithm keyNew)
        {
            var jws = new JsonWebSignature();
            var jwk = new JsonWebKey(Key);
            var jwkNew = new JsonWebKey(keyNew);

            jws.SetPayload(new Protocol.Messages.ChangeKey
            {
                Account = Location,
                Key = jwk,
            });
            jws.SetProtected(new JsonWebSignatureProtected
            {
                Algorithm = AlgorithmsEnum.RS256,
                Url = Directory.KeyChange,
                Key = jwkNew,
            });
            jws.Sign(keyNew);

            var response = await Request(Directory.KeyChange, HttpMethod.Post, jws);

            Key = keyNew;

            return await Deserialize<Account>(response);
        }

        /// <summary>
        /// Deactivates the current Account associated with this client.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.3.7"/>
        public async Task<Account> AccountDeactivateAsync()
        {
            var response = await Request(Location, HttpMethod.Post, new Account { Status = AccountStatus.Deactivated });

            return await Deserialize<Account>(response);
        }

        /// <summary>
        /// Creates a new Order for a Certificate which will contain one or more DNS Identifiers.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.4"/>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.1.3"/>
        public async Task<Order> OrderCreateAsync(Protocol.Messages.NewOrder order)
        {
            var response = await Request(Directory.NewOrder, HttpMethod.Post, order, correctHttpStatusCode: HttpStatusCode.Created);

            return await Deserialize<Order>(response);
        }

        /// <summary>
        /// Retrieves the current status and details of an existing Order.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.4"/>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.1.3"/>
        public async Task<Order> OrderGetAsync(string orderUrl)
        {
            var response = await Request(orderUrl, skipNonce: true, correctHttpStatusCode: HttpStatusCode.Created);

            return await Deserialize<Order>(response);
        }

        /// <summary>
        /// Download the issued certificate.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/rfc8555#section-7.4.2"/>
        public async Task<byte[]> OrderCertificateGetAsync(string certificateUrl)
        {
            var response = await Request(certificateUrl, HttpMethod.Post, "");

            return await response.Content.ReadAsByteArrayAsync();
        }

        /// <summary>
        /// Applying for Certificate Issuance.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.4"/>
        public async Task<Order> OrderFinalizeAsync(string orderUrl, string csr)
        {
            var response = await Request(orderUrl, HttpMethod.Post, new Protocol.Messages.FinalizeOrder { Csr = csr });

            return await Deserialize<Order>(response);
        }

        /// <summary>
        /// Retrieves the details of an Authorization associated with a previously created Order.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.5"/>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.1.4"/>
        public async Task<Protocol.Authorization> AuthorizationCreateAsync(string authorizationUrl)
        {
            var response = await Request(authorizationUrl, HttpMethod.Post, "");

            return await Deserialize<Protocol.Authorization>(response);
        }

        /// <summary>
        /// Deactivates a specific Authorization and thereby relinquishes the authority to issue Certificates for the associated Identifier.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.5.2"/>
        public async Task<Protocol.Authorization> AuthorizationDeactivateAsync(string authorizationUrl)
        {
            var response = await Request(authorizationUrl, HttpMethod.Post, new Protocol.Authorization { Status = AuthorizationStatus.Deactivated });

            return await Deserialize<Protocol.Authorization>(response);
        }

        /// <summary>
        /// Responding to Challenges.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.5.1"/>
        public async Task<Challenge> ChallengeValidateAsync(string challengeUrl)
        {
            var response = await Request(challengeUrl, HttpMethod.Post, new { });

            return await Deserialize<Challenge>(response);
        }

        /// <summary>
        /// Responding to Challenges.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.5.1"/>
        public async Task<Challenge> ChallengeGetAsync(string challengeUrl)
        {
            var response = await Request(challengeUrl, HttpMethod.Post, "");

            return await Deserialize<Challenge>(response);
        }

        /// <summary>
        /// Certificate revoke.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-18#section-7.6"/>
        public async Task CertificateRevokeAsync(string certificate)
        {
            await Request(Directory.RevokeCertificate, HttpMethod.Post, new Protocol.Messages.RevokeCertificate { Certificate = certificate });
        }
    }
}