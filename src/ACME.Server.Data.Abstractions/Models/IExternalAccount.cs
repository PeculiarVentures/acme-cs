using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PeculiarVentures.ACME.Protocol;

namespace PeculiarVentures.ACME.Server.Data.Abstractions.Models
{
    public interface IExternalAccount : IBaseObject
    {
        /// <summary>
        /// Base64 encoded one-time password
        /// </summary>
        string Key { get; set; }

        /// <summary>
        /// Expires
        /// </summary>
        DateTime? Expires { get; set; }

        /// <summary>
        /// Data of this external account
        /// </summary>
        object Account { get; set; }

        /// <summary>
        /// The status of this external account
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        ExternalAccountStatus Status { get; set; }
    }
}
