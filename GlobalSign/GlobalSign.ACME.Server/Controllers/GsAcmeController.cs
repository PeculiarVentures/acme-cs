using System;
using GlobalSign.ACME.Server.Services;
using Microsoft.Extensions.Options;
using PeculiarVentures.ACME.Server;
using PeculiarVentures.ACME.Server.Controllers;
using PeculiarVentures.ACME.Server.Services;
using PeculiarVentures.ACME.Web;

namespace GlobalSign.ACME.Server.Controllers
{
    public class GsAcmeController : AcmeController
    {
        public GsAcmeController(
            IDirectoryService directoryService,
            INonceService nonceService,
            IAccountService accountService,
            IOrderService orderService,
            IChallengeService challengeService,
            IAuthorizationService authorizationService,
            IConverterService converterService,
            ITemplateService templateService,
            IOptions<ServerOptions> options) : base(
                directoryService,
                nonceService,
                accountService,
                orderService,
                challengeService,
                authorizationService,
                converterService,
                options)
        {
            TemplateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
        }

        public ITemplateService TemplateService { get; }

        public AcmeResponse GetTemplates(AcmeRequest request)
        {
            return WrapAction((response) =>
            {
                var account = GetAccount(request.KeyId);
                response.Content = TemplateService.GetTemplates(account.Id);
            }, request);
        }

        public AcmeResponse GetExchangeItem(AcmeRequest request)
        {
            return WrapAction((response) =>
            {
                var GsOrderService = (GsOrderService)OrderService;
                var GsConverterService = (GsConverterService)ConverterService;

                var account = GetAccount(request.KeyId);
                var exchangeItem = GsOrderService.GetExchangeItem(account.Id);
                response.Content = GsConverterService.ToExchangeItem(exchangeItem);
            }, request);
        }
    }
}
