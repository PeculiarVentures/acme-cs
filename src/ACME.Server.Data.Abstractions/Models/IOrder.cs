using System;
using PeculiarVentures.ACME.Protocol;

namespace PeculiarVentures.ACME.Server.Data.Abstractions.Models
{
    public interface IOrder : IBaseObject
    {
        /// <summary>
        /// The status of this order. Possible values are "pending", "ready",
        /// "processing", "valid", and "invalid".
        /// </summary>
        OrderStatus Status { get; set; }

        /// <summary>
        /// Computed hash of a list of identifiers for quick searching. SHA256(Sort(LowCase(identifiers)))
        /// </summary>
        string Identifier { get; set; }

        /// <summary>
        /// The timestamp after which the server will consider this order invalid,
        /// encoded in the format specified in [RFC3339].  This field is REQUIRED
        /// for objects with "pending" or "valid" in the status field.
        /// </summary>
        DateTime? Expires { get; set; }

        /// <summary>
        /// The requested value of the notBefore field in the certificate.
        /// </summary>
        DateTime? NotBefore { get; set; }

        /// <summary>
        /// The requested value of the notAfter field in the certificate.
        /// </summary>
        DateTime? NotAfter { get; set; }

        /// <summary>
        /// The error that occurred while processing the order, if any.
        /// </summary>
        IError Error { get; set; }

        /// <summary>
        /// Enrolled certificate id
        /// </summary>
        /// <remarks>Controllers layer MUST compleate this value to full URI path</remarks>
        ICertificate Certificate { get; set; }

        /// <summary>
        /// The identifier of account
        /// </summary>
        int AccountId { get; set; }
    }
}
