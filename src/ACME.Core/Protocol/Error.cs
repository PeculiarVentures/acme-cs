using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PeculiarVentures.ACME.Protocol
{
    public class Error : BaseObject
    {
        [JsonProperty("type")]
        [JsonRequired]
        [JsonConverter(typeof(StringEnumConverter))]
        public ErrorType Type { get; set; }

        [JsonProperty("detail")]
        public string Detail { get; set; }

        [JsonProperty("subproblems")]
        public Error[] SubPropems { get; set; }
    }
}