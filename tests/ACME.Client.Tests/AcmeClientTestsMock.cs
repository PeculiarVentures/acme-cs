//using System;
//using System.Net;
//using System.Net.Http;
//using System.Security.Cryptography;
//using System.Threading.Tasks;
//using Newtonsoft.Json;
//using PeculiarVentures.ACME.Web;
//using RichardSzalay.MockHttp;
//using Xunit;

//namespace PeculiarVentures.ACME.Client
//{
//    public class AcmeClientTestsMock
//    {
//        private const string _url = "http://localhost/";

//        private HttpClient GetHttp()
//        {
//            var mockHttp = new MockHttpMessageHandler();
//            var http = mockHttp.ToHttpClient();
//            http.BaseAddress = new Uri(_url);

//            var dir = new Protocol.Directory
//            {
//                NewNonce = $"{_url}acme/new-nonce",
//                NewOrder = $"{_url}acme/new-order",
//                NewAccount = $"{_url}acme/new-account",
//                Meta = new Protocol.DirectoryMetadata(),
//            };

//            mockHttp
//                .When(HttpMethod.Get, $"{_url}directory")
//                .Respond(HttpStatusCode.OK, "application/json", JsonConvert.SerializeObject(dir));
//            mockHttp
//                .When(HttpMethod.Head, dir.NewNonce)
//                .Respond(_ =>
//                {
//                    var res = new HttpResponseMessage(HttpStatusCode.OK);

//                    res.Headers.Add("replay-nonce", "nonce-1");

//                    return res;
//                });
//            mockHttp
//                .Fallback.WithAny()
//                .Respond(HttpStatusCode.BadRequest);

//            return http;
//        }

//        private async Task<AcmeClient> GetClient()
//        {
//            var http = GetHttp();
//            var key = AsymmetricAlgorithm.Create("RSA");
//            key.KeySize = 2048;

//            return await AcmeClient.CreateAsync(http, key);
//        }

//        [Fact]
//        public async void ClientCreate()
//        {
//            var client = await GetClient();

//            Assert.NotEmpty(client.Directory.NewOrder);
//            Assert.NotEmpty(client.Directory.NewNonce);
//            Assert.NotEmpty(client.Directory.NewAccount);
//            Assert.NotEmpty(client.Nonce);
//        }
//    }
//}
