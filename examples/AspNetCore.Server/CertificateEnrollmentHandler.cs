using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using PeculiarVentures.ACME.Cryptography;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;

namespace AspNetCore.Server
{
    public class CertificateEnrollmentHandler : ICertificateEnrollmentHandler
    {
        public async Task<X509Certificate2> Enroll(IOrder order, Pkcs10CertificateRequest request)
        {
            return await Task.Run(() =>
            {
                var pem = "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";
                return new X509Certificate2(Convert.FromBase64String(pem));
            });
        }

        public Task Revoke(IOrder order, RevokeReason reason)
        {
            return Task.Run(() => {
                // nothing
            });
        }
    }
}
