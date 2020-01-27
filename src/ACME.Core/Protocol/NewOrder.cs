using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PeculiarVentures.ACME.Json.Converters;

namespace PeculiarVentures.ACME.Protocol
{
    public class NewOrder : BaseObject
    {
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
    }
}
