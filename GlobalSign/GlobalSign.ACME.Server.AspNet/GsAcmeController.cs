using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server.Controllers;
using PeculiarVentures.ACME.Web;
using System;
using System.Net.Http;
using System.Web.Http;

namespace GlobalSign.ACME.Server.AspNet
{
    public abstract class GsAcmeController : PeculiarVentures.ACME.Server.AspNet.AcmeController
    {
        protected GsAcmeController(IAcmeController controller) : base(controller) { }

        public override HttpResponseMessage GetDirectory()
        {
            var response = Controller.GetDirectory();
            var directory = response.GetContent<Directory>();

            return CreateHttpResponseMessage(response);
        }

        public virtual HttpResponseMessage GetTemplates([FromBody]JsonWebSignature token)
        {
            var gsController = (Controllers.GsAcmeController)Controller;
            var response = gsController.GetTemplates(GetAcmeRequest(token));

            return CreateHttpResponseMessage(response);
        }

        public virtual HttpResponseMessage GetExchangeItem([FromBody]JsonWebSignature token)
        {
            var gsController = (Controllers.GsAcmeController)Controller;
            var response = gsController.GetExchangeItem(GetAcmeRequest(token));

            return CreateHttpResponseMessage(response);
        }
    }
}
