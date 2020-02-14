using System;
using PeculiarVentures.ACME.Protocol;

namespace PeculiarVentures.ACME
{
    public class MalformedException : AcmeException
    {
        public MalformedException() : base(ErrorType.Malformed)
        {
        }

        public MalformedException(string message) : base(ErrorType.Malformed, message)
        {
        }

        public MalformedException(string message, Exception innerException) : base(ErrorType.Malformed, message, innerException)
        {
        }
    }
}
