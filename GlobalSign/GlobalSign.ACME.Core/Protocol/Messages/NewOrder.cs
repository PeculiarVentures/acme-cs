using System;
using Newtonsoft.Json;

namespace GlobalSign.ACME.Protocol.Messages
{
    public class NewOrder : PeculiarVentures.ACME.Protocol.Messages.NewOrder
    {
        /// <summary>
        /// Template identifier (GlobalSign)
        /// </summary>
        [JsonProperty("gsTemplate")]
        public string GsTemplate { get; set; }
    }
}
