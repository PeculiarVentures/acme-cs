using System;
using Newtonsoft.Json;

namespace PeculiarVentures.ACME.Protocol
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class BaseObject
    {
    }
}
