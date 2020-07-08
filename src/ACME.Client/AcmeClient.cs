using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using PeculiarVentures.ACME.Helpers;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Client
{
    /// <see cref="https://tools.ietf.org/html/rfc8555"/>
    public class AcmeClient
    {
        private readonly HttpClient _http;

        protected ILogger Logger { get; } = LogManager.GetLogger("ACME.Client");

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

        protected AcmeClient(HttpClient http, AsymmetricAlgorithm key)
        {
            Key = key;
            _http = http;
        }

        public static async Task<AcmeClient> CreateAsync(Uri rootUrl, AsymmetricAlgorithm key)
        {
            return await CreateAsync(new HttpClient { BaseAddress = rootUrl }, key);
        }

        public static async Task<AcmeClient> CreateAsync(HttpClient http, AsymmetricAlgorithm key)
        {
            var client = new AcmeClient(http, key);

            await client.DirectoryGetAsync();
            await client.NonceGetAsync();

            return client;
        }

        protected async Task<AcmeResponse> Request(
            string url,
            RequestParams @params = null
        )
        {
            @params = @params ?? new RequestParams();

            string content = null;

            var request = new HttpRequestMessage(@params.Method, url);

            if (@params.Payload != null)
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
                    KeyID = @params.IncludePublicKey ? null : Location,
                    Key = @params.IncludePublicKey ? new JsonWebKey(Key) : null
                });

                if (!(@params.Payload is string))
                {
                    jws.SetPayload(@params.Payload);
                }

                jws.Sign(Key);

                content = JsonConvert.SerializeObject(jws, Formatting.Indented);
            }

            if (!string.IsNullOrEmpty(content))
            {
                request.Content = new StringContent(content);
                request.Content.Headers.ContentType = MediaTypeHeader.JsonContentTypeHeaderValue;
            }

            // Copy headers to HTTP request from ACME request
            foreach (var key in @params.Headers.AllKeys)
            {
                request.Headers.Add(key, @params.Headers.Get(key));
            }

            Logger.Debug("Request {method} {path} {token}", @params.Method, url, @params.Headers.ToString(), content);

            var response = _http.SendAsync(request).Result;

            // Get Replay-Nonce from response
            IEnumerable<string> nonceList;
            response.Headers.TryGetValues(HeaderCollection.ReplayNonceHeader, out nonceList);
            Nonce = nonceList?.FirstOrDefault();

            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException)
            {
                string message = null;
                string errorJson = null;
                Protocol.Error error = null;
                try
                {
                    errorJson = await response.Content.ReadAsStringAsync();

                    try
                    {
                        error = JsonConvert.DeserializeObject<Protocol.Error>(errorJson);

                        message = $"{error.Type}: {error.Detail}";
                        Logger.Debug("ACME Error {@error}", error);
                    }
                    catch
                    {
                        Logger.Error("Cannot parse ACME Error from Client response");
                        Logger.Debug(errorJson);
                    }
                }
                catch (System.Exception e)
                {
                    Logger.Error("Cannot parse content from Client response");
                    Logger.Debug(e);
                }

                AcmeException ex;
                if (string.IsNullOrEmpty(message))
                {
                    message = $"Unexpected response status code [{response.StatusCode}] for [{@params.Method}] request to [{url}]";
                    ex = new AcmeException(Protocol.ErrorType.ServerInternal, message, response.StatusCode);
                }
                else
                {
                    ex = new AcmeException(error.Type, error.Detail, response.StatusCode);
                }

                Logger.Error(ex);

                throw ex;
            }

            // Copy headers to ACME Response
            var headers = new HeaderCollection();
            foreach (var header in response.Headers)
            {
                headers.Add(header.Key, string.Join(", ", header.Value));
            }

            var acmeResponse = new AcmeResponse
            {
                StatusCode = (int)response.StatusCode,
                Headers = headers,
                Content = response.Content == null
                    ? new MediaTypeContent("", new byte[0])
                    : new MediaTypeContent(
                        response.Content.Headers.ContentType?.MediaType ?? "",
                        await response.Content.ReadAsByteArrayAsync()),
            };

            Logger.Debug("Response {@response}", acmeResponse);

            return acmeResponse;
        }

        protected async Task<AcmeResponse> Request(
            Type type,
            string url,
            RequestParams @params = null
        )
        {
            var response = await Request(url, @params);
            response.Content = Deserialize((MediaTypeContent)response.Content, type);
            return response;
        }

        protected async Task<AcmeResponse<T>> Request<T>(
            string url,
            RequestParams @params = null
        ) where T : class
        {
            var response = await Request(url, @params);
            response.Content = Deserialize<T>((MediaTypeContent)response.Content);

            return response;
        }

        protected object Deserialize(MediaTypeContent content, Type type)
        {
            var text = Encoding.UTF8.GetString(content.ToArray());
            var json = JsonConvert.DeserializeObject(text, type);

            Logger.Debug("Response.Content.JSON: {@json}", json);

            return json;
        }

        protected T Deserialize<T>(MediaTypeContent response)
        {
            return (T)Deserialize(response, typeof(T));
        }

        /// <summary>
        /// Retrieves the Directory object from the target ACME CA.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.1.1"/>
        public async Task<AcmeResponse<Protocol.Directory>> DirectoryGetAsync()
        {
            Logger.Info("Getting Directory object");

            var response = await Request(GetType(typeof(Protocol.Directory)), "");

            Directory = (Protocol.Directory)response.Content;

            return response;
        }

        /// <summary>
        /// Retrieves a fresh nonce to be used in subsequent communication between the client and target ACME CA.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.2"/>
        public async Task NonceGetAsync()
        {
            Logger.Info("Getting Nonce value");

            await Request(Directory.NewNonce, new RequestParams
            {
                Method = HttpMethod.Head
            });
        }

        /// <summary>
        /// Account create.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.3"/>
        public async Task<AcmeResponse<Protocol.Account>> AccountCreateAsync(Protocol.Messages.NewAccount account)
        {
            Logger.Info("Creating a new ACME account. Params:{@params}", account);

            var response = await Request(GetType(typeof(Protocol.Account)), Directory.NewAccount, new RequestParams
            {
                Method = HttpMethod.Post,
                Payload = account,
                IncludePublicKey = true,
            });

            if (string.IsNullOrEmpty(response.Headers.Location))
            {
                var ex = new AcmeException(Protocol.ErrorType.IncorrectResponse, "Account creation response does not include Location header.");

                Logger.Error(ex, $"{nameof(AcmeClient)} request error.");

                throw ex;
            }

            Location = response.Headers.Location;

            return response;
        }

        /// <summary>
        /// Account create with external account binding.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.3.5"/>
        public async Task<AcmeResponse<Protocol.Account>> AccountCreateAsync(Protocol.Messages.NewAccount account, string kid, string keyMac)
        {
            Logger.Info("Creating a new ACME account with external account binding. Params:{@params}", new
            {
                Account = account,
                KeyId = kid,
                KeyMac = keyMac,
            });

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
            Logger.Info("Updating an ACME account. Params:{@params}", account);

            var response = await Request(GetType(typeof(Protocol.Account)), Directory.NewAccount,
                new RequestParams
                {
                    Method = HttpMethod.Post,
                    Payload = account
                });

            if (string.IsNullOrEmpty(response.Headers.Location))
            {
                var ex = new AcmeException(Protocol.ErrorType.IncorrectResponse, "Account updating response does not include Location header.");

                Logger.Error(ex, $"{nameof(AcmeClient)} request error.");

                throw ex;
            }

            Location = response.Headers.Location;

            return response;
        }

        /// <summary>
        /// Rotates the current Public key that is associated with this Account by the target ACME CA with a new Public key.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-18#section-7.3.5"/>
        public async Task<AcmeResponse<Protocol.Account>> AccountChangeKeyAsync(AsymmetricAlgorithm keyNew)
        {
            Logger.Info("Changing an ACME account key. Params:{@params}", keyNew);

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

            var response = await Request(GetType(typeof(Protocol.Account)), Directory.KeyChange,
                new RequestParams
                {
                    Method = HttpMethod.Post,
                    Payload = jws
                });

            Key = keyNew;

            return response;
        }

        /// <summary>
        /// Deactivates the current Account associated with this client.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.3.7"/>
        public async Task<AcmeResponse<Protocol.Account>> AccountDeactivateAsync()
        {
            Logger.Info("Deactivating an ACME account");

            return await Request(
                GetType(typeof(Protocol.Account)),
                Location,
                new RequestParams
                {
                    Method = HttpMethod.Post,
                    Payload = new Protocol.Account { Status = Protocol.AccountStatus.Deactivated }
                }
            );
        }

        /// <summary>
        /// Creates a new Order for a Certificate which will contain one or more DNS Identifiers.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.4"/>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.1.3"/>
        public async Task<AcmeResponse<Protocol.Order>> OrderCreateAsync(Protocol.Messages.NewOrder order)
        {
            Logger.Info("Creating a new ACME order. Params:{@params}", order);

            return await Request(GetType(typeof(Protocol.Order)), Directory.NewOrder,
                new RequestParams
                {
                    Method = HttpMethod.Post,
                    Payload = order
                });
        }

        /// <summary>
        /// Retrieves the current status and details of an existing Order.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.4"/>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.1.3"/>
        public async Task<AcmeResponse<Protocol.Order>> OrderGetAsync(string orderUrl)
        {
            Logger.Info("Getting an ACME order. Params:{@params}", orderUrl);

            return await Request(GetType(typeof(Protocol.Order)), orderUrl,
                new RequestParams
                {
                    Method = HttpMethod.Post,
                    Payload = ""
                });
        }

        public async Task<AcmeResponse<Protocol.OrderList>> OrderListGetAsync(string ordersUrl)
        {
            Logger.Info("Getting an ACME order list. Params:{@params}", ordersUrl);

            return await Request(GetType(typeof(Protocol.OrderList)), ordersUrl,
                new RequestParams
                {
                    Method = HttpMethod.Post,
                    Payload = ""
                });
        }

        /// <summary>
        /// Download the issued certificate.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/rfc8555#section-7.4.2"/>
        public async Task<AcmeResponse<X509Certificate2Collection>> OrderCertificateGetAsync(string certificateUrl)
        {
            Logger.Info("Getting an ACME certificate list. Params:{@params}", certificateUrl);

            var response = await Request(certificateUrl,
                new RequestParams
                {
                    Method = HttpMethod.Post,
                    Payload = ""
                });
            var chain = new X509Certificate2Collection();
            var mediaContnet = (MediaTypeContent)response.Content;
            var rawData = mediaContnet.ToArray();

            switch (mediaContnet.Type)
            {
                case "application/pkix-cert":
                    chain.Add(new X509Certificate2(rawData));
                    break;
                case "application/pem-certificate-chain":
                    var dec = Encoding.UTF8.GetString(rawData);
                    chain.AddRange(PemConverter.Decode(dec).Select(o => new X509Certificate2(o)).ToArray());
                    break;
                case "application/pkcs7-mime":
                    chain.Import(rawData);
                    break;
                default:
                    throw new Exception("Wrong Content type");
            }
            var resp = new AcmeResponse<X509Certificate2Collection>()
            {
                StatusCode = (int)response.StatusCode,
                // todo need upgrade
                //ReplayNonce = replayNonceValues?.FirstOrDefault(),
                //Location = locationValues?.FirstOrDefault(),
                //Links = linksValues != null ? new LinkHeaderCollection(linksValues.ToArray()) : null,
                Content = chain,
            };
            return resp;
        }

        /// <summary>
        /// Applying for Certificate Issuance.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.4"/>
        public async Task<AcmeResponse<Protocol.Order>> OrderFinalizeAsync(string orderUrl, Protocol.Messages.FinalizeOrder finalizeOrder)
        {
            Logger.Info("Finalize an ACME order. Params:{@params}", new
            {
                Url = orderUrl,
                Params = finalizeOrder,
            });

            return await Request(
                GetType(typeof(Protocol.Order)),
                orderUrl,
                new RequestParams
                {
                    Method = HttpMethod.Post,
                    Payload = finalizeOrder,
                }
            );
        }

        /// <summary>
        /// Retrieves the details of an Authorization associated with a previously created Order.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.5"/>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.1.4"/>
        public async Task<AcmeResponse<Protocol.Authorization>> AuthorizationCreateAsync(string authorizationUrl)
        {
            Logger.Info("Creating an ACME authorization. Params:{@params}", authorizationUrl);

            return await Request(GetType(typeof(Protocol.Authorization)), authorizationUrl,
                new RequestParams
                {
                    Method = HttpMethod.Post,
                    Payload = "",
                });
        }

        /// <summary>
        /// Deactivates a specific Authorization and thereby relinquishes the authority to issue Certificates for the associated Identifier.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.5.2"/>
        public async Task<AcmeResponse<Protocol.Authorization>> AuthorizationDeactivateAsync(string authorizationUrl)
        {
            Logger.Info("Deactivating an ACME authorization. Params:{@params}", authorizationUrl);

            return await Request(
                GetType(typeof(Protocol.Authorization)),
                authorizationUrl,
                new RequestParams
                {
                    Method = HttpMethod.Post,
                    Payload = new Protocol.Authorization
                    {
                        Status = Protocol.AuthorizationStatus.Deactivated
                    },
                }
            );
        }

        /// <summary>
        /// Responding to Challenges.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.5.1"/>
        public async Task<AcmeResponse<Protocol.Challenge>> ChallengeValidateAsync(string challengeUrl)
        {
            Logger.Info("Validating an ACME challenge. Params:{@params}", challengeUrl);

            return await Request(GetType(typeof(Protocol.Challenge)), challengeUrl,
                new RequestParams
                {
                    Method = HttpMethod.Post,
                    Payload = new { },
                });
        }

        /// <summary>
        /// Responding to Challenges.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.5.1"/>
        public async Task<AcmeResponse<Protocol.Challenge>> ChallengeGetAsync(string challengeUrl)
        {
            Logger.Info("Getting an ACME challenge. Params:{@params}", challengeUrl);

            return await Request(GetType(typeof(Protocol.Challenge)), challengeUrl,
                new RequestParams
                {
                    Method = HttpMethod.Post,
                    Payload = "",
                });
        }

        /// <summary>
        /// Certificate revoke.
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-18#section-7.6"/>
        public async Task CertificateRevokeAsync(Protocol.Messages.RevokeCertificate revokeCertificate)
        {
            Logger.Info("Revoking an ACME certificate. Params:{@params}", revokeCertificate);

            await Request(
                Directory.RevokeCertificate,
                new RequestParams
                {
                    Method = HttpMethod.Post,
                    Payload = revokeCertificate,
                }
            );
        }
    }
}
