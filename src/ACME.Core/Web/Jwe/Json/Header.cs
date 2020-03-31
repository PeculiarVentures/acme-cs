using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PeculiarVentures.ACME.Web.Jwe.Json
{
    /// <summary>
    /// 
    /// </summary>
    /// <see cref="https://tools.ietf.org/html/rfc7516#section-10.1"/>
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class JsonHeader
    {
        [JsonProperty("alg")]
        [JsonConverter(typeof(StringEnumConverter))]
        public Algorithms? Algorithm { get; set; }

        [JsonProperty("enc")]
        [JsonConverter(typeof(StringEnumConverter))]
        public Encryptions? EncryptionAlgorithm { get; set; }

        [JsonProperty("zip")]
        [JsonConverter(typeof(StringEnumConverter))]
        public Compressions? CompressionAlgorithm { get; set; }

        [JsonProperty("jku")]
        public string KeyUrl { get; set; }

        [JsonProperty("jwk")]
        public JsonWebKey JsonWebKey { get; set; }

        [JsonProperty("kid")]
        public string KeyID { get; set; }

        [JsonProperty("x5u")]
        public string X509URL { get; set; }

        [JsonProperty("x5c")]
        public string X509CertificateChain { get; set; }

        [JsonProperty("x5t")]
        public string X509CertificateSHA1Thumbprint { get; set; }

        [JsonProperty("x5t#S256")]
        public string X509CertificateSHA256Thumbprint { get; set; }

        [JsonProperty("typ")]
        public string Type { get; set; }

        [JsonProperty("cty")]
        public string ContentType { get; set; }

        [JsonProperty("crit")]
        public string[] Critical { get; set; }
    }
}
