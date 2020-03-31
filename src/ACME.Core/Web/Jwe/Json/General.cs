using Newtonsoft.Json;

namespace PeculiarVentures.ACME.Web.Jwe.Json
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class JsonGeneral
    {
        [JsonProperty("protected")]
        public string ProtectedHeader { get; set; }

        [JsonProperty("unprotected")]
        public object UnprotectedHeader { get; set; }

        [JsonProperty("aad")]
        public string AdditionalAuthenticatedData { get; set; }

        [JsonProperty("recipients")]
        public JsonRecipientCollection Recipients { get; set; }

        [JsonProperty("iv")]
        public string InitializationVector { get; set; }

        [JsonProperty("ciphertext")]
        public string CipherText { get; set; }

        [JsonProperty("tag")]
        public string AuthenticationTag { get; set; }

        [JsonProperty("encrypted_key")]
        public string EncryptedKey { get; set; }
    }
}
