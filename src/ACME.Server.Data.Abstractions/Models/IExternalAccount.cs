using System;
using Newtonsoft.Json;

namespace PeculiarVentures.ACME.Server.Data.Abstractions.Models
{
    public interface IExternalAccount : IBaseObject
    {
        string Key { get; set; }

        DateTime? Expires { get; set; }

        object Account { get; set; }
    }
}
