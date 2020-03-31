using System.Security.Cryptography;
using System.Text;
using PeculiarVentures.ACME.Web.Jwe;
using PeculiarVentures.ACME.Web.Jwe.Json;
using Xunit;

namespace PeculiarVentures.ACME.Web
{
    public class JsonWebEncyptionTests
    {
        [Fact]
        public void EncryptDecrypt_RSA_Compact()
        {
            string message = "Message to encrypt";
            var key = RSA.Create(2048);
            var jwe = new JsonWebEncyption(
                message,
                new JsonHeader
                {
                    EncryptionAlgorithm = Encryptions.A128GCM,
                    Algorithm = Algorithms.RSA_OAEP,
                }
            );

            jwe.Encrypt(key);

            var jweEncResult = jwe.Export(ExportType.Compact);
            var jweDecResult = JsonWebEncyption.Decrypt(jweEncResult, key);

            Assert.Equal(Encoding.UTF8.GetString(jweDecResult), message);
        }

        [Fact]
        public void EncryptDecrypt_RSA_General()
        {
            string message = "Message to encrypt";
            var key = RSA.Create();
            var jwe = new JsonWebEncyption(
                message,
                new JsonHeader
                {
                    EncryptionAlgorithm = Encryptions.A128GCM,
                    Algorithm = Algorithms.RSA_OAEP,
                },
                "AAD custom value"
            );

            jwe.Encrypt(key);

            var jweEncResult = jwe.Export(ExportType.General);
            var jweDecResult = JsonWebEncyption.Decrypt(jweEncResult, key);

            Assert.Equal(Encoding.UTF8.GetString(jweDecResult), message);
        }

        [Fact]
        public void EncryptDecrypt_RSA_Recipients_Compact()
        {
            string message = "Message to encrypt";
            var key1 = RSA.Create();
            var key2 = RSA.Create();
            var jwe = new JsonWebEncyption(
                message,
                new JsonHeader
                {
                    EncryptionAlgorithm = Encryptions.A128GCM
                },
                "AAD custom value"
            );

            jwe.Recipient(key1, new JsonHeader { Algorithm = Algorithms.RSA_OAEP });
            jwe.Recipient(key2, new JsonHeader { Algorithm = Algorithms.RSA1_5 });

            jwe.Encrypt();

            void export() => jwe.Export(ExportType.Compact);

            var ex = Record.Exception(export);

            Assert.NotNull(ex);
        }

        [Fact]
        public void EncryptDecrypt_RSA_Recipients_General()
        {
            string message = "Message to encrypt";
            var key1 = RSA.Create();
            var key2 = RSA.Create();
            var jwe = new JsonWebEncyption(
                message,
                new JsonHeader
                {
                    EncryptionAlgorithm = Encryptions.A128GCM
                },
                "AAD custom value"
            );

            jwe.Recipient(key1, new JsonHeader { Algorithm = Algorithms.RSA_OAEP });
            jwe.Recipient(key2, new JsonHeader { Algorithm = Algorithms.RSA1_5 });

            jwe.Encrypt();

            var jweEncResult = jwe.Export(ExportType.General);
            var jweDecResult = JsonWebEncyption.Decrypt(jweEncResult, key1);

            Assert.Equal(Encoding.UTF8.GetString(jweDecResult), message);
        }

        [Fact]
        public void EncryptDecrypt_HMAC_General()
        {
            string message = "Message to encrypt";
            var key = new HMACSHA256(Utils.Random(128));
            var jwe = new JsonWebEncyption(
                message,
                new JsonHeader
                {
                    EncryptionAlgorithm = Encryptions.A128CBC_HS256,
                    Algorithm = Algorithms.A128KW,
                },
                "AAD custom value"
            );

            jwe.Encrypt(key);

            var jweEncResult = jwe.Export(ExportType.General);
            var jweDecResult = JsonWebEncyption.Decrypt(jweEncResult, key);

            Assert.Equal(Encoding.UTF8.GetString(jweDecResult), message);
        }
    }
}
