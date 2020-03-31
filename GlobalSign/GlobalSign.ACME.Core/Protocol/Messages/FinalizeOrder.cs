using System.Collections.Generic;
using Newtonsoft.Json;

namespace GlobalSign.ACME.Protocol.Messages
{
    public class FinalizeOrder : PeculiarVentures.ACME.Protocol.Messages.FinalizeOrder
    {
        [JsonProperty("archivedKey")]
        public string ArchivedKey { get; set; }
    }
}
