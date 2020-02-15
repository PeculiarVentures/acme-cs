using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PeculiarVentures.ACME.Protocol;

namespace PeculiarVentures.ACME.Server.Data.Abstractions.Models
{
    public interface IExternalAccount : IBaseObject
    {
        string Key { get; set; }

        DateTime? Expires { get; set; }

        object Account { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        ExternalAccountStatus Status { get; set; }
    }
}
