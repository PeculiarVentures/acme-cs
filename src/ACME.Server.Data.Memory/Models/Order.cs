using System;
using System.Collections.Generic;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;

namespace PeculiarVentures.ACME.Server.Data.Memory.Models
{
    public class Order : BaseObject, IOrder
    {
        public OrderStatus Status { get; set; }
        public DateTime? Expires { get; set; }
        public ICollection<IIdentifier> Identifiers { get; set; } = new List<IIdentifier>();
        public DateTime? NotBefore { get; set; }
        public DateTime? NotAfter { get; set; }
        public IError Error { get; set; }
        public ICollection<IAuthorization> Authorizations { get; set; } = new List<IAuthorization>();
        public int AccountId { get; set; }
        public ICertificate Certificate { get; set; }
    }
}
