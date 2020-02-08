using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;

namespace PeculiarVentures.ACME.Server.Data.Memory.Repositories
{
    public class OrderRepository : BaseRepository<IOrder>, IOrderRepository
    {
        public override IOrder Create()
        {
            return new Models.Order();
        }

        public ICertificate CreateCertificate(X509Certificate2 testCert)
        {
            return new Models.Certificate
            {
                RawData = testCert.RawData,
                Thumbprint = testCert.Thumbprint,
            };
        }

        public IOrder GetByIdentifiers(int accountId, Identifier[] identifiers)
        {
            if (identifiers is null)
            {
                throw new ArgumentNullException(nameof(identifiers));
            }

            var oderedIdentifiers = identifiers
                .OrderBy(o => o.Type)
                .OrderBy(o => o.Value)
                .ToArray();

            return Items
                .Where(o => o.AccountId == accountId)
                .Where(o => o.Authorizations
                    .Select(a => new Identifier(a.Identifier.Type, a.Identifier.Value))
                    .OrderBy(a => a.Type)
                    .OrderBy(a => a.Value)
                    .SequenceEqual(oderedIdentifiers))
                .LastOrDefault();
        }
    }
}
