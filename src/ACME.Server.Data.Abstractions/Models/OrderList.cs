using System;
namespace PeculiarVentures.ACME.Server.Data.Abstractions.Models
{
    public class OrderList : IOrderList
    {
        public IOrder[] Orders { get; set; }
        public bool NextPage { get; set; }
    }
}
