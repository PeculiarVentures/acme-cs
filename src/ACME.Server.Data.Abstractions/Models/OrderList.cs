namespace PeculiarVentures.ACME.Server.Data.Abstractions.Models
{
    /// <summary>
    /// List of orders
    /// </summary>
    public class OrderList : IOrderList
    {
        public IOrder[] Orders { get; set; }
        public bool NextPage { get; set; }
    }
}
