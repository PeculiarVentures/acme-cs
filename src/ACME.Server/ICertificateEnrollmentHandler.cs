using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using PeculiarVentures.ACME.Cryptography;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;

namespace PeculiarVentures.ACME.Server
{
    public interface ICertificateEnrollmentHandler
    {
        Task<X509Certificate2> Enroll(IOrder order, Pkcs10CertificateRequest request);
        Task Revoke(IOrder order, Protocol.RevokeReason reason);
    }
}
