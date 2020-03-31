using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace GlobalSign.ACME.Server.Data.EF.Core.Models
{
    public interface IExchangeItem
    {
        AsymmetricAlgorithm Key { get; set; }
        X509Certificate2Collection Certificates { get; set; }
    }
}
