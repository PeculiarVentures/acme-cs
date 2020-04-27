using System;
using System.Collections.Generic;

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

        public Query Query { get; set; } = new Query();
        public object Content { get; set; }
        public string Method { get; set; }

        /// <summary>
        /// Returns Url header from Token
        /// </summary>
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

        public string Path { get; set; }

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

        public object GetContent(Type type)
        {
            try
            {
                return Token.GetPayload(type);
            }
            catch (Exception e)
            {
                throw new MalformedException(e.Message);
            }
        }

    }
}
