using System;
using PeculiarVentures.ACME.Protocol;

namespace PeculiarVentures.ACME.Server.Data.Abstractions.Models
{
    public interface IAuthorization : IBaseObject, IAccountId
    {
        /// <summary>
        /// The identifier that the account is authorized to represent.
        /// </summary>
        IIdentifier Identifier { get; set; }

        /// <summary>
        /// The status of this authorization.
        /// </summary>
        AuthorizationStatus Status { get; set; }

        /// <summary>
        /// The timestamp after which the server will consider this authorization invalid
        /// </summary>
        DateTime? Expires { get; set; }

        /// <summary>
        /// This field MUST be present and true
        /// for authorizations created as a result of a newOrder request
        /// containing a DNS identifier with a value that was a wildcard
        /// domain name.For other authorizations, it MUST be absent
        /// </summary>
        bool? Wildcard { get; set; }

        /// <summary>
        /// The identifier of account
        /// </summary>
        int AccountId { get; set; }
    }
}
