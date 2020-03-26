using System;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;

namespace PeculiarVentures.ACME.Server.Data.Memory.Models
{
    public class Order : BaseObject, IOrder
    {
        public OrderStatus Status { get; set; }
        public DateTime? Expires { get; set; }
        public DateTime? NotBefore { get; set; }
        public DateTime? NotAfter { get; set; }
        public IError Error { get; set; }
        public int AccountId { get; set; }
        public ICertificate Certificate { get; set; }
        public string Identifier { get; set; }
        public string TemplateId { get; set; }
    }
}
