using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PeculiarVentures.ACME.Server.Data.Abstractions.Repositories
{
    public interface IOrderAuthorizationRepository : IBaseRepository<IOrderAuthorization>
    {
        /// <summary>
        /// Returns array of order authorizations by identifier of order
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        IOrderAuthorization[] GetByOrder(int orderId);

        /// <summary>
        /// Returns array of order authorizations by identifier of authorization
        /// </summary>
        /// <param name="authzId"></param>
        /// <returns></returns>
        IOrderAuthorization[] GetByAuthorization(int authzId);

        /// <summary>
        /// Creates and returns new order authorization
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="authzId"></param>
        /// <returns></returns>
        IOrderAuthorization Create(int orderId, int authzId);
    }
}
