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
            var response = Controller.GetDirectory();
            var directory = response.GetContent<Directory>();

            directory.NewNonce = directory.NewNonce ?? new Uri(BaseUri, "new-nonce").ToString();
            directory.NewAccount = directory.NewAccount ?? new Uri(BaseUri, "new-acct").ToString();
            directory.NewOrder = directory.NewOrder ?? new Uri(BaseUri, "new-order").ToString();
            directory.RevokeCertificate = directory.RevokeCertificate ?? new Uri(BaseUri, "revoke").ToString();

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

            foreach (var link in response.Links)
            {
                result.Headers.Add("Link", link.ToString());
            }

            if (response.Location != null)
            {
                result.Headers.Location = new Uri(response.Location);
            }

            if (response.ReplayNonce != null)
            {
                result.Headers.Add("Replay-Nonce", response.ReplayNonce);
            }

            return result;
        }

        public virtual HttpResponseMessage NewNonce()
        {
            AcmeRequest acmeRequest = GetAcmeRequest();
            var response = Controller.GetNonce(acmeRequest);
            return CreateHttpResponseMessage(response);
        }

        private AcmeRequest GetAcmeRequest()
        {
            return new AcmeRequest
            {
                Method = Request.Method.Method,
            };
        }

        protected AcmeRequest GetAcmeRequest(JsonWebSignature token)
        {
            return new AcmeRequest(token)
            {
                Method = Request.Method.Method,
            };
        }

        public virtual HttpResponseMessage NewAccount(JsonWebSignature token)
        {
            var response = Controller.CreateAccount(GetAcmeRequest(token));

            if (response.Location != null)
            {
                // Complete Location
                response.Location = new Uri(BaseUri, $"acct/{response.Location}").ToString();
            }

            return CreateHttpResponseMessage(response);
        }

        public virtual HttpResponseMessage UpdateAccount(JsonWebSignature token, int id)
        {
            var response = Controller.PostAccount(GetAcmeRequest(token));
            return CreateHttpResponseMessage(response);
        }

        public virtual HttpResponseMessage NewOrder(JsonWebSignature token)
        {
            var response = Controller.CreateOrder(GetAcmeRequest(token));

            if (response.Location != null)
            {
                // Complete Location
                response.Location = new Uri(BaseUri, $"order/{response.Location}").ToString();
            }

            ProcessOrder(response);

            return CreateHttpResponseMessage(response);
        }

        public virtual HttpResponseMessage PostOrder(JsonWebSignature token, int id)
        {
            var response = Controller.PostOrder(GetAcmeRequest(token), id);

            ProcessOrder(response);

            return CreateHttpResponseMessage(response);
        }

        private void ProcessOrder(AcmeResponse response)
        {
            if (response.Content is Order order)
            {
                order.Authorizations = order.Authorizations.Select(o => $"{BaseUri}authz/{o}").ToArray();
                order.Finalize = $"{BaseUri}finalize/{order.Finalize}";
                if (order.Certificate != null)
                {
                    order.Certificate = $"{BaseUri}cert/{order.Certificate}";
                }
            }
        }

        public virtual HttpResponseMessage PostAuthz([FromBody]JsonWebSignature token, int id)
        {
            var response = Controller.PostAuthorization(GetAcmeRequest(token), id);

            // fix URLs
            if (response.Content is Protocol.Authorization authz)
            {
                foreach (var challenge in authz.Challenges)
                {
                    challenge.Url = $"{BaseUri}challenge/{challenge.Url}";
                }
            }

            return CreateHttpResponseMessage(response);
        }

        public virtual HttpResponseMessage PostChallenge([FromBody]JsonWebSignature token, int id)
        {
            var response = Controller.PostChallenge(GetAcmeRequest(token), id);

            // fix URLs
            if (response.Content is Challenge challenge)
            {
                challenge.Url = $"{BaseUri}challenge/{challenge.Url}";
            }

            return CreateHttpResponseMessage(response);
        }

        public virtual HttpResponseMessage Finalize([FromBody]JsonWebSignature token, int id)
        {
            var response = Controller.FinalizeOrder(GetAcmeRequest(token), id);

            ProcessOrder(response);

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
