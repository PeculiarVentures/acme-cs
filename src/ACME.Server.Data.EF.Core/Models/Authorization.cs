using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;

namespace PeculiarVentures.ACME.Server.Data.EF.Core.Models
{
    public class Authorization : BaseObject, IAuthorization
    {
        [Column(nameof(Identifier))]
        public Identifier IdentifierValue { get; set; } = new Identifier();

        [NotMapped]
        public virtual IIdentifier Identifier
        {
            get => IdentifierValue;
            set => IdentifierValue = new Identifier(value.Type, value.Value);
        }

        public AuthorizationStatus Status { get; set; }

        public DateTime? Expires { get; set; }

        public bool? Wildcard { get; set; }

        public int AccountId { get; set; }

        [ForeignKey(nameof(AccountId))]
        public Account Account { get; set; }

        public virtual ICollection<Challenge> Challenges { get; set; } = new Collection<Challenge>();
    }
}
