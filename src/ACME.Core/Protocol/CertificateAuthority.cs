using Newtonsoft.Json;

namespace PeculiarVentures.ACME.Protocol
{
    public class CertificateAuthority : BaseObject
    {
        /// <summary>
        /// Authority key identifier
        /// </summary>
        /// aki (required, object)
        [JsonProperty("aki")]
        [JsonRequired]
        public  KeyIdentifier AuthorityKeyIdentifier { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        /// name (required, string)
        [JsonProperty("name")]
        [JsonRequired]
        public string Name { get; set; }

        /// <summary>
        /// Url
        /// </summary>
        /// url (required, string)
        [JsonProperty("url")]
        [JsonRequired]
        public string Url { get; set; }
    }
}