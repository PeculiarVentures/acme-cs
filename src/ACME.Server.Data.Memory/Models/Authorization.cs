using System;
using System.Collections.Generic;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;

namespace PeculiarVentures.ACME.Server.Data.Memory.Models
{
    public class Authorization : IAuthorization
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public IIdentifier Identifier { get; set; } = new Identifier();
        public AuthorizationStatus Status { get; set; }
        public DateTime? Expires { get; set; }
        public ICollection<IChallenge> Challenges { get; set; } = new List<IChallenge>();
        public bool? Wildcard { get; set; }
    }
}
