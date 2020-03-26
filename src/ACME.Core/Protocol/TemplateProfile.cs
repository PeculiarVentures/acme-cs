using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PeculiarVentures.ACME.Protocol
{
    public class TemplateProfile : BaseObject
    {
        /// <summary>
        /// Validity period
        /// </summary>
        /// validityPerod (required, number)
        [JsonProperty("validityPeriod")]
        [JsonRequired]
        public int ValidityPeriod { get; set; }

        /// <summary>
        /// EKUs
        /// </summary>
        /// ekus (required, string[])
        [JsonProperty("ekus")]
        [JsonRequired]
        public string[] EKUs { get; set; }

        /// <summary>
        /// Identifiers
        /// </summary>
        /// identifiers (required, string[])
        [JsonProperty("identifiers", ItemConverterType = typeof(StringEnumConverter))]
        //[JsonRequired]
        public List<TemplateIdentifierType> Identifiers { get; set; }
    }
}