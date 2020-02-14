using System;
using PeculiarVentures.ACME.Protocol;

namespace PeculiarVentures.ACME
{
    public class BadNonceException : AcmeException
    {
        public BadNonceException() : base(ErrorType.BadNonce, "Bad nonce")
        {
        }
    }
}
