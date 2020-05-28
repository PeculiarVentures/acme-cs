using PeculiarVentures.ACME.Cryptography;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace PeculiarVentures.ACME.Server.Services
{
    public interface ICertificateEnrollmentService
    {
        /// <summary>
        /// Enrolls crtificate
        /// </summary>
        /// <param name="order"><see cref="IOrder"/></param>
        /// <param name="request">PKCS10 request</param>
        /// <returns></returns>
        Task<X509Certificate2> Enroll(IOrder order, Pkcs10CertificateRequest request);

        /// <summary>
        /// Revokes certificate
        /// </summary>
        /// <param name="order"><see cref="IOrder"/></param>
        /// <param name="reason">Revoke reason</param>
        /// <returns></returns>
        Task Revoke(IOrder order, Protocol.RevokeReason reason);
    }
}
