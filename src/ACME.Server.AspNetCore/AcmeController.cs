using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server.Controllers;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Web;
using PeculiarVentures.ACME.Web.Http;

namespace PeculiarVentures.ACME.Server.AspNetCore
{
    [Route("")]
    [ApiController]
    public class AcmeController : Controller
    {
        public AcmeController(IAcmeController controller)
        {
            Controller = controller ?? throw new ArgumentNullException(nameof(controller));
        }

        public Uri BaseUri => new Uri($"{Request.Scheme}://{Request.Host.Value}");

        public IAcmeController Controller { get; }

        private AcmeRequest GetAcmeRequest(JsonWebSignature token)
        {
            return new AcmeRequest(token)
            {
                Method = Request.Method,
            };
        }

        private AcmeRequest GetAcmeRequest()
        {
            return new AcmeRequest()
            {
                Method = Request.Method,
            };
        }

        protected ActionResult CreateActionResult(AcmeResponse response)
        {
            // TODO NewNoce must return 200 status for HEAD request
            ActionResult result = response.Content == null
                ? new NoContentResult()
                : (ActionResult)new OkObjectResult(response.Content)
                {
                    StatusCode = response.StatusCode,
                };

            #region Add Link header
            var directoryLink = new Uri(BaseUri, "directory");
            Response.Headers.Add(
                "Link",
                new LinkHeader(directoryLink.ToString(), new LinkHeaderItem("rel", "index", true)).ToString());

            foreach (LinkHeader link in response.Links)
            {
                Response.Headers.Add("Link", link.ToString());
            }
            #endregion

            #region Add Loacation header
            if (response.Location != null)
            {
                Response.Headers.Add("Location", response.Location);
            }
            #endregion

            #region Add Replay-Nonce header
            if (response.ReplayNonce != null)
            {
                Response.Headers.Add("Replay-Nonce", response.ReplayNonce);
            }
            #endregion

            return result;
        }

        [Route("directory")]
        [HttpGet]
        public ActionResult GetDirectory()
        {
            var response = Controller.GetDirectory();
            var directory = response.GetContent<Directory>();

            directory.NewNonce ??= new Uri(BaseUri, "new-nonce").ToString();
            directory.NewAccount ??= new Uri(BaseUri, "new-acct").ToString();
            directory.NewOrder ??= new Uri(BaseUri, "new-order").ToString();
            directory.RevokeCertificate ??= new Uri(BaseUri, "revoke").ToString();

            return CreateActionResult(response);
        }

        [Route("new-nonce")]
        [HttpGet]
        [HttpHead]
        public ActionResult NewNonce()
        {
            var response = Controller.GetNonce(GetAcmeRequest());
            return CreateActionResult(response);
        }

        [Route("new-acct")]
        [HttpPost]
        public ActionResult CreateAccount([FromBody]JsonWebSignature token)
        {
            var response = Controller.CreateAccount(GetAcmeRequest(token));

            if (response.Location != null)
            {
                // Complete Location
                response.Location = new Uri(BaseUri, $"acct/{response.Location}").ToString();
            }

            return CreateActionResult(response);
        }

        [Route("acct/{id:int}")]
        [HttpPost]
        public ActionResult Update([FromBody]JsonWebSignature token)
        {
            var response = Controller.PostAccount(GetAcmeRequest(token));
            return CreateActionResult(response);
        }

        [Route("new-order")]
        [HttpPost]
        public ActionResult NewOrder([FromBody]JsonWebSignature token)
        {
            var response = Controller.CreateOrder(GetAcmeRequest(token));

            if (response.Location != null)
            {
                // Complete Location
                response.Location = new Uri(BaseUri, $"order/{response.Location}").ToString();
            }

            ProcessOrder(response);

            return CreateActionResult(response);
        }

        [Route("order/{id:int}")]
        [HttpPost]
        public ActionResult PostOrder([FromBody]JsonWebSignature token, int id)
        {
            var response = Controller.PostOrder(GetAcmeRequest(token), id);

            ProcessOrder(response);

            return CreateActionResult(response);
        }

        [Route("authz/{id:int}")]
        [HttpPost]
        public ActionResult PostAuthz([FromBody]JsonWebSignature token, int id)
        {
            var response = Controller.PostAuthorization(GetAcmeRequest(token), id);

            // fix URLs
            if (response.Content is Authorization authz)
            {
                foreach (var challenge in authz.Challenges)
                {
                    challenge.Url = $"{BaseUri}challenge/{challenge.Url}";
                }
            }

            return CreateActionResult(response);
        }

        [Route("challenge/{id:int}")]
        [HttpPost]
        public ActionResult PostChallenge([FromBody]JsonWebSignature token, int id)
        {
            var response = Controller.PostChallenge(GetAcmeRequest(token), id);

            // fix URLs
            if (response.Content is Challenge challenge)
            {
                challenge.Url = $"{BaseUri}challenge/{challenge.Url}";
            }

            return CreateActionResult(response);
        }

        [Route("finalize/{id:int}")]
        [HttpPost]
        public ActionResult Finalize([FromBody]JsonWebSignature token, int id)
        {
            var response = Controller.FinalizeOrder(GetAcmeRequest(token), id);

            ProcessOrder(response);

            return CreateActionResult(response);
        }

        protected void ProcessOrder(AcmeResponse response)
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

        [Route("cert/{id}")]
        [HttpPost]
        public ActionResult GetCertificate([FromBody]JsonWebSignature token, string id)
        {
            var response = Controller.GetCertificate(GetAcmeRequest(token), id);

            if (response.Content is MediaTypeContent content)
            {
                return new FileStreamResult(content.Content, content.Type);
            }
            else
            {
                return CreateActionResult(response);
            }

        }

        [Route("revoke")]
        [HttpPost]
        public ActionResult RevokeCertificate([FromBody]JsonWebSignature token)
        {
            var response = Controller.RevokeCertificate(GetAcmeRequest(token));

            return CreateActionResult(response);
        }
    }
}
