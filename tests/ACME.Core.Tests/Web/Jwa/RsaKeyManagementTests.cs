using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace PeculiarVentures.ACME.Web.Jwa
{
    public class RsaKeyManagementTests
    {
        private RSA _key = RSA.Create();
        private byte[] _secret = Encoding.UTF8.GetBytes("secret");

        [Fact]
        public void WrapUnwrapKey_RSA_OAEP()
        {
            var alg = new RsaKeyManagement(RSAEncryptionPadding.OaepSHA1);
            var encrypted = alg.WrapKey(_secret, _key);
            var decrypted = alg.UnwrapKey(encrypted, _key);

            Assert.Equal(decrypted, _secret);
        }

        [Fact]
        public void WrapUnwrapKey_RSA_OAEP_256()
        {
            var alg = new RsaKeyManagement(RSAEncryptionPadding.OaepSHA256);
            var encrypted = alg.WrapKey(_secret, _key);
            var decrypted = alg.UnwrapKey(encrypted, _key);

            Assert.Equal(decrypted, _secret);
        }

        [Fact]
        public void WrapUnwrapKey_RSA1_5()
        {
            var alg = new RsaKeyManagement(RSAEncryptionPadding.Pkcs1);
            var encrypted = alg.WrapKey(_secret, _key);
            var decrypted = alg.UnwrapKey(encrypted, _key);

            Assert.Equal(decrypted, _secret);
        }

        [Fact]
        public void WrapUnwrapKey_ThrowError()
        {
            var key = RSA.Create();
            var alg = new RsaKeyManagement(RSAEncryptionPadding.OaepSHA1);
            var encrypted = alg.WrapKey(_secret, _key);

            void unwrap() => alg.UnwrapKey(encrypted, key);

            var ex = Record.Exception(unwrap);

            Assert.NotNull(ex);
        }
    }
}
