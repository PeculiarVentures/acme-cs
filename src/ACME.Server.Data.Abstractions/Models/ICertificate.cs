namespace PeculiarVentures.ACME.Server.Data.Abstractions.Models
{
    public interface ICertificate : IBaseObject
    {
        byte[] RawData { get; set; }
    }
}