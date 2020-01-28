using System;
using Microsoft.AspNetCore.Mvc;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server.Services;
using PeculiarVentures.ACME.Web;
using PeculiarVentures.ACME.Web.Http;

namespace PeculiarVentures.ACME.Server.AspNetCore
{
    [Route("")]
    [ApiController]
    public class AcmeController : Controller
    {
        public Uri BaseUri => new Uri(Url.ActionLink());

        public AcmeController(
            IDirectoryService directoryService,
            INonceService nonceService,
            IAccountService accountService
            )
        {
            DirectoryService = directoryService ?? throw new ArgumentNullException(nameof(directoryService));
            NonceService = nonceService ?? throw new ArgumentNullException(nameof(nonceService));
            AccountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        }

        public IDirectoryService DirectoryService { get; }
        public INonceService NonceService { get; }
        public IAccountService AccountService { get; }

        protected ActionResult CreateActionResult(AcmeResponse response)
        {
            ActionResult result = response.Content == null
                ? new NoContentResult()
                : (ActionResult)new OkObjectResult(response.Content);


            #region Set status code
            Response.StatusCode = response.StatusCode;
            #endregion

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
            var response = DirectoryService.GetDirectory();
            var directory = response.GetContent<Directory>();

            directory.NewNonce = new Uri(BaseUri, "new-nonce").ToString();
            directory.NewAccount = new Uri(BaseUri, "new-acct").ToString();

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

        [Route("new-acct")]
        [HttpPost]
        public ActionResult Create([FromBody]JsonWebSignature token)
        {
            var response = AccountService.Create(new AcmeRequest(token));
            if (response.Location != null)
            {
                response.Location = new Uri(BaseUri, response.Location).ToString();
            }
            return CreateActionResult(response);
        }

        [Route("acct/{id:int}")]
        [HttpPost]
        public ActionResult Update([FromBody]JsonWebSignature token)
        {
            var response = AccountService.Update(new AcmeRequest(token));
            return CreateActionResult(response);
        }
    }
}
