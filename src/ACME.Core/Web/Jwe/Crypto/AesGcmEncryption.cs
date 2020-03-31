using System;
using System.IO;
using System.Linq;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

namespace PeculiarVentures.ACME.Web.Jwe.Crypto
{
    public class AesGcmEncryption : IJweAlgorithm
    {
        public int KeySize { get; }

        public AesGcmEncryption(int keySize)
        {
            KeySize = keySize;
        }

        public byte[][] Encrypt(byte[] plainText, byte[] cek, byte[] iv, byte[] aad)
        {
            var cipher = new GcmBlockCipher(new AesEngine());
            var parameters = new AeadParameters(new KeyParameter(cek), KeySize, iv, aad);
            cipher.Init(true, parameters);

            var cipherText = new byte[cipher.GetOutputSize(plainText.Length)];
            var len = cipher.ProcessBytes(plainText, 0, plainText.Length, cipherText, 0);
            cipher.DoFinal(cipherText, len);

            return new[] { cipherText.SkipLast(16).ToArray(), cipher.GetMac() };
        }

        public byte[] Decrypt(byte[] secretText, byte[] cek, byte[] iv, byte[] aad, byte[] tag)
        {
            byte[] fullText = Utils.CombineArrays(secretText, tag);

            using (var cipherStream = new MemoryStream(fullText))
            using (var cipherReader = new BinaryReader(cipherStream))
            {
                var cipher = new GcmBlockCipher(new AesEngine());
                var parameters = new AeadParameters(new KeyParameter(cek), KeySize, iv, aad);
                cipher.Init(false, parameters);

                var cipherText = cipherReader.ReadBytes(fullText.Length);
                var plainText = new byte[cipher.GetOutputSize(cipherText.Length)];

                var len = cipher.ProcessBytes(cipherText, 0, cipherText.Length, plainText, 0);
                cipher.DoFinal(plainText, len);

                return plainText;
            }
        }
    }
}
