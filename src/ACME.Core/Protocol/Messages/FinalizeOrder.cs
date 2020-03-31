using System.Collections.Generic;
using Newtonsoft.Json;

namespace PeculiarVentures.ACME.Protocol.Messages
{
    public class FinalizeOrder : BaseObject
    {
        /// <summary>
        /// A CSR encoding the parameters for the certificate being requested.
        /// </summary>
        /// csr (required, string)
        [JsonProperty("csr")]
        [JsonRequired]
        public string Csr { get; set; }
    }
}
