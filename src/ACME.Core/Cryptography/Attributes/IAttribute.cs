namespace PeculiarVentures.ACME.Cryptography
{
    public interface IAsnEncodableData
    {
        byte[] RawData { get; }

        byte[] Export();
        void Import(byte[] rawData);

        void Reset();
    }
}