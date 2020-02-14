using System;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;

namespace PeculiarVentures.ACME.Server.Data.Memory.Models
{
    public class Certificate : BaseObject, ICertificate
    {
        public string Thumbprint { get; set; }
        public byte[] RawData { get; set; }
        public bool Revoked { get; set; }
    }
}
