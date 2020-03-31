using System;
using System.Security.Cryptography;
using System.Text;
using GlobalSign.ACME.Server.Data.EF.Core.Models;
using GlobalSign.ACME.Server.Data.EF.Core.Repositories;
using GlobalSign.ACME.Server.Services;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PeculiarVentures.ACME.Helpers;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Protocol.Messages;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Server.Services
{
    public class GsOrderService : OrderService
    {
        private const string IDENTIFIER_HASH = "SHA256";

        public GsOrderService(
            IOrderRepository orderRepository,
            IAccountService accountService,
            IAuthorizationService authorizationService,
            IOrderAuthorizationRepository orderAuthorizationRepository,
            ITemplateService templateService,
            ICertificateEnrollmentService certificateEnrollmentService,
            IOptions<ServerOptions> options)
            : base(
                    orderRepository,
                    accountService,
                    authorizationService,
                    orderAuthorizationRepository,
                    certificateEnrollmentService,
                    options)
        {
            TemplateService = templateService
                ?? throw new ArgumentNullException(nameof(templateService));
        }

        public ITemplateService TemplateService { get; }

        private AsymmetricAlgorithm DecryptArchivedKey(string archivedKey)
        {
            var json = Base64Url.Decode(archivedKey);
            var jwk = JsonConvert.DeserializeObject<JsonWebKey>(Encoding.UTF8.GetString(json));
            return jwk.GetAsymmetricAlgorithm();
        }

        protected override void OnCreatAuth(IOrder order, NewOrder @params)
        {
            var gsParams = (GlobalSign.ACME.Protocol.Messages.NewOrder)@params;
            if (gsParams.GsTemplate != null)
            {
                var gsOrder = (GsOrder)order;
                // Ignore Identifiers if Template presents
                TemplateService.GetById(gsOrder.AccountId, gsParams.GsTemplate);
                gsOrder.TemplateId = gsParams.GsTemplate;
                gsOrder.Expires = DateTime.UtcNow.AddDays(Options.ExpireAuthorizationDays);
                gsOrder.Status = OrderStatus.Ready;
            }
            else
            {
                base.OnCreatAuth(order, @params);
            }
        }

        protected override object OnEnrollCertificateBefore(IOrder order, FinalizeOrder @params)
        {
            var gsOrder = (GsOrder)order;

            AsymmetricAlgorithm key = null;
            if (gsOrder.TemplateId != null)
            {
                var template = TemplateService.GetById(gsOrder.AccountId, gsOrder.TemplateId);
                if (template.Requirements.Archival?.Algorithm != null)
                {
                    var gsParams = (GlobalSign.ACME.Protocol.Messages.FinalizeOrder)@params;
                    if (gsParams.ArchivedKey == null)
                    {
                        throw new MalformedException("Archived key is required");
                    }
                    key = DecryptArchivedKey(gsParams.ArchivedKey);
                }
            }
            return key;
        }

        protected override void OnEnrollCertificateTask(IOrder order, FinalizeOrder @params, object result)
        {
            var key = (AsymmetricAlgorithm)result;

            if (key != null)
            {
                CertificateEnrollmentService.ArchiveKey(order, key);
            }
        }

        protected override IOrder OnGetActualCheckBefore(NewOrder @params, int accountId)
        {
            var gsParams = (GlobalSign.ACME.Protocol.Messages.NewOrder)@params;

            if (gsParams.GsTemplate != null)
            {
                var GsOrderRepository = (GsOrderRepository)OrderRepository;
                var order = GsOrderRepository.LastByTemplate(accountId, gsParams.GsTemplate);
                return order;
            }
            return base.OnGetActualCheckBefore(@params, accountId);
        }

        public IExchangeItem GetExchangeItem(int accountId)
        {
            var gsService = (DefaultGsCertificateEnrollmentService)CertificateEnrollmentService;

            var account = AccountService.GetById(accountId);
            return gsService.GetExchangeItem(account);
        }
    }
}
