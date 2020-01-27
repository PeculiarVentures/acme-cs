using System;
using Newtonsoft.Json;

namespace PeculiarVentures.ACME.Protocol
{
    public class DirectoryMetadata : BaseObject
    {
        /// <summary>
        /// A URL identifying the current terms of service
        /// </summary>
        /// termsOfService (optional, string)
        [JsonProperty("termsOfService")]
        public string TermsOfService { get; set; }

        /// <summary>
        /// An HTTP or HTTPS URL locating a website providing more information
        /// about the ACME server
        /// </summary>
        /// website (optional, string)
        [JsonProperty("website")]
        public string Website { get; set; }

        /// <summary>
        /// The hostnames that the ACME server recognizes as referring to itself
        /// for the purposes of CAA record validation
        /// </summary>
        /// <remarks>
        /// Each string MUST represent the same sequence of ASCII code points
        /// that the server will expect to see as the "Issuer Domain Name"
        /// in a CAA issue or issuewild property tag.This allows clients
        /// to determine the correct issuer domain name to use
        /// when configuring CAA records
        /// </remarks>
        /// caaIdentities (optional, array of string)
        [JsonProperty("caaIdentities")]
        public string[] CaaIdentities { get; set; }

        /// <summary>
        /// If this field is present and set to "true", then the CA requires
        /// that all newAccount requests include an "externalAccountBinding"
        /// field associating the new account with an external account
        /// </summary>
        /// externalAccountRequired (optional, boolean)
        [JsonProperty("externalAccountRequired")]
        public bool? ExternalAccountRequired { get; set; }
    }
}
