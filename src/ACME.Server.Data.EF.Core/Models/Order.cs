using System;
using System.ComponentModel.DataAnnotations.Schema;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;

namespace PeculiarVentures.ACME.Server.Data.EF.Core.Models
{
    public class Order : BaseObject, IOrder
    {
        public OrderStatus Status { get; set; }

        public DateTime? Expires { get; set; }

        public DateTime? NotBefore { get; set; }

        public DateTime? NotAfter { get; set; }

        public int? ErrorId { get; set; }

        [ForeignKey(nameof(ErrorId))]
        public virtual Error ErrorValue { get; set; }

        [NotMapped]
        public virtual IError Error
        {
            get => ErrorValue;
            set => ErrorValue = (Error)value;
        }

        [Column(nameof(Certificate))]
        public Certificate CertificateValue { get; set; } = new Certificate();

        [NotMapped]
        public virtual ICertificate Certificate
        {
            get => CertificateValue;
            set => CertificateValue = (Certificate)value;
        }

        public int AccountId { get; set; }

        [ForeignKey(nameof(AccountId))]
        public virtual Account Account { get; set; }

        public string Identifier { get; set; }
    }
}
