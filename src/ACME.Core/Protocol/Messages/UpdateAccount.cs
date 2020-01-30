using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PeculiarVentures.ACME.Protocol.Messages
{
    public class UpdateAccount : BaseObject
    {
        /// <summary>
        /// An array of URLs that the server can use to contact the client
        /// for issues related to this account
        /// </summary>
        /// contact (optional, array of string)
        [JsonProperty("contact")]
        public string[] Contacts { get; set; }

        /// <summary>
        /// The status of the account.
        /// </summary>
        /// status (optional, string)
        [JsonProperty("status")]
        [JsonConverter(typeof(StringEnumConverter))]
        public AccountStatus? Status { get; set; } 
    }
}
