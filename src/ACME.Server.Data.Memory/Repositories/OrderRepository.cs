using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;
using PeculiarVentures.ACME.Server.Data.Memory.Models;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Server.Data.Memory.Repositories
{
    public class OrderRepository : BaseRepository<IOrder>, IOrderRepository
    {
        public OrderRepository()
        {
        }

        public override IOrder Create()
        {
            return new Order();
        }

        public ICertificate CreateCertificate(X509Certificate2 cert)
        {
            return new Certificate
            {
                RawData = cert.RawData,
                Thumbprint = cert.Thumbprint,
            };
        }

        public IOrder LastByIdentifier(int accountId, string identifier)
        {
            return Items.LastOrDefault(o => o.Identifier == identifier && o.AccountId == accountId);
        }

        public IOrder GetByThumbprint(string thumbprint)
        {
            return Items
                .FirstOrDefault(o => o.Certificate != null && o.Certificate.Thumbprint.Equals(thumbprint, StringComparison.CurrentCultureIgnoreCase));
        }

        public IOrderList GetList(int accountId, Query @params, int size)
        {
            var items = Items.Where(o => o.AccountId == accountId);

            int page = 0;
            if (@params.ContainsKey("cursor"))
            {
                page = int.Parse(@params["cursor"].FirstOrDefault());
            }

            items = items.Skip(page * size);
            var count = items.Count();
            var orders = items.Take(size).ToArray();

            return new OrderList {
                Orders = orders,
                NextPage = count > size,
            };
        }
    }
}
