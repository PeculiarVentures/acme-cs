using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PeculiarVentures.ACME.Protocol;

namespace GlobalSign.ACME.Protocol
{
    public class TemplateRequirementIdentifier : BaseObject
    {
        /// <summary>
        /// Type
        /// </summary>
        /// type (required, string)
        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonRequired]
        public TemplateIdentifierType Type { get; set; }

        /// <summary>
        /// Presence
        /// </summary>
        /// presence (optional, string)
        [JsonProperty("presence")]
        [JsonConverter(typeof(StringEnumConverter))]
        public TemplateIdentifierPresence? Presence { get; set; }
    }
}