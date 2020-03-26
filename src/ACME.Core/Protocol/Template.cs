using System.Collections.Generic;
using Newtonsoft.Json;

namespace PeculiarVentures.ACME.Protocol
{
    public class Template : BaseObject
    {
        /// <summary>
        /// CAs
        /// </summary>
        /// cas (required, object[])
        [JsonProperty("cas")]
        //[JsonRequired]
        public List<KeyIdentifier> Cas { get; set; }

        /// <summary>
        /// Identifier
        /// </summary>
        /// identifier (required, string)
        [JsonProperty("identifier")]
        [JsonRequired]
        public string Identifier { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        /// name (required, string)
        [JsonProperty("name")]
        [JsonRequired]
        public string Name { get; set; }

        /// <summary>
        /// Profile
        /// </summary>
        /// profile (required, object)
        [JsonProperty("profile")]
        [JsonRequired]
        public TemplateProfile Profile { get; set; } = new TemplateProfile();

        /// <summary>
        /// Requirements
        /// </summary>
        /// requirements (required, object)
        [JsonProperty("requirements")]
        [JsonRequired]
        public TemplateRequirements Requirements { get; set; } = new TemplateRequirements();
    }
}