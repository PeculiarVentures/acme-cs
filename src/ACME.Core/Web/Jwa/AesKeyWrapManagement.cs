using System;
using System.IO;
using System.Security.Cryptography;

namespace PeculiarVentures.ACME.Web.Jwa
{
    public class AesKeyWrapManagement : IKeyManagement
    {
        public int KeyLength { get; }

        public AesKeyWrapManagement(int keyLength)
        {
            KeyLength = keyLength;
        }

        public byte[] WrapKey(byte[] cek, object key)
        {
            if (key is HMAC keyHMAC)
            {
                using (var aes = new AesManaged
                {
                    KeySize = KeyLength,
                    Mode = CipherMode.ECB,
                    Padding = PaddingMode.None,
                    Key = keyHMAC.Key
                })
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                        {
                            using (CryptoStream encrypt = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                            {
                                encrypt.Write(cek, 0, cek.Length);
                                encrypt.FlushFinalBlock();

                                return ms.ToArray();
                            }
                        }
                    }
                }
            }

            throw new ArgumentException("AesKeyWrapManagement algorithm expects key to be HMAC type");
        }

        public byte[] UnwrapKey(byte[] encryptedCek, object key)
        {
            if (key is HMAC keyHMAC)
            {
                using (var aes = new AesManaged
                {
                    KeySize = KeyLength,
                    Mode = CipherMode.ECB,
                    Padding = PaddingMode.None,
                    Key = keyHMAC.Key
                })
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                        {
                            using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                            {
                                cs.Write(encryptedCek, 0, encryptedCek.Length);
                                cs.FlushFinalBlock();

                                return ms.ToArray();
                            }
                        }
                    }
                }
            }

            throw new ArgumentException("AesKeyWrapManagement algorithm expects key to be HMAC type");
        }
    }
}
