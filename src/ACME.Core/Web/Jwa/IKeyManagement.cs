namespace PeculiarVentures.ACME.Web.Jwa
{
    public interface IKeyManagement
    {
        byte[] WrapKey(byte[] cek, object key);

        byte[] UnwrapKey(byte[] encryptedCek, object key);
    }
}
