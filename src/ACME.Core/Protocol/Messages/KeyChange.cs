using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Protocol.Messages
{
    public class KeyChange : BaseObject
    {
        [JsonProperty("account")]
        [JsonRequired]
        public string Account { get; set; }

        [JsonProperty("oldKey")]
        [JsonRequired]
        public JsonWebKey Key { get; set; }
    }
}
