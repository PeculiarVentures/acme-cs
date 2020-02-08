namespace PeculiarVentures.ACME.Server.Data.Abstractions.Models
{
    public interface ICertificate : IBaseObject
    {
        string Thumbprint { get; set; }
        byte[] RawData { get; set; }
    }
}