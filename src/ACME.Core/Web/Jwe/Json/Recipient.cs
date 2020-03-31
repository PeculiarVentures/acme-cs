using Newtonsoft.Json;

namespace PeculiarVentures.ACME.Web.Jwe.Json
{
    /// <summary>
    /// 
    /// </summary>
    /// <see cref="https://tools.ietf.org/html/rfc7516#section-7.2.1"/>
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class JsonRecipient
    {
        [JsonProperty("header")]
        public JsonHeader Header { get; set; }

        [JsonProperty("encrypted_key")]
        public string EncryptedKey { get; set; }
    }
}
