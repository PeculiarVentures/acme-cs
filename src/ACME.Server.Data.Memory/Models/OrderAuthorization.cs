using PeculiarVentures.ACME.Server.Data.Abstractions.Models;

namespace PeculiarVentures.ACME.Server.Data.Memory.Models
{
    public class OrderAuthorization : BaseObject, IOrderAuthorization
    {
        public int AuthorizationId { get; set; }
        public int OrderId { get; set; }
    }
}
