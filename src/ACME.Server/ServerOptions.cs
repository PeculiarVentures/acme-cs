using System;
using System.Security.Cryptography.X509Certificates;

namespace PeculiarVentures.ACME.Server
{
    public class ServerOptions
    {
        public ICertificateEnrollmentHandler EnrollmentHandler { get; set; }
        public DownloadCertificateFormat DownloadCertificateFormat { get; set; } = DownloadCertificateFormat.PemCertificateChain;
        public X509Certificate2Collection ExtraCertificateStorage { get; set; } = new X509Certificate2Collection();
    }
}
