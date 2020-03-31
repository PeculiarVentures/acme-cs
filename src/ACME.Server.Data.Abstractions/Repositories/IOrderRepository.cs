using System.Security.Cryptography.X509Certificates;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;

namespace PeculiarVentures.ACME.Server.Data.Abstractions.Repositories
{
    public interface IOrderRepository : IBaseRepository<IOrder>
    {
        IOrder GetByThumbprint(string thumbprint);
        ICertificate CreateCertificate(X509Certificate2 cert);
        IOrder LastByIdentifier(int accountId, string identifier);
    }
}
