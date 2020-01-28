using System;
using Microsoft.AspNetCore.Mvc;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server.Services;
using PeculiarVentures.ACME.Web;
using PeculiarVentures.ACME.Web.Http;

namespace PeculiarVentures.ACME.Server.AspNetCore
{
    [Route("")]
    public class AcmeController : Controller
    {
        public AcmeController(IDirectoryService directoryService)
        {
            DirectoryService = directoryService ?? throw new ArgumentNullException(nameof(directoryService));
        }

        public IDirectoryService DirectoryService { get; }
        public INonceService NonceService { get; }

        public ActionResult CreateActionResult(AcmeResponse response)
        {
            ActionResult result = response.Content == null
                ? new NoContentResult()
                : (ActionResult)new OkObjectResult(response.Content);


            #region Set status code
            Response.StatusCode = response.StatusCode;
            #endregion

            #region Add Link header

            foreach (LinkHeader link in response.Links)
            {
                Response.Headers.Add("Link", link.ToString());
            }
            #endregion

            #region Add Loacation header
            if (response.Location != null)
            {
                Response.Headers.Add("Location", response.Location.ToString());
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
            var response = DirectoryService.GetDirectory();
            return CreateActionResult(response);
        }

        [Route("new-nonce")]
        [HttpGet]
        [HttpHead]
        public ActionResult NewNonce()
        {
            var response = NonceService.NewNonce();
            return CreateActionResult(response);
        }
    }
}
