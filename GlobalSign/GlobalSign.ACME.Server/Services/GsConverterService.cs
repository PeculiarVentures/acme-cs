using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;
using PeculiarVentures.ACME.Web;
using PeculiarVentures.ACME.Server.Services;
using GlobalSign.ACME.Server.Data.EF.Core.Models;
using PeculiarVentures.ACME.Server;
using PeculiarVentures.ACME;
using GlobalSign.ACME.Protocol;
using GlobalSign.ACME.Protocol.Messages;

namespace GlobalSign.ACME.Server.Services
{
    public class GsConverterService : ConverterService
    {
        public GsConverterService(
            IAuthorizationRepository authorizationRepository,
            IChallengeRepository challengeRepository,
            IOrderAuthorizationRepository orderAuthorizationRepository,
            IExternalAccountRepository externalAccountRepository,
            IOptions<ServerOptions> options)
            : base(
                  authorizationRepository,
                  challengeRepository,
                  orderAuthorizationRepository,
                  externalAccountRepository,
                  options)
        {
            Types.Add(typeof(PeculiarVentures.ACME.Protocol.Messages.NewOrder), typeof(NewOrder));
            Types.Add(typeof(PeculiarVentures.ACME.Protocol.Messages.FinalizeOrder), typeof(FinalizeOrder));
        }

        protected override PeculiarVentures.ACME.Protocol.Order OnToOrderConvertBefore(IOrder source)
        {
            return new Order();
        }

        protected override PeculiarVentures.ACME.Protocol.Order OnToOrderConvert(PeculiarVentures.ACME.Protocol.Order order, IOrder source, IAuthorization[] authzs)
        {
            var gsOrder = (Order)base.OnToOrderConvert(order, source, authzs);
            var gsSource = (GsOrder)source;
            gsOrder.Template = gsSource.TemplateId == null
                ? null
                : new Uri(new Uri(Options.BaseAddress), $"template/{gsSource.TemplateId}").ToString();
            return gsOrder;
        }

        public ExchangeItem ToExchangeItem(IExchangeItem exchangeItem)
        {
            if (exchangeItem.Key != null)
            {
                return new ExchangeItem
                {
                    Key = new JsonWebKey(exchangeItem.Key),
                };
            }
            else if (exchangeItem.Certificates != null)
            {
                var list = new List<X509Certificate2>();
                foreach (var cert in exchangeItem.Certificates)
                {
                    list.Add(cert);
                }
                return new ExchangeItem
                {
                    CertificateChain = list.Select(o => Convert.ToBase64String(o.RawData)).ToArray(),
                };
            }
            throw new MalformedException($"Bad value of {nameof(IExchangeItem)}");
        }


    }
}
