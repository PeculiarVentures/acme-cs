namespace PeculiarVentures.ACME.Web.Jwe.Crypto
{
    public interface IJweAlgorithm
    {
        byte[][] Encrypt(byte[] plainText, byte[] cek, byte[] iv, byte[] aad);

        byte[] Decrypt(byte[] secretText, byte[] cek, byte[] iv, byte[] aad, byte[] tag);

        int KeySize { get; }
    }
}
