using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PeculiarVentures.ACME.Json.Converters;

namespace PeculiarVentures.ACME.Protocol
{
    public class Authorization : BaseObject
    {
        /// <summary>
        /// The identifier that the account is authorized to represent.
        /// </summary>
        /// identifier (required, object)
        [JsonProperty("identifier")]
        [JsonRequired]
        public Identifier Identifier { get; set; } = new Identifier();

        /// <summary>
        /// The status of this authorization.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonRequired]
        public AuthorizationStatus Status { get; set; }

        /// <summary>
        /// The timestamp after which the server will consider this authorization invalid
        /// </summary>
        /// expires (optional, string)
        [JsonProperty("expires")]
        public DateTime? Expires { get; set; }

        /// <summary>
        /// An array of challenges
        /// </summary>
        /// challenges (required, array of objects)
        [JsonProperty("challenges")]
        [JsonRequired]
        public List<Challenge> Challenges { get; set; }

        /// <summary>
        /// This field MUST be present and true
        /// for authorizations created as a result of a newOrder request
        /// containing a DNS identifier with a value that was a wildcard
        /// domain name.For other authorizations, it MUST be absent
        /// </summary>
        /// wildcard (optional, boolean)
        [JsonProperty("wildcard")]
        public bool? Wildcard { get; set; }
    }
}
