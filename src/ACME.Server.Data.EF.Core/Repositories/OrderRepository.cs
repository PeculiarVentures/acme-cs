using System.Security.Cryptography.X509Certificates;
using Microsoft.EntityFrameworkCore;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;
using PeculiarVentures.ACME.Server.Data.EF.Core.Models;
using System.Linq;
using System;

namespace PeculiarVentures.ACME.Server.Data.EF.Core.Repositories
{
    public class OrderRepository : BaseRepository<IOrder, Order>, IOrderRepository
    {
        public OrderRepository(
            IOrderAuthorizationRepository orderAuthorizationRepository,
            IAuthorizationRepository authorizationRepository,
            AcmeContext context) : base(context)
        {
            OrderAuthorizationRepository = orderAuthorizationRepository
                ?? throw new ArgumentNullException(nameof(orderAuthorizationRepository));
            AuthorizationRepository = authorizationRepository
                ?? throw new ArgumentNullException(nameof(authorizationRepository));
        }

        public IOrderAuthorizationRepository OrderAuthorizationRepository { get; }
        public IAuthorizationRepository AuthorizationRepository { get; }

        public override DbSet<Order> Records => Context.Orders;

        public ICertificate CreateCertificate(X509Certificate2 cert)
        {
            return new Certificate
            {
                Thumbprint = cert.Thumbprint,
                RawData = cert.RawData,
            };
        }

        public override IOrder GetById(int id)
        {
            return Records
                .FirstOrDefault(o => o.Id == id);
        }

        public IOrder GetByThumbprint(string thumbprint)
        {
            return Records
                .FirstOrDefault(o => o.CertificateValue.Thumbprint == thumbprint);
        }

        public IOrder LastByIdentifier(int accountId, string identifier)
        {
            return Records
                .OrderByDescending(o => o.Id)
                .FirstOrDefault(o => o.Identifier == identifier && o.AccountId == accountId);
        }

        public IOrder LastByTemplate(int accountId, string templateId)
        {
            return Records
                .OrderByDescending(o => o.Id)
                .FirstOrDefault(o => o.TemplateId == templateId && o.AccountId == accountId);
        }
    }
}
