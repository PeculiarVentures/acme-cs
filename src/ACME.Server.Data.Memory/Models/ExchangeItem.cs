using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;

namespace PeculiarVentures.ACME.Server.Data.Memory.Models
{
    public class ExchangeItem : IExchangeItem
    {
        public AsymmetricAlgorithm Key { get; set; }
        public X509Certificate2Collection Certificates { get; set; }
    }
}
