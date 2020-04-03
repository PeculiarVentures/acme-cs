using System.Security.Cryptography.X509Certificates;
using Microsoft.EntityFrameworkCore;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;
using PeculiarVentures.ACME.Server.Data.EF.Core.Models;
using System.Linq;
using System;
using PeculiarVentures.ACME.Protocol;

namespace PeculiarVentures.ACME.Server.Data.EF.Core.Repositories
{
    public class OrderRepository : BaseRepository<IOrder, Models.Order>, IOrderRepository
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

        public override DbSet<Models.Order> Records => Context.Orders;

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

        public IOrderList GetList(int accountId, int page, int size)
        {
            var items = Records.Where(o => o.AccountId == accountId);
            items = items.Skip(page * size);
            var count = items.Count();
            var orders = items.Take(size).ToArray();

            return new Models.OrderList
            {
                Orders = orders,
                NextPage = count > size,
            };
        }

    }
}
