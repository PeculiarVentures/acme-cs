using System.Security.Cryptography.X509Certificates;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Server.Data.Abstractions.Repositories
{
    public interface IOrderRepository : IBaseRepository<IOrder>
    {
        /// <summary>
        /// Returns the order by thumbprint of certificate
        /// </summary>
        /// <param name="thumbprint">The thumbprint of certificate</param>
        /// <returns></returns>
        IOrder GetByThumbprint(string thumbprint);

        /// <summary>
        /// Creates and returns certificate
        /// </summary>
        /// <param name="cert"></param>
        /// <returns></returns>
        ICertificate CreateCertificate(X509Certificate2 cert);

        /// <summary>
        /// Returns the last order by identifier
        /// </summary>
        /// <param name="accountId">The identifier of account</param>
        /// <param name="identifier">The identifier of order</param>
        /// <returns></returns>
        IOrder LastByIdentifier(int accountId, string identifier);

        /// <summary>
        /// Returns the orders list
        /// </summary>
        /// <param name="accountId">The identifier of account</param>
        /// <param name="page">Page</param>
        /// <param name="size">Amount of elements in page</param>
        /// <returns></returns>
        IOrderList GetList(int accountId, Query page, int size);
    }
}
