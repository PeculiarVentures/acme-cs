using System;
using System.Collections.Generic;
using System.Linq;
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
        private const string _directory = "https://acme-staging-v02.api.letsencrypt.org/directory";
        //private const string _directory = "https://localhost:5003/directory";
        private const string _contact = "mailto:spark1.dell@gmail.com";
        private const string _domain = "aeg-dev0-srv.aegdomain2.com";
        //private const string _domain = "localhost:5003.com";

        private readonly Identifier _identifier = new Identifier
        {
            Type = "dns",
            Value = _domain,
        };

        private async Task<AcmeClient> GetClient()
        {
            var key = RSA.Create(2048);

            return await AcmeClient.CreateAsync(new Uri(_directory), key);
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

        /*
        // TODO: for this test need server with getOrders
        [Fact]
        public async void GetOrders()
        {
            var client = await GetClient();
            var account = await client.AccountCreateAsync(new NewAccount
            {
                TermsOfServiceAgreed = true,
                Contacts = new[] { _contact },
            });
            var identifiers = new List<Identifier> { _identifier };
            await client.OrderCreateAsync(new NewOrder
            {
                Identifiers = identifiers,
            });
            await client.OrderCreateAsync(new NewOrder
            {
                Identifiers = new List<Identifier> { new Identifier { Value = "googl.com", Type = "dns" } },
            });

            var orders = await client.OrderListGetAsync(account.Content.Orders);
            var orders2 = await client.OrderListGetAsync($"{account.Content.Orders}?cursor=1");
            var urlNext = orders.Links.FindUrl("next");
            //var nextLink = orders.Links.FirstOrDefault(o => o.Items == "rel=\"next\"");
            //if (nextLink != null)
            //{
            //    var orders3 = await client.OrderListGetAsync(nextLink.Url.ToString());
            //}
            var stop = 5;
        }
        */

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
            Assert.Equal(AccountStatus.Valid, account.Content.Status);
            Assert.Single(account.Content.Contacts);
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
            Assert.Equal(AccountStatus.Valid, account.Content.Status);
            Assert.Single(account.Content.Contacts);

            account = await client.AccountDeactivateAsync();
            Assert.NotNull(account);
            Assert.Equal(AccountStatus.Deactivated, account.Content.Status);
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
            Assert.Equal(OrderStatus.Pending, order.Content.Status);
            Assert.Equal(identifiers.Count, order.Content.Identifiers.Length);
            Assert.Single(order.Content.Authorizations);
            Assert.NotNull(order.Content.Finalize);
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

            var authorization = await client.AuthorizationCreateAsync(order.Content.Authorizations[0]);
            Assert.NotNull(authorization);
            Assert.Equal(AuthorizationStatus.Pending, authorization.Content.Status);
            Assert.Equal(order.Content.Identifiers[0].ToString(), authorization.Content.Identifier.ToString());
            Assert.Equal(3, authorization.Content.Challenges.Count);
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
            var authorization = await client.AuthorizationCreateAsync(order.Content.Authorizations[0]);

            foreach (var authorizationChallenge in authorization.Content.Challenges)
            {
                if (authorizationChallenge.Type == "http-01")
                {
                    var challenge = await client.ChallengeGetAsync(authorizationChallenge.Url);
                    Assert.NotNull(challenge);
                    Assert.Equal(ChallengeStatus.Pending, challenge.Content.Status);
                    Assert.Equal("http-01", challenge.Content.Type);
                    Assert.NotEmpty(challenge.Content.Url);
                    Assert.NotEmpty(challenge.Content.Token);
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
            var authorization = await client.AuthorizationCreateAsync(order.Content.Authorizations[0]);

            foreach (var authorizationChallenge in authorization.Content.Challenges)
            {
                if (authorizationChallenge.Type == "http-01")
                {
                    await CreateUrlTestServer(authorizationChallenge.Token, account.Content.Key);

                    var challenge = await client.ChallengeValidateAsync(authorizationChallenge.Url);
                    Assert.NotNull(challenge);
                    Assert.Equal(ChallengeStatus.Pending, challenge.Content.Status);
                    Assert.Equal("http-01", challenge.Content.Type);
                    Assert.NotEmpty(challenge.Content.Url);
                    Assert.NotEmpty(challenge.Content.Token);

                    Thread.Sleep(5 * 1000);

                    challenge = await client.ChallengeGetAsync(authorizationChallenge.Url);
                    Assert.NotNull(challenge);
                    Assert.Equal(ChallengeStatus.Valid, challenge.Content.Status);
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
            var authorization = await client.AuthorizationCreateAsync(order.Content.Authorizations[0]);

            foreach (var authorizationChallenge in authorization.Content.Challenges)
            {
                if (authorizationChallenge.Type == "http-01")
                {
                    await CreateUrlTestServer(authorizationChallenge.Token, account.Content.Key);

                    var challenge = await client.ChallengeValidateAsync(authorizationChallenge.Url);
                    Assert.NotNull(challenge);
                    Assert.Equal(ChallengeStatus.Pending, challenge.Content.Status);
                    Assert.Equal("http-01", challenge.Content.Type);
                    Assert.NotEmpty(challenge.Content.Url);
                    Assert.NotEmpty(challenge.Content.Token);

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

                    var orderFinal = await client.OrderFinalizeAsync(order.Content.Finalize, new FinalizeOrder { Csr = csrDer });
                    Assert.Equal(OrderStatus.Valid, orderFinal.Content.Status);
                    Assert.NotEmpty(orderFinal.Content.Certificate);

                    await client.OrderCertificateGetAsync(orderFinal.Content.Certificate);
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
            var authorization = await client.AuthorizationCreateAsync(order.Content.Authorizations[0]);

            foreach (var authorizationChallenge in authorization.Content.Challenges)
            {
                if (authorizationChallenge.Type == "http-01")
                {
                    await CreateUrlTestServer(authorizationChallenge.Token, account.Content.Key);

                    var challenge = await client.ChallengeValidateAsync(authorizationChallenge.Url);
                    Assert.NotNull(challenge);
                    Assert.Equal(ChallengeStatus.Pending, challenge.Content.Status);
                    Assert.Equal("http-01", challenge.Content.Type);
                    Assert.NotEmpty(challenge.Content.Url);
                    Assert.NotEmpty(challenge.Content.Token);

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

                    var orderFinal = await client.OrderFinalizeAsync(order.Content.Finalize, new FinalizeOrder { Csr = Base64Url.Encode(csr.Export()) });
                    Assert.Equal(OrderStatus.Valid, orderFinal.Content.Status);
                    Assert.NotEmpty(orderFinal.Content.Certificate);

                    var certificatePemBytes = await client.OrderCertificateGetAsync(orderFinal.Content.Certificate);
                    var certificate = new X509Certificate2(certificatePemBytes);

                    await client.CertificateRevokeAsync(new RevokeCertificate { Certificate = Base64Url.Encode(certificate.Export(X509ContentType.Cert)) });
                }
            }
        }
    }
}
