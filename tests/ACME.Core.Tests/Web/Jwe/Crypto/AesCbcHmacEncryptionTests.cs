using System;
using System.Text;
using Xunit;

namespace PeculiarVentures.ACME.Web.Jwe.Crypto
{
    public class AesCbcHmacEncryptionTests
    {
        private byte[] _secret = new byte[] { 194, 164, 235, 6, 138, 248, 171, 239, 24, 216, 11, 22, 137, 199, 215, 133 };

        [Fact]
        public void Encrypt()
        {
            var aes = new AesCbcHmacEncryption("HMACSHA256", 256);
            byte[] iv = Convert.FromBase64String("AQIDBAUGBwgJCgsMDQ4PEA==");
            byte[] aad = Encoding.UTF8.GetBytes("secre");
            byte[] message = Encoding.UTF8.GetBytes("hellow aes !");

            byte[][] encrypted = aes.Encrypt(message, _secret, iv, aad);

            Assert.Equal(encrypted[0], Convert.FromBase64String("XPiWNCFhRNF4FNCpEDo3XQ=="));
            Assert.Equal(encrypted[1], Convert.FromBase64String("YNXx5Vr/kXqN6oAvJov/IA=="));
        }

        [Fact]
        public void Decrypt()
        {
            var aes = new AesCbcHmacEncryption("HMACSHA256", 256);
            byte[] iv = Convert.FromBase64String("AQIDBAUGBwgJCgsMDQ4PEA==");
            byte[] tag = Convert.FromBase64String("ks3MK9t3B93w3v8DCnfl2A==");
            byte[] cipherText = Convert.FromBase64String("IsUCtcnDgfJ3qN/1VlridQ==");
            byte[] aad = Encoding.UTF8.GetBytes("top secret");

            byte[] decrypted = aes.Decrypt(cipherText, _secret, iv, aad, tag);

            Assert.Equal(decrypted, Encoding.UTF8.GetBytes("decrypt me"));
        }
    }
}
