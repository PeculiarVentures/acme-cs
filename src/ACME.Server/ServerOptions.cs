using System;
namespace PeculiarVentures.ACME.Server
{
    public class ServerOptions
    {
        public ICertificateEnrollmentHandler EnrollmentHandler { get; set; }
        public DownloadCertificateFormat DownloadCertificateFormat { get; set; }
    }
}
