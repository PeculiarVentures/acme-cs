using System;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;

namespace PeculiarVentures.ACME.Server.Data.EF.Core.Models
{
    public class OrderList : IOrderList
    {
        public IOrder[] Orders { get; set; }
        public bool NextPage { get; set; }
    }
}
