namespace PeculiarVentures.ACME.Server.Data.Abstractions.Models
{
    public interface ICertificate
    {
        /// <summary>
        /// Certificate thumbprint
        /// </summary>
        string Thumbprint { get; set; }

        /// <summary>
        /// Certificate in array bytes
        /// </summary>
        byte[] RawData { get; set; }

        /// <summary>
        /// The status of this certificate
        /// </summary>
        bool Revoked { get; set; }
    }
}