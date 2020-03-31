using PeculiarVentures.ACME.Web.Jwe.Json;

namespace PeculiarVentures.ACME.Web.Jwe
{
    public class Recipient
    {
        public JsonHeader Header { get; set; }

        public object EncryptionKey { get; set; }
    }
}
