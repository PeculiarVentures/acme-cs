using System;
namespace PeculiarVentures.ACME
{
    public class BadNonceException : AcmeException
    {
        public BadNonceException() : base("Bad nonce")
        {
            Type = Protocol.ErrorType.BadNonce;
        }
    }
}
