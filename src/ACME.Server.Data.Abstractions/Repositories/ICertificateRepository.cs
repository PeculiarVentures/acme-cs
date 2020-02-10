using System;
using System.Security.Cryptography.X509Certificates;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;

namespace PeculiarVentures.ACME.Server.Data.Abstractions.Repositories
{
    public interface ICertificateRepository : IBaseRepository<ICertificate>
    {
        ICertificate Create(X509Certificate2 cert);
        ICertificate GetByThumbprint(string thumbprint);
    }
}
