using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PeculiarVentures.ACME.Json.Converters;

namespace PeculiarVentures.ACME.Protocol
{
    /// <summary>
    /// JSON ACME account object.
    /// See <see href="https://tools.ietf.org/html/rfc8555#section-7.1.3">RFC8555</see>
    /// </summary>
    public class Order : BaseObject
    {
        /// <summary>
        /// The status of this order. Possible values are "pending", "ready",
        /// "processing", "valid", and "invalid".
        /// </summary>
        /// status (required, string)
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("status")]
        [JsonRequired]
        public OrderStatus Status { get; set; }

        /// <summary>
        /// The timestamp after which the server will consider this order invalid,
        /// encoded in the format specified in [RFC3339].  This field is REQUIRED
        /// for objects with "pending" or "valid" in the status field.
        /// </summary>
        /// expires (optional, string)
        [JsonConverter(typeof(DateTimeFormatConverter))]
        [JsonProperty("expires")]
        public DateTime? Expires { get; set; }

        /// <summary>
        /// An array of identifier objects that the order pertains to
        /// </summary>
        /// identifiers (required, array of object)
        [JsonProperty("identifiers")]
        [JsonRequired]
        public List<Identifier> Identifiers { get; set; } = new List<Identifier>();

        /// <summary>
        /// The requested value of the notBefore field in the certificate.
        /// </summary>
        /// notBefore (optional, string) in the date format defined in [RFC3339]
        [JsonConverter(typeof(DateTimeFormatConverter))]
        [JsonProperty("notBefore")]
        public DateTime? NotBefore { get; set; }

        /// <summary>
        /// The requested value of the notAfter field in the certificate.
        /// </summary>
        /// notAfter (optional, string) in the date format defined in [RFC3339]
        [JsonConverter(typeof(DateTimeFormatConverter))]
        [JsonProperty("notAfter")]
        public DateTime? NotAfter { get; set; }

        /// <summary>
        /// The error that occurred while processing the order, if any.
        /// </summary>
        /// error (optional, object)
        [JsonProperty("error")]
        public Error Error { get; set; }

        /// <summary>
        /// An array of authorization objects
        /// </summary>
        /// authorizations (required, array of string)
        [JsonProperty("authorizations")]
        [JsonRequired]
        public List<string> Authorizations { get; set; } = new List<string>();

        /// <summary>
        /// A URL that a CSR must be POSTed to once
        /// all of the order's authorizations are satisfied to finalize the
        /// order.The result of a successful finalization will be the
        /// population of the certificate URL for the order.
        /// </summary>
        /// finalize (required, string)
        [JsonProperty("finalize")]
        public string Finalize { get; set; }

        /// <summary>
        /// A URL for the certificate that has been issued in response to this order.
        /// </summary>
        /// certificate (optional, string)
        [JsonProperty("certificate")]
        public string Certificate { get; set; }
    }
}
