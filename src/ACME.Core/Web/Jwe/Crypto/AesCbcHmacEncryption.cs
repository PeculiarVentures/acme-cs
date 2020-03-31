using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace PeculiarVentures.ACME.Web.Jwe.Crypto
{
    public class AesCbcHmacEncryption : IJweAlgorithm
    {
        private readonly string _hashAlgorithm;
        public int KeySize { get; }

        public AesCbcHmacEncryption(string hashAlgorithm, int keySize)
        {
            _hashAlgorithm = hashAlgorithm;
            KeySize = keySize;
        }

        public byte[][] Encrypt(byte[] plainText, byte[] cek, byte[] iv, byte[] aad)
        {
            using (var aes = new AesManaged
            {
                KeySize = KeySize,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
            })
            {
                using (var cipher = aes.CreateEncryptor(cek, iv))
                {
                    var cipherText = cipher.TransformFinalBlock(plainText, 0, plainText.Length);

                    using (var hmac = HMAC.Create(_hashAlgorithm))
                    using (var hmacStream = new MemoryStream())
                    using (var hmacWriter = new BinaryWriter(hmacStream))
                    {
                        hmac.Key = cek;

                        hmacWriter.Write(aad);
                        hmacWriter.Write(iv);
                        hmacWriter.Write(cipherText);
                        hmacWriter.Flush();

                        var tag = hmac.ComputeHash(hmacStream.ToArray())
                            .Take(cek.Length)
                            .ToArray();

                        return new[] { cipherText, tag };
                    }
                }
            }
        }

        public byte[] Decrypt(byte[] secretText, byte[] cek, byte[] iv, byte[] aad, byte[] tag)
        {
            using (var aes = new AesManaged
            {
                KeySize = KeySize,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
            })
            {
                using (var decipher = aes.CreateDecryptor(cek, iv))
                {
                    var message = decipher.TransformFinalBlock(secretText, 0, secretText.Length);

                    using (var hmac = HMAC.Create(_hashAlgorithm))
                    using (var hmacStream = new MemoryStream())
                    using (var hmacWriter = new BinaryWriter(hmacStream))
                    {
                        hmac.Key = cek;

                        hmacWriter.Write(aad);
                        hmacWriter.Write(iv);
                        hmacWriter.Write(secretText);
                        hmacWriter.Flush();

                        return message;
                    }
                }
            }
        }
    }
}
