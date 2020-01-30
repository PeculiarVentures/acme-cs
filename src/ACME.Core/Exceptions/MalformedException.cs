using System;
using PeculiarVentures.ACME.Protocol;

namespace PeculiarVentures.ACME
{
    public class MalformedException : AcmeException
    {
        public MalformedException()
        {
            Type = ErrorType.Malformed;
        }

        public MalformedException(string message) : base(message)
        {
            Type = ErrorType.Malformed;
        }

        public MalformedException(string message, Exception innerException) : base(message, innerException)
        {
            Type = ErrorType.Malformed;
        }
    }
}
