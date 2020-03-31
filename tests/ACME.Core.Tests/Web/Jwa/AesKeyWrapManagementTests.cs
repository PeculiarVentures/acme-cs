using System.Security.Cryptography;
using PeculiarVentures.ACME.Web.Jwe;
using Xunit;

namespace PeculiarVentures.ACME.Web.Jwa
{
    public class AesKeyWrapManagementTests
    {
        private HMAC _key = new HMACSHA256(Utils.Random(256));
        private byte[] _secret = new byte[] { 177, 161, 244, 128, 84, 143, 225, 115, 63, 180, 3, 255, 107, 154, 212, 246, 138, 7, 110, 91, 112, 46, 34, 105, 47, 130, 203, 46, 122, 234, 64, 252 };

        [Fact]
        public void WrapUnwrapKey()
        {
            var alg = new AesKeyWrapManagement(256);
            var encrypted = alg.WrapKey(_secret, _key);
            var decrypted = alg.UnwrapKey(encrypted, _key);

            Assert.Equal(decrypted, _secret);
        }

        [Fact]
        public void WrapUnwrapKey_NotEqual()
        {
            var key = new HMACSHA256(Utils.Random(256));
            var alg = new AesKeyWrapManagement(256);
            var encrypted = alg.WrapKey(_secret, _key);
            var decrypted = alg.UnwrapKey(encrypted, key);

            Assert.NotEqual(decrypted, _secret);
        }
    }
}
