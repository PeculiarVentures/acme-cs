using System;
namespace PeculiarVentures.ACME.Web
{
    public class AcmeRequest : JsonWebSignature
    {
        public JsonWebKey PublicKey
        {
            get
            {
                var header = GetProtected();
                return header.Key;
            }
        }

        public string KeyId
        {
            get
            {
                var header = GetProtected();
                return header.KeyID;
            }
        }
    }
}
