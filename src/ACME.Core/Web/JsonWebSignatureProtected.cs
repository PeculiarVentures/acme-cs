using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PeculiarVentures.ACME.Web
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class JsonWebSignatureProtected
    {
        [JsonProperty("alg")]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonRequired]
        public AlgorithmsEnum Algorithm { get; set; }

        [JsonProperty("jwk")]
        public JsonWebKey Key { get; set; }

        [JsonProperty("kid")]
        public string KeyID { get; set; }

        [JsonProperty("nonce")]
        public string Nonce { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
