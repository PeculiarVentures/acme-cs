using System.Collections.Generic;
using Newtonsoft.Json;

namespace PeculiarVentures.ACME.Protocol
{
    public class TemplateRequirements : BaseObject
    {
        /// <summary>
        /// Signatute
        /// </summary>
        /// sig (required, object)
        [JsonProperty("sig")]
        [JsonRequired]
        public TemplateRequirementSignature Signatute { get; set; } = new TemplateRequirementSignature();

        /// <summary>
        /// Identifiers
        /// </summary>
        /// identifiers (required, object[])
        [JsonProperty("identifiers")]
        [JsonRequired]
        public List<TemplateRequirementIdentifier> Identifiers { get; set; } = new List<TemplateRequirementIdentifier>();

        /// <summary>
        /// Archival
        /// </summary>
        /// archival (required, object)
        [JsonProperty("archival")]
        public TemplateRequirementArchival Archival { get; set; }
    }
}