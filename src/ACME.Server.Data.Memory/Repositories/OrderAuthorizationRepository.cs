using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;
using PeculiarVentures.ACME.Server.Data.Memory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeculiarVentures.ACME.Server.Data.Memory.Repositories
{
    public class OrderAuthorizationRepository : BaseRepository<IOrderAuthorization>, IOrderAuthorizationRepository
    {
        public IOrderAuthorization Create(int orderId, int authzId)
        {
            var res = Create();
            res.OrderId = orderId;
            res.AuthorizationId = authzId;
            return res;
        }

        public override IOrderAuthorization Create()
        {
            return new OrderAuthorization();
        }

        public IOrderAuthorization[] GetByAuthorization(int authzId)
        {
            return Items.Where(o => o.AuthorizationId == authzId).ToArray();
        }

        public IOrderAuthorization[] GetByOrder(int orderId)
        {
            return Items.Where(o => o.OrderId == orderId).ToArray();
        }
    }
}
