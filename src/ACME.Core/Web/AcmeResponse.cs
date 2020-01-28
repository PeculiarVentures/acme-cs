using System;
using PeculiarVentures.ACME.Web.Http;

namespace PeculiarVentures.ACME.Web
{
    public class AcmeResponse
    {
        public AcmeResponse()
        {
        }

        public int StatusCode { get; set; }
        public string ReplayNonce { get; set; }
        public Uri Location { get; set; }
        public LinkHeaderCollection Links { get; set; } = new LinkHeaderCollection();
        public object Content { get; set; }

        /// <summary>
        /// Returns content in specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidCastException"/>
        public T GetContent<T>() where T : class
        {
            if (Content is T)
            {
                return (T)Content;
            }

            throw new InvalidCastException();
        }
    }

    public class AcmeResponse<T> : AcmeResponse where T : class
    {
        public new T Content
        {
            get => GetContent<T>();
            set => base.Content = value;
        }
    }
}
