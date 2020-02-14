using Microsoft.EntityFrameworkCore;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;
using PeculiarVentures.ACME.Server.Data.EF.Core.Models;
using System.Linq;

namespace PeculiarVentures.ACME.Server.Data.EF.Core.Repositories
{
    public class OrderAuthorizationRepository : BaseRepository<IOrderAuthorization, OrderAuthorization>, IOrderAuthorizationRepository
    {
        public OrderAuthorizationRepository(AcmeContext context) : base(context)
        {
        }

        public override DbSet<OrderAuthorization> Records => Context.OrderAuthorization;

        public IOrderAuthorization Create(int orderId, int authzId)
        {
            var item = Create();
            item.OrderId = orderId;
            item.AuthorizationId = authzId;
            return item;
        }

        public IOrderAuthorization[] GetByAuthorization(int authzId)
        {
            return Records.Where(o => o.AuthorizationId == authzId).ToArray();
        }

        public override IOrderAuthorization GetById(int id)
        {
            return Records.FirstOrDefault(o => o.Id == id);
        }

        public IOrderAuthorization[] GetByOrder(int orderId)
        {
            return Records.Where(o => o.OrderId == orderId).ToArray();
        }
    }
}
