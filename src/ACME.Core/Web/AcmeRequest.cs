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

        public JsonWebKey Key
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
        public string Url
        {
            get
            {
                if (Token != null)
                {
                    var header = Token.GetProtected();
                    return header.Url;
                }
                return null;
            }
        }

        /// <summary>
        /// Converts content to specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="MalformedException"/>
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
