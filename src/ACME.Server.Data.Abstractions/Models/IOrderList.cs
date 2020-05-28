namespace PeculiarVentures.ACME.Server.Data.Abstractions.Models
{
    public interface IOrderList
    {
        /// <summary>
        /// Array of orders
        /// </summary>
        IOrder[] Orders { get; set; }

        /// <summary>
        /// Next page flag
        /// </summary>
        bool NextPage { get; set; }
    }
}
