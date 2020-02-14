using System;
using Newtonsoft.Json;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Protocol.Messages
{
    public class NewAccount : BaseObject
    {
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
        public JsonWebSignature ExternalAccountBinding { get; set; }

        /// <summary>
        /// If this field is present
        /// with the value "true", then the server MUST NOT create a new
        /// account if one does not already exist.This allows a client to
        /// look up an account URL based on an account key.
        /// </summary>
        /// onlyReturnExisting (optional, boolean)
        [JsonProperty("onlyReturnExisting")]
        public bool? OnlyReturnExisting { get; set; }
    }
}
