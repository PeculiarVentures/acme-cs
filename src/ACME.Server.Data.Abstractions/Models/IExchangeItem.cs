using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Server.Data.Abstractions.Models
{
    public interface IExchangeItem
    {
        AsymmetricAlgorithm Key { get; set; }
        X509Certificate2Collection Certificates { get; set; }
    }
}
