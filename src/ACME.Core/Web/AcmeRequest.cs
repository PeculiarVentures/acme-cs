using System;
namespace PeculiarVentures.ACME.Web
{
    public class AcmeRequest
    {
        public AcmeRequest(JsonWebSignature token)
        {
            Token = token ?? throw new ArgumentNullException(nameof(token));
        }

        public JsonWebSignature Token { get; set; }

        public JsonWebKey PublicKey
        {
            get
            {
                var header = Token.GetProtected();
                return header.Key;

            }
        }

        public string KeyId
        {
            get
            {
                var header = Token.GetProtected();
                return header.KeyID;
            }
        }

        public object Content { get; set; }

        public T GetContent<T>()
        {
            return Token.GetPayload<T>();
        }

    }
}
