using System;
using System.Security.Cryptography;

namespace PeculiarVentures.ACME.Web.Jwa
{
    public class RsaKeyManagement : IKeyManagement
    {
        private readonly RSAEncryptionPadding RsaPadding;

        public RsaKeyManagement(RSAEncryptionPadding padding)
        {
            RsaPadding = padding;
        }

        public byte[] WrapKey(byte[] cek, object key)
        {
            if (key is RSA keyRSA)
            {
                return keyRSA.Encrypt(cek, RsaPadding);
            }

            throw new ArgumentException("RsaKeyManagement algorithm expects key to be RSA type");
        }

        public byte[] UnwrapKey(byte[] encryptedCek, object key)
        {
            if (key is RSA keyRSA)
            {
                return keyRSA.Decrypt(encryptedCek, RsaPadding);
            }

            throw new ArgumentException("RsaKeyManagement algorithm expects key to be RSA type");
        }
    }
}
