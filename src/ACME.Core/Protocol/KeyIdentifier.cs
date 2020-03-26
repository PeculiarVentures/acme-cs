using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Protocol
{
    public class KeyIdentifier: BaseObject
    {
        /// <summary>
        /// Algorithm
        /// </summary>
        /// alg (required, string)
        [JsonProperty("alg")]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonRequired]
        public AlgorithmsEnum Algorithm { get; set; }

        /// <summary>
        /// Identifier
        /// </summary>
        /// identifier (required, string)
        [JsonProperty("identifier")]
        [JsonRequired]
        public string Identifier { get; set; }
    }
}