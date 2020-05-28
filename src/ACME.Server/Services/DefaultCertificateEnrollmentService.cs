using PeculiarVentures.ACME.Cryptography;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace PeculiarVentures.ACME.Server.Services
{
    /// <summary>
    /// Default certificate enrollment service
    /// </summary>
    public class DefaultCertificateEnrollmentService : ICertificateEnrollmentService
    {
        /// <inheritdoc/>
        public Task<X509Certificate2> Enroll(IOrder order, Pkcs10CertificateRequest request)
        {
            throw new MalformedException("Method not implemented");
        }

        /// <inheritdoc/>
        public Task Revoke(IOrder order, RevokeReason reason)
        {
            throw new MalformedException("Method not implemented");
        }
    }
}
