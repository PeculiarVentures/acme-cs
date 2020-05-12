using System;
using PeculiarVentures.ACME.Protocol;

namespace PeculiarVentures.ACME
{
    public class MalformedException : AcmeException
    {
        public MalformedException()
            : this("Malformed request")
        { }

        public MalformedException(string message)
            : base(ErrorType.Malformed, message, System.Net.HttpStatusCode.BadRequest)
        { }

        public MalformedException(string message, Exception innerException)
            : base(ErrorType.Malformed, message, System.Net.HttpStatusCode.BadRequest, innerException)
        { }
    }
}
