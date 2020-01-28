using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PeculiarVentures.ACME.Protocol.Messages
{
    public class DeactivateAccount : BaseObject
    {
        /// <summary>
        /// The status of the account.
        /// </summary>
        /// status (required, string)
        [JsonProperty("status")]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonRequired]
        public AccountStatus Status { get; set; } = AccountStatus.Deactivated;
    }
}
