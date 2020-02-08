using System;
using PeculiarVentures.ACME.Protocol;

namespace PeculiarVentures.ACME
{
    public class AcmeException : Exception
    {
        public ErrorType Type { get; set; }

        public AcmeException(string message) : base(message)
        {
        }
        public AcmeException(ErrorType type, string message) : base(message)
        {
            Type = type;
        }
        public AcmeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public AcmeException()
        {
        }

        public AcmeException(ErrorType type) : base()
        {
            Type = type;
        }
    }
}
