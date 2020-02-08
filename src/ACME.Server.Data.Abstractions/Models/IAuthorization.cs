using System;
using System.Collections.Generic;
using PeculiarVentures.ACME.Protocol;

namespace PeculiarVentures.ACME.Server.Data.Abstractions.Models
{
    public interface IAuthorization : IBaseObject
    {
        /// <summary>
        /// The identifier that the account is authorized to represent.
        /// </summary>
        IIdentifier Identifier { get; set; }

        /// <summary>
        /// The status of this authorization.
        /// </summary>
        public AuthorizationStatus Status { get; set; }

        /// <summary>
        /// The timestamp after which the server will consider this authorization invalid
        /// </summary>
        public DateTime? Expires { get; set; }

        /// <summary>
        /// An array of challenges
        /// </summary>
        public ICollection<IChallenge> Challenges { get; set; }

        /// <summary>
        /// This field MUST be present and true
        /// for authorizations created as a result of a newOrder request
        /// containing a DNS identifier with a value that was a wildcard
        /// domain name.For other authorizations, it MUST be absent
        /// </summary>
        public bool? Wildcard { get; set; }

        public int AccountId { get; set; }
    }
}
