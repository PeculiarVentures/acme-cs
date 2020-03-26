using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Protocol
{
    public class TemplateRequirementSignature : BaseObject
    {
        /// <summary>
        /// Algorithms
        /// </summary>
        /// alg (optional, string[])
        [JsonProperty("alg", ItemConverterType = typeof(StringEnumConverter))]
        public List<AlgorithmsEnum> Algorithms { get; set; }

        /// <summary>
        /// Modules
        /// </summary>
        /// n (optional, number[])
        [JsonProperty("n")]
        public int[] Modules { get; set; }

        /// <summary>
        /// EllipticCurves
        /// </summary>
        /// crv (optional, string[])
        [JsonProperty("crv", ItemConverterType = typeof(StringEnumConverter))]
        public List<EllipticCurvesEnum> EllipticCurves { get; set; }

        /// <summary>
        /// Use
        /// </summary>
        /// use (required, string[])
        [JsonProperty("use")]
        public string[] Use { get; set; }
    }
}
