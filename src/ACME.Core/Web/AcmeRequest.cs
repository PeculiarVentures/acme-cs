using System;
namespace PeculiarVentures.ACME.Web
{
    public class AcmeRequest
    {
        public AcmeRequest()
        {
        }

        public AcmeRequest(JsonWebSignature token)
        {
            Token = token ?? throw new ArgumentNullException(nameof(token));
        }

        public JsonWebSignature Token { get; set; }

        public JsonWebKey PublicKey
        {
            get
            {
                if (Token != null)
                {
                    var header = Token.GetProtected();
                    return header.Key;
                }
                return null;
            }
        }

        public string KeyId
        {
            get
            {
                if (Token != null)
                {
                    var header = Token.GetProtected();
                    return header.KeyID;
                }
                return null;
            }
        }

        public object Content { get; set; }
        public string Method { get; set; }

        public T GetContent<T>()
        {
            try
            {
                return Token.GetPayload<T>();
            }
            catch (Exception e)
            {
                throw new MalformedException(e.Message);
            }

        }

    }
}
