using System.Security.Cryptography.X509Certificates;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Server.Data.Abstractions.Repositories
{
    public interface IOrderRepository : IBaseRepository<IOrder>
    {
        IOrder GetByThumbprint(string thumbprint);
        ICertificate CreateCertificate(X509Certificate2 cert);
        IOrder LastByIdentifier(int accountId, string identifier);
        IOrderList GetList(int accountId, Query page, int size);
    }
}
