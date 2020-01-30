using System;
using Microsoft.AspNetCore.Mvc;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server.Controllers;
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

        public Uri BaseUri => new Uri($"{Request.Scheme}://{Request.Host.Value}{Request.Path.Value}");

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
            directory.NewAccount ??= new Uri(BaseUri, "new-order").ToString();

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
                // Compleate Location
                response.Location = new Uri(BaseUri, response.Location).ToString();
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
    }
}
