using Microsoft.Extensions.Options;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server.Controllers;
using PeculiarVentures.ACME.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace PeculiarVentures.ACME.Server.AspNet
{
    public abstract class AcmeController : ApiController
    {
        protected AcmeController(IAcmeController controller)
        {
            Controller = controller ?? throw new ArgumentNullException(nameof(controller));
        }

        public IAcmeController Controller { get; }

        public Uri BaseUri => new Uri($"{Controller.Options.BaseAddress ?? throw new ArgumentException(nameof(Controller.Options.BaseAddress))}");

        public virtual HttpResponseMessage GetDirectory()
        {
            var response = Controller.GetDirectory(GetAcmeRequest());
            var directory = response.GetContent<Directory>();

            return CreateHttpResponseMessage(response);
        }

        protected HttpResponseMessage CreateHttpResponseMessage(AcmeResponse response)
        {
            HttpResponseMessage result;

            if (response.Content != null)
            {
                if (response.Content is MediaTypeContent content)
                {
                    result = Request.CreateResponse((HttpStatusCode)response.StatusCode);
                    var streamContent = new StreamContent(content.Content);
                    streamContent.Headers.Add("Content-Type", content.Type);
                    result.Content = streamContent;
                }
                else
                {
                    result = Request.CreateResponse((HttpStatusCode)response.StatusCode, response.Content);
                }
            }
            else
            {
                result = Request.CreateResponse(response.StatusCode);
            }

            foreach (var link in response.Headers.Link)
            {
                result.Headers.Add("Link", link.ToString());
            }

            if (response.Headers.Location != null)
            {
                result.Headers.Location = new Uri(response.Headers.Location);
            }

            if (response.Headers.ReplayNonce != null)
            {
                result.Headers.Add("Replay-Nonce", response.Headers.ReplayNonce);
            }

            if (response.Content is Error)
            {
                result.Headers.Add("Content-Type", new string[] { "application/problem+json" });
            }

            return result;
        }

        public virtual HttpResponseMessage NewNonce()
        {
            AcmeRequest acmeRequest = GetAcmeRequest();
            var response = Controller.GetNonce(acmeRequest);
            return CreateHttpResponseMessage(response);
        }

        protected AcmeRequest GetAcmeRequest()
        {
            var headers = new HeaderCollection();
            foreach (var header in Request.Headers)
            {
                headers.Add(header.Key, string.Join(", ", header.Value));
            }
            return new AcmeRequest
            {
                Method = Request.Method.Method,
                Query = GetQuery(),
                Path = Request.RequestUri.ToString(),
                Headers = headers,
            };
        }

        protected AcmeRequest GetAcmeRequest(JsonWebSignature token)
        {
            return new AcmeRequest(token)
            {
                Method = Request.Method.Method,
                Query = GetQuery(),
                Path = Request.RequestUri.ToString(),
            };
        }

        private Query GetQuery()
        {
            var query = new Query();
            var pairQuery = Request.GetQueryNameValuePairs();
            if (pairQuery.Count() > 0)
            {
                foreach (var item in pairQuery)
                {
                    if (query.ContainsKey(item.Key))
                    {
                        var value = query[item.Key];
                        var list = value.ToList();
                        list.Add(item.Value);
                        query[item.Key] = list.ToArray();
                    }
                    else
                    {
                        var value = new string[] { item.Value };
                        query.Add(item.Key, value);
                    }
                }
            }
            return query;
        }

        public virtual HttpResponseMessage NewAccount(JsonWebSignature token)
        {
            var response = Controller.CreateAccount(GetAcmeRequest(token));

            if (response.Headers.Location != null)
            {
                // Complete Location
                response.Headers.Location = new Uri(BaseUri, $"acct/{response.Headers.Location}").ToString();
            }

            return CreateHttpResponseMessage(response);
        }

        public virtual HttpResponseMessage UpdateAccount(JsonWebSignature token, int id)
        {
            var response = Controller.PostAccount(GetAcmeRequest(token));
            return CreateHttpResponseMessage(response);
        }

        public virtual HttpResponseMessage ChangeKey(JsonWebSignature token)
        {
            var response = Controller.ChangeKey(GetAcmeRequest(token));
            return CreateHttpResponseMessage(response);
        }

        public virtual HttpResponseMessage NewOrder(JsonWebSignature token)
        {
            var response = Controller.CreateOrder(GetAcmeRequest(token));

            return CreateHttpResponseMessage(response);
        }

        public virtual HttpResponseMessage PostOrder(JsonWebSignature token, int id)
        {
            var response = Controller.PostOrder(GetAcmeRequest(token), id);

            return CreateHttpResponseMessage(response);
        }

        public virtual HttpResponseMessage PostOrders(JsonWebSignature token)
        {
            var response = Controller.PostOrders(GetAcmeRequest(token));

            return CreateHttpResponseMessage(response);
        }

        public virtual HttpResponseMessage PostAuthz([FromBody]JsonWebSignature token, int id)
        {
            var response = Controller.PostAuthorization(GetAcmeRequest(token), id);

            return CreateHttpResponseMessage(response);
        }

        public virtual HttpResponseMessage PostChallenge([FromBody]JsonWebSignature token, int id)
        {
            var response = Controller.PostChallenge(GetAcmeRequest(token), id);

            return CreateHttpResponseMessage(response);
        }

        public virtual HttpResponseMessage Finalize([FromBody]JsonWebSignature token, int id)
        {
            var response = Controller.FinalizeOrder(GetAcmeRequest(token), id);

            return CreateHttpResponseMessage(response);
        }

        public virtual HttpResponseMessage GetCertificate([FromBody]JsonWebSignature token, string id)
        {
            var response = Controller.GetCertificate(GetAcmeRequest(token), id);

            return CreateHttpResponseMessage(response);
        }

        public virtual HttpResponseMessage RevokeCertificate([FromBody]JsonWebSignature token)
        {
            var response = Controller.RevokeCertificate(GetAcmeRequest(token));

            return CreateHttpResponseMessage(response);
        }
    }
}
