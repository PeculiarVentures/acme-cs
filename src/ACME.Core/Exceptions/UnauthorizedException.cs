using System;
using System.Net;
using PeculiarVentures.ACME.Protocol;

namespace PeculiarVentures.ACME
{
    public class UnauthorizedException : AcmeException
    {
        public UnauthorizedException() : this("Unauthorized")
        {
        }

        public UnauthorizedException(string message) : base(ErrorType.Unauthorized, message, HttpStatusCode.Unauthorized)
        {
        }
    }
}
