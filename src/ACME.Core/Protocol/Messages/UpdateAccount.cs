using System;
using Newtonsoft.Json;

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
    }
}
