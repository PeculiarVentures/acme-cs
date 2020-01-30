using System;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;

namespace PeculiarVentures.ACME.Server.Data.Memory.Repositories
{
    public class OrderRepository : BaseRepository<IOrder>, IOrderRepository
    {
    }
}
