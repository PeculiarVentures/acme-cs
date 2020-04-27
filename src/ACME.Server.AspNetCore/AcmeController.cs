using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http.Extensions;
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
                Query = GetQuery(),
                Path = UriHelper.GetDisplayUrl(Request),
            };
        }

        private AcmeRequest GetAcmeRequest()
        {
            return new AcmeRequest()
            {
                Method = Request.Method,
                Query = GetQuery(),
                Path = UriHelper.GetDisplayUrl(Request),
            };
        }

        private Query GetQuery()
        {
            var query = new Query();
            if (Request.QueryString.HasValue)
            {
                foreach (var item in Request.Query)
                {
                    query.Add(item.Key, item.Value);
                }
            }
            return query;
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
            response.Links.Add(
                new LinkHeader(directoryLink.ToString(), new LinkHeaderItem("rel", "index", true)));

            var links = response.Links.Select(o => o.ToString()).ToArray();
            Response.Headers.Add("Link", links);
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
            var response = Controller.GetDirectory(GetAcmeRequest());
            var directory = response.GetContent<Directory>();

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

            return CreateActionResult(response);
        }

        [Route("order/{id:int}")]
        [HttpPost]
        public ActionResult PostOrder([FromBody]JsonWebSignature token, int id)
        {
            var response = Controller.PostOrder(GetAcmeRequest(token), id);

            return CreateActionResult(response);
        }

        [Route("orders")]
        [HttpPost]
        public ActionResult PostOrders([FromBody]JsonWebSignature token)
        {
            var response = Controller.PostOrders(GetAcmeRequest(token));

            ProcessOrders(response);

            return CreateActionResult(response);
        }



        [Route("authz/{id:int}")]
        [HttpPost]
        public ActionResult PostAuthz([FromBody]JsonWebSignature token, int id)
        {
            var response = Controller.PostAuthorization(GetAcmeRequest(token), id);

            return CreateActionResult(response);
        }

        [Route("challenge/{id:int}")]
        [HttpPost]
        public ActionResult PostChallenge([FromBody]JsonWebSignature token, int id)
        {
            var response = Controller.PostChallenge(GetAcmeRequest(token), id);

            return CreateActionResult(response);
        }

        [Route("finalize/{id:int}")]
        [HttpPost]
        public ActionResult Finalize([FromBody]JsonWebSignature token, int id)
        {
            var response = Controller.FinalizeOrder(GetAcmeRequest(token), id);

            return CreateActionResult(response);
        }

        protected void ProcessOrders(AcmeResponse response)
        {
            if (response.Content is Protocol.OrderList orderList)
            {
                orderList.Orders = orderList.Orders.Select(o => $"{BaseUri}order/{o}").ToArray();
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
