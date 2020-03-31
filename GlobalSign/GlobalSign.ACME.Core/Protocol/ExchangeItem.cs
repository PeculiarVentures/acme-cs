using System;
using Newtonsoft.Json;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Web;

namespace GlobalSign.ACME.Protocol
{
    public class ExchangeItem : BaseObject
    {
        [JsonProperty("jwk")]
        public JsonWebKey Key { get; set; }

        /// <summary>
        /// X509 Certificate chain
        /// </summary>
        /// <see cref="https://tools.ietf.org/html/rfc7515#section-4.1.6"/>
        [JsonProperty("x5c")]
        public string[] CertificateChain { get; set; }
    }
}
