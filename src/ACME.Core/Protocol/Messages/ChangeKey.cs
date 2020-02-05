using Newtonsoft.Json;

namespace PeculiarVentures.ACME.Protocol.Messages
{
    /// <summary></summary>
    /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-18#section-7.3.5"/>
    public class ChangeKey : BaseObject
    {
        /// <summary>
        /// The URL for the account being modified.
        /// </summary>
        /// account (required, string)
        [JsonProperty("account")]
        [JsonRequired]
        public string Account { get; set; }

        /// <summary>
        /// The JWK representation of the old key.
        /// </summary>
        /// oldKey (required, JWK)
        [JsonProperty("oldKey")]
        [JsonRequired]
        public string OldKey { get; set; }
    }
}
