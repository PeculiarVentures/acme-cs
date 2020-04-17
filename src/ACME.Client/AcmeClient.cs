using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PeculiarVentures.ACME.Helpers;
using PeculiarVentures.ACME.Web;
using PeculiarVentures.ACME.Web.Http;

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

        #region TypeConvert
        /// <summary>
        /// Dictionary extended types
        /// </summary>
        protected Dictionary<Type, Type> Types { get; } = new Dictionary<Type, Type>();

        /// <summary>
        /// Returns type from Types by key
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Type GetType(Type type)
        {
            if (Types.TryGetValue(type, out Type result))
            {
                return result;
            }
            return type;
        }

        /// <summary>
        /// Returns type from Types by key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Type GetType<T>()
        {
            return GetType(typeof(T));
        }
        #endregion

        protected AcmeClient(HttpClient http, AsymmetricAlgorithm key, ILogger logger = null)
        {
            Key = key;
            _http = http;
            _logger = logger;

            _logger?.LogInformation($"{nameof(AcmeClient)} is starting.");
        }

        public static async Task<AcmeClient> CreateAsync(Uri rootUrl, AsymmetricAlgorithm key, ILogger logger = null)
        {
            return await CreateAsync(new HttpClient { BaseAddress = rootUrl }, key, logger);
        }

        public static async Task<AcmeClient> CreateAsync(HttpClient http, AsymmetricAlgorithm key, ILogger logger = null)
        {
            var client = new AcmeClient(http, key, logger);

            await client.DirectoryGetAsync();
            await client.NonceGetAsync();

            return client;
        }

        protected async Task<HttpResponseMessage> Request(
            string url,
            HttpMethod method = null,
            object payload = null,
            bool includePublicKey = false
        )
        {
            string content = null;

            if (method == null)
            {
                method = HttpMethod.Get;
            }

            var request = new HttpRequestMessage(method, url);

            if (payload != null)
            {
                var jws = new JsonWebSignature
                {
                    Payload = "",
                };

                jws.SetProtected(new JsonWebSignatureProtected
                {
                    Algorithm = AlgorithmsEnum.RS256,
                    Nonce = Nonce,
                    Url = url,
                    KeyID = includePublicKey ? null : Location,
                    Key = includePublicKey ? new JsonWebKey(Key) : null
                });

                if (!(payload is string))
                {
                    jws.SetPayload(payload);
                }

                jws.Sign(Key);

                content = JsonConvert.SerializeObject(jws, Formatting.Indented);
            }

            if (!String.IsNullOrEmpty(content))
            {
                request.Content = new StringContent(content);
                request.Content.Headers.ContentType = Protocol.MediaTypeHeader.JsonContentTypeHeaderValue;
            }

            _logger?.LogDebug($"{nameof(AcmeClient)} request \nParameters: \n{request} \nContent: \n{content}");

            var response = await _http.SendAsync(request);

            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException)
            {
                string message = null;

                try
                {
                    var error = await Deserialize<Protocol.Error>(response);

                    message = $"{error.Type}: {error.Detail}";
                }
                catch
                {
                    _logger?.LogError("Cannot parse ACME Error from Client response");
                    _logger?.LogError(await response.Content.ReadAsStringAsync());
                }

                if (string.IsNullOrEmpty(message))
                {
                    message = $"Unexpected response status code [{response.StatusCode}] for [{method}] request to [{url}]";
                }

                var ex = new AcmeException(Protocol.ErrorType.ServerInternal, message);

                _logger?.LogError(ex, $"{nameof(AcmeClient)} request error.");

                throw ex;
            }

            response.Headers.TryGetValues("replay-nonce", out var replayNonceValues);

            if (replayNonceValues != null)
            {
                Nonce = replayNonceValues.FirstOrDefault();
            }

            return response;
        }

        protected async Task<AcmeResponse> Request(
            Type type,
            string url,
            HttpMethod method = null,
            object payload = null,
            bool includePublicKey = false
        )
        {
            var response = await Request(url, method, payload, includePublicKey);

            response.Headers.TryGetValues("replay-nonce", out var replayNonceValues);
            response.Headers.TryGetValues("location", out var locationValues);
            response.Headers.TryGetValues("link", out var linksValues);

            var acmeResponse = new AcmeResponse
            {
                StatusCode = (int)response.StatusCode,
                ReplayNonce = replayNonceValues?.FirstOrDefault(),
                Location = locationValues?.FirstOrDefault(),
                Links = linksValues != null ? new LinkHeaderCollection(linksValues.ToArray()) : null,
                Content = await Deserialize(response, type),
            };

            _logger?.LogDebug($"{nameof(AcmeClient)} response: \n{acmeResponse}");

            return acmeResponse;
        }

        protected async Task<AcmeResponse<T>> Request<T>(
            string url,
            HttpMethod method = null,
            object payload = null,
            bool includePublicKey = false
        ) where T : class
        {
            var response = await Request(url, method, payload, includePublicKey);

            response.Headers.TryGetValues("replay-nonce", out var replayNonceValues);
            response.Headers.TryGetValues("location", out var locationValues);
            response.Headers.TryGetValues("links", out var linksValues);

            var acmeResponse = new AcmeResponse<T>
            {
                StatusCode = (int)response.StatusCode,
                ReplayNonce = replayNonceValues?.FirstOrDefault(),
                Location = locationValues?.FirstOrDefault(),
                Links = linksValues != null ? new LinkHeaderCollection(linksValues.ToArray()) : null,
                Content = await Deserialize<T>(response),
            };

            _logger?.LogDebug($"{nameof(AcmeClient)} response: \n{acmeResponse}");

            return acmeResponse;
        }

        protected async Task<object> Deserialize(HttpResponseMessage response, Type type)
        {
            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject(content, type);
        }

        protected async Task<T> Deserialize<T>(HttpResponseMessage response)
        {
            return (T)(await Deserialize(response, typeof(T)));
        }

        /// <summary>
        /// Retrieves the Directory object from the target ACME CA.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.1.1"/>
        public async Task<AcmeResponse<Protocol.Directory>> DirectoryGetAsync()
        {
            var response = await Request(GetType(typeof(Protocol.Directory)), "directory");

            Directory = (Protocol.Directory)response.Content;

            return response;
        }

        /// <summary>
        /// Retrieves a fresh nonce to be used in subsequent communication between the client and target ACME CA.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.2"/>
        public async Task NonceGetAsync()
        {
            await Request(Directory.NewNonce, HttpMethod.Head);
        }

        /// <summary>
        /// Account create.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.3"/>
        public async Task<AcmeResponse<Protocol.Account>> AccountCreateAsync(Protocol.Messages.NewAccount account)
        {
            var response = await Request(GetType(typeof(Protocol.Account)), Directory.NewAccount, HttpMethod.Post, account, true);

            if (string.IsNullOrEmpty(response.Location))
            {
                var ex = new AcmeException(Protocol.ErrorType.IncorrectResponse, "Account creation response does not include Location header.");

                _logger?.LogError(ex, $"{nameof(AcmeClient)} request error.");

                throw ex;
            }

            Location = response.Location;

            return response;
        }

        /// <summary>
        /// Account create with external account binding.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.3.5"/>
        public async Task<AcmeResponse<Protocol.Account>> AccountCreateAsync(Protocol.Messages.NewAccount account, string kid, string keyMac)
        {
            var jws = new JsonWebSignature();

            jws.SetProtected(new JsonWebSignatureProtected
            {
                Algorithm = AlgorithmsEnum.HS256,
                KeyID = kid,
                Url = Directory.NewAccount,
            });

            jws.SetPayload(new JsonWebKey(Key));

            var key = HMAC.Create("HMACSHA256");
            key.Key = Base64Url.Decode(keyMac);

            jws.Sign(key);

            account.ExternalAccountBinding = jws;

            return await AccountCreateAsync(account);
        }

        /// <summary>
        /// Updates existing Account information registered with the ACME CA.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.3.2"/>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.3.3"/>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.3.4"/>
        public async Task<AcmeResponse<Protocol.Account>> AccountUpdateAsync(Protocol.Messages.NewAccount account)
        {
            var response = await Request(GetType(typeof(Protocol.Account)), Directory.NewAccount, HttpMethod.Post, account);

            if (string.IsNullOrEmpty(response.Location))
            {
                var ex = new AcmeException(Protocol.ErrorType.IncorrectResponse, "Account updating response does not include Location header.");

                _logger?.LogError(ex, $"{nameof(AcmeClient)} request error.");

                throw ex;
            }

            Location = response.Location;

            return response;
        }

        /// <summary>
        /// Rotates the current Public key that is associated with this Account by the target ACME CA with a new Public key.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-18#section-7.3.5"/>
        public async Task<AcmeResponse<Protocol.Account>> AccountChangeKeyAsync(AsymmetricAlgorithm keyNew)
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

            var response = await Request(GetType(typeof(Protocol.Account)), Directory.KeyChange, HttpMethod.Post, jws);

            Key = keyNew;

            return response;
        }

        /// <summary>
        /// Deactivates the current Account associated with this client.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.3.7"/>
        public async Task<AcmeResponse<Protocol.Account>> AccountDeactivateAsync()
        {
            return await Request(
                GetType(typeof(Protocol.Account)),
                Location,
                HttpMethod.Post,
                new Protocol.Account { Status = Protocol.AccountStatus.Deactivated }
            );
        }

        /// <summary>
        /// Creates a new Order for a Certificate which will contain one or more DNS Identifiers.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.4"/>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.1.3"/>
        public async Task<AcmeResponse<Protocol.Order>> OrderCreateAsync(Protocol.Messages.NewOrder order)
        {
            return await Request(GetType(typeof(Protocol.Order)), Directory.NewOrder, HttpMethod.Post, order);
        }

        /// <summary>
        /// Retrieves the current status and details of an existing Order.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.4"/>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.1.3"/>
        public async Task<AcmeResponse<Protocol.Order>> OrderGetAsync(string orderUrl)
        {
            return await Request(GetType(typeof(Protocol.Order)), orderUrl, HttpMethod.Post, "");
        }

        public async Task<AcmeResponse<Protocol.OrderList>> OrderListGetAsync(string ordersUrl)
        {
            return await Request(GetType(typeof(Protocol.OrderList)), ordersUrl, HttpMethod.Post, "");
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
        public async Task<AcmeResponse<Protocol.Order>> OrderFinalizeAsync(string orderUrl, Protocol.Messages.FinalizeOrder finalizeOrder)
        {
            return await Request(
                GetType(typeof(Protocol.Order)),
                orderUrl,
                HttpMethod.Post,
                finalizeOrder
            );
        }

        /// <summary>
        /// Retrieves the details of an Authorization associated with a previously created Order.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.5"/>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.1.4"/>
        public async Task<AcmeResponse<Protocol.Authorization>> AuthorizationCreateAsync(string authorizationUrl)
        {
            return await Request(GetType(typeof(Protocol.Authorization)), authorizationUrl, HttpMethod.Post, "");
        }

        /// <summary>
        /// Deactivates a specific Authorization and thereby relinquishes the authority to issue Certificates for the associated Identifier.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.5.2"/>
        public async Task<AcmeResponse<Protocol.Authorization>> AuthorizationDeactivateAsync(string authorizationUrl)
        {
            return await Request(
                GetType(typeof(Protocol.Authorization)),
                authorizationUrl,
                HttpMethod.Post,
                new Protocol.Authorization
                {
                    Status = Protocol.AuthorizationStatus.Deactivated
                }
            );
        }

        /// <summary>
        /// Responding to Challenges.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.5.1"/>
        public async Task<AcmeResponse<Protocol.Challenge>> ChallengeValidateAsync(string challengeUrl)
        {
            return await Request(GetType(typeof(Protocol.Challenge)), challengeUrl, HttpMethod.Post, new { });
        }

        /// <summary>
        /// Responding to Challenges.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.5.1"/>
        public async Task<AcmeResponse<Protocol.Challenge>> ChallengeGetAsync(string challengeUrl)
        {
            return await Request(GetType(typeof(Protocol.Challenge)), challengeUrl, HttpMethod.Post, "");
        }

        /// <summary>
        /// Certificate revoke.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-18#section-7.6"/>
        public async Task CertificateRevokeAsync(Protocol.Messages.RevokeCertificate revokeCertificate)
        {
            await Request(
                Directory.RevokeCertificate,
                HttpMethod.Post,
                revokeCertificate
            );
        }
    }
}
