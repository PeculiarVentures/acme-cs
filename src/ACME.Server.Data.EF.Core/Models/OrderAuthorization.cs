using System.ComponentModel.DataAnnotations.Schema;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;

namespace PeculiarVentures.ACME.Server.Data.EF.Core.Models
{
    public class OrderAuthorization : BaseObject, IOrderAuthorization
    {
        public int AuthorizationId { get; set; }

        [ForeignKey(nameof(AuthorizationId))]
        public virtual Authorization Authorization { get; set; }

        public int OrderId { get; set; }

        [ForeignKey(nameof(OrderId))]
        public virtual Order Order { get; set; }
    }
}
