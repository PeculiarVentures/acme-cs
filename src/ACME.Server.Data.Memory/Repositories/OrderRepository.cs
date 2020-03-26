using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;

namespace PeculiarVentures.ACME.Server.Data.Memory.Repositories
{
    public class OrderRepository : BaseRepository<IOrder>, IOrderRepository
    {
        public OrderRepository()
        {
        }

        public override IOrder Create()
        {
            return new Models.Order();
        }

        public ICertificate CreateCertificate(X509Certificate2 cert)
        {
            return new Models.Certificate
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

        public IOrder LastByTemplate(int accountId, string templateId)
        {
            return Items
                .LastOrDefault(o => o.AccountId == accountId && o.TemplateId == templateId);
        }
    }
}
