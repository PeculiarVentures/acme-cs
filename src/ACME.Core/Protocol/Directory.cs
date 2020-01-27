using System;
using Newtonsoft.Json;

namespace PeculiarVentures.ACME.Protocol
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
    public class Directory : BaseObject
    {
        /// <summary>
        /// New nonce.
        /// </summary>
        [JsonProperty("newNonce")]
        public string NewNonce { get; set; }

        /// <summary>
        /// New account.
        /// </summary>
        [JsonProperty("newAccount")]
        public string NewAccount { get; set; }

        /// <summary>
        /// New order.
        /// </summary>
        [JsonProperty("newOrder")]
        public string NewOrder { get; set; }

        /// <summary>
        /// New authorization
        /// </summary>
        [JsonProperty("newAuthz")]
        public string NewAuthorization { get; set; }

        /// <summary>
        /// Revoke certificate
        /// </summary>
        [JsonProperty("revokeCert")]
        public string RevokeCertificate { get; set; }

        /// <summary>
        /// Key change
        /// </summary>
        [JsonProperty("keyChange")]
        public string KeyChange { get; set; }

        /// <summary>
        /// Metadata object
        /// </summary>
        /// meta (optional, object)
        [JsonProperty("meta")]
        public DirectoryMetadata Meta { get; set; }

    }
}
