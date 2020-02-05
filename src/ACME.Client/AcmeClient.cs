using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Client
{
    // TODO: Need to add Logger
    // TODO: Need to add correct throw error type
    public class AcmeClient
    {
        public AsymmetricAlgorithm Key { get; set; }

        private readonly HttpClient Http = new HttpClient();

        public Protocol.Directory Directory { get; set; }

        public string Nonce { get; set; }

        public string Location { get; set; }

        private AcmeClient(Uri rootUrl, AsymmetricAlgorithm key)
        {
            Key = key;
            Http.BaseAddress = rootUrl;
        }

        public static async Task<AcmeClient> CreateAsync(Uri rootUrl, AsymmetricAlgorithm key)
        {
            var client = new AcmeClient(rootUrl, key);

            await client.DirectoryGetAsync();
            await client.NonceGetAsync();

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
            if (method == null)
            {
                method = HttpMethod.Get;
            }

            var request = new HttpRequestMessage(method, url);

            if (parameters != null)
            {
                string payload;

                if (skipSigning)
                {
                    payload = JsonConvert.SerializeObject(parameters);
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
                        jwsProtected.Key = new JsonWebKey(Key)
                        {
                            Algorithm = AlgorithmsEnum.RS256,
                        };
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

                    payload = JsonConvert.SerializeObject(jws);
                }

                request.Content = new StringContent(payload);
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/jose+json");
            }

            var response = await Http.SendAsync(request);

            if (!skipNonce)
            {
                if (response.Headers.TryGetValues("replay-nonce", out var values))
                {
                    Nonce = string.Join(",", values);
                }
            }

            if (response.StatusCode != correctHttpStatusCode)
            {
                var error = await Deserialize<Protocol.Error>(response);

                // TODO: Add AcmeException
                throw new WebException(JsonConvert.SerializeObject(error));
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
        public async Task<Protocol.Account> AccountCreateAsync(Protocol.Messages.NewAccount account)
        {
            var response = await Request(Directory.NewAccount, HttpMethod.Post, account, includePublicKey: true, correctHttpStatusCode: HttpStatusCode.Created);

            Location = response.Headers.Location?.ToString();

            if (string.IsNullOrEmpty(Location))
            {
                throw new InvalidDataException("Account creation response does not include Location header");
            }

            return await Deserialize<Protocol.Account>(response);
        }

        /// <summary>
        /// Updates existing Account information registered with the ACME CA.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.3.2"/>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.3.3"/>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.3.4"/>
        public async Task<Protocol.Account> AccountUpdateAsync(Protocol.Messages.NewAccount account)
        {
            var response = await Request(Location, HttpMethod.Post, account, correctHttpStatusCode: HttpStatusCode.Created);

            Location = response.Headers.Location?.ToString();

            if (string.IsNullOrEmpty(Location))
            {
                throw new InvalidDataException("Account update response does not include Location header");
            }

            return await Deserialize<Protocol.Account>(response);
        }

        /// <summary>
        /// Rotates the current Public key that is associated with this Account by the target ACME CA with a new Public key.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-18#section-7.3.5"/>
        public async Task<Protocol.Account> AccountChangeKeyAsync(AsymmetricAlgorithm newKey)
        {
            JsonWebSignature jws = new JsonWebSignature();
            JsonWebKey oldJWK = new JsonWebKey(Key)
            {
                Algorithm = AlgorithmsEnum.RS256,
            };
            JsonWebKey newJWK = new JsonWebKey(newKey)
            {
                Algorithm = AlgorithmsEnum.RS256,
            };

            jws.SetPayload(new Protocol.Messages.ChangeKey
            {
                Account = Location,
                OldKey = JsonConvert.SerializeObject(oldJWK),
            });
            jws.SetProtected(new JsonWebSignatureProtected
            {
                Algorithm = AlgorithmsEnum.RS256,
                Url = Directory.KeyChange,
                Key = newJWK,
            });
            jws.Sign(newKey);

            var response = await Request(Directory.KeyChange, HttpMethod.Post, jws);

            Key = newKey;

            return await Deserialize<Protocol.Account>(response);
        }

        /// <summary>
        /// Deactivates the current Account associated with this client.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.3.7"/>
        public async Task<Protocol.Account> AccountDeactivateAsync()
        {
            var response = await Request(Location, HttpMethod.Post, new Protocol.Account { Status = Protocol.AccountStatus.Deactivated });

            return await Deserialize<Protocol.Account>(response);
        }

        /// <summary>
        /// Creates a new Order for a Certificate which will contain one or more DNS Identifiers.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.4"/>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.1.3"/>
        public async Task<Protocol.Order> OrderCreateAsync(Protocol.Messages.NewOrder order)
        {
            var response = await Request(Directory.NewOrder, HttpMethod.Post, order, correctHttpStatusCode: HttpStatusCode.Created);

            return await Deserialize<Protocol.Order>(response);
        }

        /// <summary>
        /// Retrieves the current status and details of an existing Order.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.4"/>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.1.3"/>
        public async Task<Protocol.Order> OrderGetAsync(string orderUrl)
        {
            var response = await Request(orderUrl, skipNonce: true, correctHttpStatusCode: HttpStatusCode.Created);

            return await Deserialize<Protocol.Order>(response);
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
        public async Task<Protocol.Order> OrderFinalizeAsync(string orderUrl, string csr)
        {
            var response = await Request(orderUrl, HttpMethod.Post, new Protocol.Messages.FinalizeOrder { Csr = csr });

            return await Deserialize<Protocol.Order>(response);
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
            var response = await Request(authorizationUrl, HttpMethod.Post, new Protocol.Authorization { Status = Protocol.AuthorizationStatus.Deactivated });

            return await Deserialize<Protocol.Authorization>(response);
        }

        /// <summary>
        /// Responding to Challenges.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.5.1"/>
        public async Task<Protocol.Challenge> ChallengeValidateAsync(string challengeUrl)
        {
            var response = await Request(challengeUrl, HttpMethod.Post, new { });

            return await Deserialize<Protocol.Challenge>(response);
        }

        /// <summary>
        /// Responding to Challenges.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.5.1"/>
        public async Task<Protocol.Challenge> ChallengeGetAsync(string challengeUrl)
        {
            var response = await Request(challengeUrl, HttpMethod.Post, "");

            return await Deserialize<Protocol.Challenge>(response);
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
