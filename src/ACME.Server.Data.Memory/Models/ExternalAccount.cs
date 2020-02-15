using System;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;

namespace PeculiarVentures.ACME.Server.Data.Memory.Models
{
    public class ExternalAccount : BaseObject, IExternalAccount
    {
        public string Key { get; set; }
        public DateTime? Expires { get; set; }
        public object Account { get; set; }
        public ExternalAccountStatus Status { get; set; }
    }
}
