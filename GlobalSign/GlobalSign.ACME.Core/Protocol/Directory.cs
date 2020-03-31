using System;
using Newtonsoft.Json;

namespace GlobalSign.ACME.Protocol
{
    /// <summary>
    /// JSON ACME Directory
    /// </summary>
    /// <example>
    /// {
    ///   "newNonce": "https://example.com/acme/new-nonce",
    ///   "newAccount": "https://example.com/acme/new-account",
    ///   "newOrder": "https://example.com/acme/new-order",
    ///   "newAuthz": "https://example.com/acme/new-authz",
    ///   "revokeCert": "https://example.com/acme/revoke-cert",
    ///   "keyChange": "https://example.com/acme/key-change",
    ///   "meta": {
    ///     "termsOfService": "https://example.com/acme/terms/2017-5-30",
    ///     "website": "https://www.example.com/",
    ///     "caaIdentities": ["example.com"],
    ///     "externalAccountRequired": false
    ///   }
    /// }
    /// </example>
    public class Directory : PeculiarVentures.ACME.Protocol.Directory
    {
        /// <summary>
        /// Templates
        /// </summary>
        /// gsGetTemplates (optional, string)
        [JsonProperty("gsGetTemplates")]
        public string GsGetTemplates { get; set; }

        /// <summary>
        /// Exchange item
        /// </summary>
        /// gsGetExchange (optional, string)
        [JsonProperty("gsGetExchange")]
        public string GsGetExchange { get; set; }
    }
}
