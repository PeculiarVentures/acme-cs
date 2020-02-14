namespace PeculiarVentures.ACME.Server.Data.Abstractions.Models
{
    public interface ICertificate
    {
        string Thumbprint { get; set; }
        byte[] RawData { get; set; }
        bool Revoked { get; set; }
    }
}