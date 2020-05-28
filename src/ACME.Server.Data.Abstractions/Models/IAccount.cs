using System;
using System.Collections.Generic;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Server.Data.Abstractions.Models
{
    public interface IAccount : IBaseObject
    {
        /// <summary>
        /// The status of this account
        /// </summary>
        AccountStatus Status { get; set; }

        /// <summary>
        /// JWK key
        /// </summary>
        JsonWebKey Key { get; set; }

        /// <summary>
        /// Agreement with the terms of service
        /// </summary>
        bool? TermsOfServiceAgreed { get; set; }

        /// <summary>
        /// External account identifier
        /// </summary>
        int? ExternalAccountId { get; set; }

        /// <summary>
        /// An array of contact
        /// </summary>
        ICollection<string> Contacts { get; set; }

        /// <summary>
        /// Account creation date
        /// </summary>
        DateTime CreatedAt { get; set; }
    }
}
