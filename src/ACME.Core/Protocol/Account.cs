using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PeculiarVentures.ACME.Protocol
{
    /// <summary>
    /// JSON ACME account object.
    /// See <see href="https://tools.ietf.org/html/rfc8555#section-7.1.2">RFC8555</see>
    /// </summary>
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Account : BaseObject
    {
        /// <summary>
        /// The status of the account.
        /// </summary>
        /// status (required, string)
        [JsonProperty("status")]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonRequired]
        public AccountStatus Status { get; set; }

        /// <summary>
        /// An array of URLs that the server can use to contact the client
        /// for issues related to this account
        /// </summary>
        /// contact (optional, array of string)
        [JsonProperty("contact")]
        public string[] Contacts { get; set; }

        /// <summary>
        /// Including this field in a newAccount request, with a value of true,
        /// indicates the client's agreement with the terms of service.
        /// </summary>
        /// <remarks>This field cannot be updated by the client.</remarks>
        /// termsOfServiceAgreed (optional, boolean)
        [JsonProperty("termsOfServiceAgreed")]
        public bool? TermsOfServiceAgreed { get; set; }


        /// <summary>
        /// Including this field in a newAccount request indicates approval
        /// by the holder of an existing non-ACME account to bind that account
        /// to this ACME account.
        /// </summary>
        /// <remarks>This field is not updateable by the client</remarks>
        /// externalAccountBinding (optional, object)
        [JsonProperty("externalAccountBinding")]
        public object ExternalAccountBinding { get; set; }

        /// <summary>
        /// A URL from which a list of orders submitted by this account
        /// can be fetched via a POST-as-GET request.
        /// </summary>
        /// orders (required, string)
        [JsonProperty("orders")]
        [JsonRequired()]
        public string Orders { get; set; }
    }
}
