namespace PeculiarVentures.ACME.Server.Data.Abstractions.Models
{
    public interface IOrderAuthorization : IBaseObject
    {
        /// <summary>
        /// The identifier of authorization
        /// </summary>
        int AuthorizationId { get; set; }

        /// <summary>
        /// The identifier of order
        /// </summary>
        int OrderId { get; set; }
    }
}
