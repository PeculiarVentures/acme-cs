using System;
using System.Text;
using Xunit;

namespace PeculiarVentures.ACME.Web.Jwe.Crypto
{
    public class AesGcmEncryptionTests
    {
        private byte[] _secret = new byte[] { 194, 164, 235, 6, 138, 248, 171, 239, 24, 216, 11, 22, 137, 199, 215, 133 };

        [Fact]
        public void Encrypt()
        {
            var aes = new AesGcmEncryption(128);
            byte[] iv = Convert.FromBase64String("AQIDBAUGBwgJCgsM");
            byte[] aad = Encoding.UTF8.GetBytes("secre");
            byte[] message = Encoding.UTF8.GetBytes("hellow aes !");

            byte[][] encrypted = aes.Encrypt(message, _secret, iv, aad);

            Assert.Equal(encrypted[0], Convert.FromBase64String("9fKgpvo+ZtOeKj5J"));
            Assert.Equal(encrypted[1], Convert.FromBase64String("w0XYjHY6MIMv4c3GTgy0TA=="));
        }

        [Fact]
        public void Decrypt()
        {
            var aes = new AesGcmEncryption(128);
            byte[] iv = Convert.FromBase64String("DAsKCQgHBgUEAwIB");
            byte[] tag = Convert.FromBase64String("eetdqbnAyuaCJSOHLoGoaA==");
            byte[] cipherText = Convert.FromBase64String("IQbOAbZyg9p8PA==");
            byte[] aad = Encoding.UTF8.GetBytes("top secret");

            byte[] decrypted = aes.Decrypt(cipherText, _secret, iv, aad, tag);

            Assert.Equal(decrypted, Encoding.UTF8.GetBytes("decrypt me"));
        }
    }
}
