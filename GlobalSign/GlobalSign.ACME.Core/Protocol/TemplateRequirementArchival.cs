using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Web;

namespace GlobalSign.ACME.Protocol
{
    public class TemplateRequirementArchival : BaseObject
    {
        /// <summary>
        /// Algorithm
        /// </summary>
        /// alg (required, string)
        [JsonProperty("alg")]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonRequired]
        public AlgorithmsEnum Algorithm { get; set; }
    }
}
