using System;
using System.Net;

namespace PeculiarVentures.ACME.Web
{
    public class AcmeRequest
    {
        public AcmeRequest()
        {
        }

        public HeaderCollection Headers { get; set; } = new HeaderCollection();

        public AcmeRequest(JsonWebSignature token)
        {
            Token = token ?? throw new ArgumentNullException(nameof(token));
        }

        public JsonWebSignature Token { get; set; }

        public Query Query { get; set; } = new Query();
        public string Method { get; set; }
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
