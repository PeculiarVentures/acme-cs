using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PeculiarVentures.ACME.Cryptography;
using PeculiarVentures.ACME.Helpers;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Protocol.Messages;
using PeculiarVentures.ACME.Web;
using Xunit;

namespace PeculiarVentures.ACME.Client
{
    // TODO: Need to write tests for a error states
    public class AcmeClientTests
    {
        private const string _url = "https://acme-staging-v02.api.letsencrypt.org/";
        private const string _contact = "mailto:spark1.dell@gmail.com";
        private const string _domain = "aeg-dev0-srv.aegdomain2.com";

        private readonly Identifier _identifier = new Identifier
        {
            Type = "dns",
            Value = _domain,
        };

        private async Task<AcmeClient> GetClient()
        {
            var key = AsymmetricAlgorithm.Create("RSA");
            key.KeySize = 2048;

            return await AcmeClient.CreateAsync(new Uri(_url), key);
        }

        private async Task CreateUrlTestServer(string challengeToken, JsonWebKey key)
        {
            var http = new HttpClient();
            var sha = SHA256.Create();
            var request = new HttpRequestMessage(HttpMethod.Post, $"http://{_domain}/acme-challenge");

            var newKey = new
            {
                e = key.Exponent,
                kty = "RSA",
                n = key.Modulus,
            };
            var jwkJson = JsonConvert.SerializeObject(newKey, Formatting.None);
            var jwkBytes = Encoding.UTF8.GetBytes(jwkJson);
            var jwkHash = sha.ComputeHash(jwkBytes);
            var jwkThumb = Base64Url.Encode(jwkHash);

            var data = new { id = challengeToken, token = $"{challengeToken}.{jwkThumb}" };
            var payload = JsonConvert.SerializeObject(data, Formatting.None);

            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await http.SendAsync(request);

            if (response.StatusCode != HttpStatusCode.NoContent)
            {
                throw new WebException("Post key authorization error.");
            }
        }

        [Fact]
        public async void ClientCreate()
        {
            var client = await GetClient();

            Assert.NotEmpty(client.Directory.NewOrder);
            Assert.NotEmpty(client.Directory.NewNonce);
            Assert.NotEmpty(client.Directory.NewAccount);
            Assert.NotEmpty(client.Nonce);
        }

        [Fact]
        public async void AccountCreate()
        {
            var client = await GetClient();

            Assert.Null(client.Location);
            var account = await client.AccountCreateAsync(new NewAccount
            {
                TermsOfServiceAgreed = true,
                Contacts = new[] { _contact },
            });
            Assert.NotNull(account);
            Assert.NotNull(client.Location);
            Assert.Equal(AccountStatus.Valid, account.Status);
            Assert.Single(account.Contacts);
        }

        [Fact]
        public async void AccountDeactivate()
        {
            var client = await GetClient();

            Assert.Null(client.Location);
            var account = await client.AccountCreateAsync(new NewAccount
            {
                TermsOfServiceAgreed = true,
                Contacts = new[] { _contact },
            });
            Assert.NotNull(account);
            Assert.NotNull(client.Location);
            Assert.Equal(AccountStatus.Valid, account.Status);
            Assert.Single(account.Contacts);

            account = await client.AccountDeactivateAsync();
            Assert.NotNull(account);
            Assert.Equal(AccountStatus.Deactivated, account.Status);
        }

        [Fact]
        public async void OrderCreate()
        {
            var client = await GetClient();

            await client.AccountCreateAsync(new NewAccount
            {
                TermsOfServiceAgreed = true,
                Contacts = new[] { _contact },
            });

            var identifiers = new List<Identifier> { _identifier };
            var order = await client.OrderCreateAsync(new NewOrder
            {
                Identifiers = identifiers,
            });
            Assert.NotNull(order);
            Assert.Equal(OrderStatus.Pending, order.Status);
            Assert.Equal(identifiers.Count, order.Identifiers.Length);
            Assert.Single(order.Authorizations);
            Assert.NotNull(order.Finalize);
        }

        [Fact]
        public async void AuthorizationCreate()
        {
            var client = await GetClient();

            await client.AccountCreateAsync(new NewAccount
            {
                TermsOfServiceAgreed = true,
                Contacts = new[] { _contact },
            });
            var order = await client.OrderCreateAsync(new NewOrder
            {
                Identifiers = new List<Identifier> { _identifier },
            });

            var authorization = await client.AuthorizationCreateAsync(order.Authorizations[0]);
            Assert.NotNull(authorization);
            Assert.Equal(AuthorizationStatus.Pending, authorization.Status);
            Assert.Equal(order.Identifiers[0].ToString(), authorization.Identifier.ToString());
            Assert.Equal(3, authorization.Challenges.Count);
        }

        [Fact]
        public async void ChallengeGet()
        {
            var client = await GetClient();

            await client.AccountCreateAsync(new NewAccount
            {
                TermsOfServiceAgreed = true,
                Contacts = new[] { _contact },
            });
            var order = await client.OrderCreateAsync(new NewOrder
            {
                Identifiers = new List<Identifier> { _identifier },
            });
            var authorization = await client.AuthorizationCreateAsync(order.Authorizations[0]);

            foreach (var authorizationChallenge in authorization.Challenges)
            {
                if (authorizationChallenge.Type == "http-01")
                {
                    var challenge = await client.ChallengeGetAsync(authorizationChallenge.Url);
                    Assert.NotNull(challenge);
                    Assert.Equal(ChallengeStatus.Pending, challenge.Status);
                    Assert.Equal("http-01", challenge.Type);
                    Assert.NotEmpty(challenge.Url);
                    Assert.NotEmpty(challenge.Token);
                }
            }
        }

        [Fact]
        public async void ChallengeValidate()
        {
            var client = await GetClient();

            var account = await client.AccountCreateAsync(new NewAccount
            {
                TermsOfServiceAgreed = true,
                Contacts = new[] { _contact },
            });
            var order = await client.OrderCreateAsync(new NewOrder
            {
                Identifiers = new List<Identifier> { _identifier },
            });
            var authorization = await client.AuthorizationCreateAsync(order.Authorizations[0]);

            foreach (var authorizationChallenge in authorization.Challenges)
            {
                if (authorizationChallenge.Type == "http-01")
                {
                    await CreateUrlTestServer(authorizationChallenge.Token, account.Key);

                    var challenge = await client.ChallengeValidateAsync(authorizationChallenge.Url);
                    Assert.NotNull(challenge);
                    Assert.Equal(ChallengeStatus.Pending, challenge.Status);
                    Assert.Equal("http-01", challenge.Type);
                    Assert.NotEmpty(challenge.Url);
                    Assert.NotEmpty(challenge.Token);

                    Thread.Sleep(5 * 1000);

                    challenge = await client.ChallengeGetAsync(authorizationChallenge.Url);
                    Assert.NotNull(challenge);
                    Assert.Equal(ChallengeStatus.Valid, challenge.Status);
                }
            }
        }

        [Fact]
        public async void OrderFinalize()
        {
            var client = await GetClient();

            var account = await client.AccountCreateAsync(new NewAccount
            {
                TermsOfServiceAgreed = true,
                Contacts = new[] { _contact },
            });
            var order = await client.OrderCreateAsync(new NewOrder
            {
                Identifiers = new List<Identifier> { _identifier },
            });
            var authorization = await client.AuthorizationCreateAsync(order.Authorizations[0]);

            foreach (var authorizationChallenge in authorization.Challenges)
            {
                if (authorizationChallenge.Type == "http-01")
                {
                    await CreateUrlTestServer(authorizationChallenge.Token, account.Key);

                    var challenge = await client.ChallengeValidateAsync(authorizationChallenge.Url);
                    Assert.NotNull(challenge);
                    Assert.Equal(ChallengeStatus.Pending, challenge.Status);
                    Assert.Equal("http-01", challenge.Type);
                    Assert.NotEmpty(challenge.Url);
                    Assert.NotEmpty(challenge.Token);

                    Thread.Sleep(5 * 1000);

                    await client.ChallengeGetAsync(authorizationChallenge.Url);

                    var param = new CertificateRequestParams
                    {
                        Attributes = new Cryptography.Attribute[] { },
                        CommonName = $"CN={_domain}",
                    };
                    var key = RSA.Create();
                    key.ExportParameters(true);
                    var csr = new Pkcs10CertificateRequest(param, key);
                    var csrDer = Base64Url.Encode(csr.Export());

                    var orderFinal = await client.OrderFinalizeAsync(order.Finalize, csrDer);
                    Assert.Equal(OrderStatus.Valid, orderFinal.Status);
                    Assert.NotEmpty(orderFinal.Certificate);

                    await client.OrderCertificateGetAsync(orderFinal.Certificate);
                }
            }
        }

        [Fact]
        public async void CertificateRevoke()
        {
            var client = await GetClient();

            var account = await client.AccountCreateAsync(new NewAccount
            {
                TermsOfServiceAgreed = true,
                Contacts = new[] { _contact },
            });
            var order = await client.OrderCreateAsync(new NewOrder
            {
                Identifiers = new List<Identifier> { _identifier },
            });
            var authorization = await client.AuthorizationCreateAsync(order.Authorizations[0]);

            foreach (var authorizationChallenge in authorization.Challenges)
            {
                if (authorizationChallenge.Type == "http-01")
                {
                    await CreateUrlTestServer(authorizationChallenge.Token, account.Key);

                    var challenge = await client.ChallengeValidateAsync(authorizationChallenge.Url);
                    Assert.NotNull(challenge);
                    Assert.Equal(ChallengeStatus.Pending, challenge.Status);
                    Assert.Equal("http-01", challenge.Type);
                    Assert.NotEmpty(challenge.Url);
                    Assert.NotEmpty(challenge.Token);

                    Thread.Sleep(5 * 1000);

                    await client.ChallengeGetAsync(authorizationChallenge.Url);

                    var param = new CertificateRequestParams
                    {
                        Attributes = new Cryptography.Attribute[] { },
                        CommonName = $"CN={_domain}",
                    };
                    var key = RSA.Create();
                    key.ExportParameters(true);
                    var csr = new Pkcs10CertificateRequest(param, key);
                    var csrDerBytes = csr.Export();

                    var orderFinal = await client.OrderFinalizeAsync(order.Finalize, Base64Url.Encode(csrDerBytes));
                    Assert.Equal(OrderStatus.Valid, orderFinal.Status);
                    Assert.NotEmpty(orderFinal.Certificate);

                    var certificatePemBytes = await client.OrderCertificateGetAsync(orderFinal.Certificate);
                    var certificate = new X509Certificate2(certificatePemBytes);
                    var certificateDerBytes = certificate.Export(X509ContentType.Cert);

                    await client.CertificateRevokeAsync(Base64Url.Encode(certificateDerBytes));
                }
            }
        }
    }
}
