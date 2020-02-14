using System;
using System.Collections.Generic;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Server.Data.Abstractions.Models
{
    public interface IAccount : IBaseObject
    {
        AccountStatus Status { get; set; }

        JsonWebKey Key { get; set; }

        bool TermsOfServiceAgreed { get; set; }

        int? ExternalAccountId { get; set; }

        ICollection<string> Contacts { get; set; }
        DateTime CreatedAt { get; set; }
    }
}
