using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;
using PeculiarVentures.ACME.Server.Data.Memory.Models;

namespace PeculiarVentures.ACME.Server.Data.Memory.Repositories
{
    public class CertificateRepository : BaseRepository<ICertificate>, ICertificateRepository
    {
        public override ICertificate Create()
        {
            return new Certificate();
        }

        public ICertificate Create(X509Certificate2 cert)
        {
            return new Certificate
            {
                RawData = cert.RawData,
                Thumbprint = cert.Thumbprint,
            };
        }

        public ICertificate GetByThumbprint(string thumbprint)
        {
            if (thumbprint is null)
            {
                throw new ArgumentNullException(nameof(thumbprint));
            }

            return Items.FirstOrDefault(o => o.Thumbprint.Equals(thumbprint, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}
