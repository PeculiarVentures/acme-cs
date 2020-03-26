using System;
using System.Security.Cryptography.X509Certificates;

namespace PeculiarVentures.ACME.Server
{
    public class ServerOptions
    {
        public DownloadCertificateFormat DownloadCertificateFormat { get; set; } = DownloadCertificateFormat.PemCertificateChain;
        public X509Certificate2Collection ExtraCertificateStorage { get; set; } = new X509Certificate2Collection();
        public string BaseAddress { get; set; }
        public int ExpireAuthorizationDays { get; set; } = 7;
        public ExternalAccountOptions ExternalAccountOptions { get; set; } = new ExternalAccountOptions();
    }
}
