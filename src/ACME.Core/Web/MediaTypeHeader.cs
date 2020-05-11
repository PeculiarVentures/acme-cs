using System.Net.Http.Headers;

namespace PeculiarVentures.ACME.Web
{
    /// <summary>
    /// Content-Type headers.
    /// </summary>
    /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-6.2"/>
    public static class MediaTypeHeader
    {
        public static readonly MediaTypeHeaderValue JsonContentTypeHeaderValue =
            MediaTypeHeaderValue.Parse("application/jose+json");
        public static readonly MediaTypeHeaderValue ProblemJsonContentTypeHeaderValue =
            MediaTypeHeaderValue.Parse("application/problem+json");
    }
}