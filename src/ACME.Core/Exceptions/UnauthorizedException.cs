using System;
namespace PeculiarVentures.ACME
{
    public class UnauthorizedException : AcmeException
    {
        public UnauthorizedException() : base("Unauthorized")
        {
            Type = Protocol.ErrorType.Unauthorized;
        }

        public UnauthorizedException(string message) : base(message)
        {
            Type = Protocol.ErrorType.Unauthorized;
        }
    }
}
