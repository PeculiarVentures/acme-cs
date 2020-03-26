using PeculiarVentures.ACME.Cryptography;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace PeculiarVentures.ACME.Server.Services
{
    public sealed class DefaultCertificateEnrollmentService : ICertificateEnrollmentService
    {
        public Task<X509Certificate2> Enroll(IOrder order, Pkcs10CertificateRequest request)
        {
            throw new MalformedException("Method not implemented");
        }

        public Task Revoke(IOrder order, RevokeReason reason)
        {
            throw new MalformedException("Method not implemented");
        }

        public IExchangeItem GetExchangeItem(IAccount account)
        {
            throw new MalformedException("Method not implemented");
        }

        public void ArchiveKey(IOrder order, AsymmetricAlgorithm key)
        {
            throw new MalformedException("Method not implemented");
        }
    }
}
