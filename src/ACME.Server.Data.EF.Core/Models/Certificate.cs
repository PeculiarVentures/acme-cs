using Microsoft.EntityFrameworkCore;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;

namespace PeculiarVentures.ACME.Server.Data.EF.Core.Models
{
    [Owned]
    public class Certificate : ICertificate
    {
        public string Thumbprint { get; set; }

        public byte[] RawData { get; set; }

        public bool Revoked { get; set; }
    }
}
