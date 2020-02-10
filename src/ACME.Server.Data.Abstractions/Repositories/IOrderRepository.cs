using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;

namespace PeculiarVentures.ACME.Server.Data.Abstractions.Repositories
{
    public interface IOrderRepository : IBaseRepository<IOrder>
    {
        /// <summary>
        /// Returns the last actual order by a list of identifiers
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="identifiers"></param>
        /// <returns>Order or null</returns>
        IOrder GetByIdentifiers(int accountId, Identifier[] identifiers);
    }
}
