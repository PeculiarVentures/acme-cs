using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PeculiarVentures.ACME.Server.Data.Abstractions.Repositories
{
    public interface IOrderAuthorizationRepository : IBaseRepository<IOrderAuthorization>
    {
        IOrderAuthorization[] GetByOrder(int orderId);
        IOrderAuthorization[] GetByAuthorization(int authzId);
        IOrderAuthorization Create(int orderId, int authzId);
    }
}
