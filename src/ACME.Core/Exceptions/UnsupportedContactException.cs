using System;
namespace PeculiarVentures.ACME.Exceptions
{
    public class UnsupportedContactException : AcmeException
    {
        public UnsupportedContactException()
            : this("Unsupported contact")
        {
        }

        public UnsupportedContactException(string message)
            : base(Protocol.ErrorType.UnsupportedContact, message, System.Net.HttpStatusCode.BadRequest)
        { }
    }
}
