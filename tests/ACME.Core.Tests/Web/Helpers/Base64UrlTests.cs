using System.Text;
using Xunit;

namespace PeculiarVentures.ACME.Web.Helpers
{
    public class Base64UrlTests
    {
        [Fact]
        public void EncodeFromStringAndDecode()
        {
            string message = "He=llo+Wo/rld";

            var encoded = Base64Url.Encode(message);
            var decoded = Base64Url.Decode(encoded);
            var messageDecoded = Encoding.UTF8.GetString(decoded);

            Assert.Equal(messageDecoded, message);
        }

        [Fact]
        public void EncodeFromByteAndDecode()
        {
            string message = "He=llo+Wo/rld";
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            var encoded = Base64Url.Encode(messageBytes);
            var decoded = Base64Url.Decode(encoded);
            var messageDecoded = Encoding.UTF8.GetString(decoded);

            Assert.Equal(messageDecoded, message);
        }
    }
}
