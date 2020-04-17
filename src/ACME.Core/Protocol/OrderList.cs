using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PeculiarVentures.ACME.Json.Converters;

namespace PeculiarVentures.ACME.Protocol
{
    public class OrderList : BaseObject
    {
        [JsonProperty("orders")]
        [JsonRequired]
        public string[] Orders { get; set; } = new string[0];
    }
}
