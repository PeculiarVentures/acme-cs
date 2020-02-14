using System;
using System.Net;
using PeculiarVentures.ACME.Protocol;

namespace PeculiarVentures.ACME
{
    public class AcmeException : Exception
    {
        private const HttpStatusCode StatusCodeDefault = HttpStatusCode.InternalServerError;

        public ErrorType Type { get; private set; }
        public HttpStatusCode StatusCode { get; private set; }

        public AcmeException()
        {
        }

        public AcmeException(ErrorType type) : base()
        {
            Type = type;
            StatusCode = StatusCodeDefault;
        }

        public AcmeException(ErrorType type, string message, HttpStatusCode status) : base(message)
        {
            Type = type;
            StatusCode = status;
        }

        public AcmeException(ErrorType type, string message) : base(message)
        {
            Type = type;
            StatusCode = StatusCodeDefault;
        }

        public AcmeException(ErrorType type, string message, HttpStatusCode status, Exception innerException) : base(message, innerException)
        {
            Type = type;
            StatusCode = status;
        }

        public AcmeException(ErrorType type, string message, Exception innerException) : base(message, innerException)
        {
            Type = type;
            StatusCode = StatusCodeDefault;
        }
    }
}
