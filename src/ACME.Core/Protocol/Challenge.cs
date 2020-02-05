using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PeculiarVentures.ACME.Json.Converters;

namespace PeculiarVentures.ACME.Protocol
{
    public class Challenge : BaseObject
    {
        /// <summary>
        /// The type of challenge encoded in the object.
        /// </summary>
        /// type (required, string)
        [JsonProperty("type")]
        [JsonRequired]
        public string Type { get; set; }

        /// <summary>
        /// The URL to which a response can be posted.
        /// </summary>
        /// url (required, string)
        [JsonProperty("url")]
        [JsonRequired]
        public string Url { get; set; }

        /// <summary>
        /// The status of this challenge.
        /// </summary>
        /// status (required, string)
        [JsonProperty("status")]
        [JsonRequired]
        [JsonConverter(typeof(StringEnumConverter))]
        public ChallengeStatus Status { get; set; }

        /// <summary>
        /// The time at which the server validated this challenge.
        /// </summary>
        /// validated (optional, string)
        [JsonProperty("validated")]
        public DateTime? Validated { get; set; }

        /// <summary>
        /// Error that occurred while the server was validating the challenge.
        /// </summary>
        /// error (optional, object)
        [JsonProperty("error")]
        public Error Error { get; set; }

        /// <summary>
        /// A random value that uniquely identifies the challenge.
        /// </summary>
        /// token (required, string)
        [JsonProperty("token")]
        [JsonRequired]
        public string Token { get; set; }
    }
}