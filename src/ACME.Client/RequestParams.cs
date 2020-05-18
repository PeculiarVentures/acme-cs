using System;
using System.Net.Http;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Client
{
    public class RequestParams
    {
        public HttpMethod Method { get; set; } = HttpMethod.Get;
        public object Payload { get; set; }
        public bool IncludePublicKey { get; set; } = false;
        public HeaderCollection Headers { get; set; } = new HeaderCollection();
    }
}
