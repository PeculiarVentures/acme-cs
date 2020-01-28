using System;
namespace PeculiarVentures.ACME
{
    public class AccountDoesNotExistException : AcmeException
    {
        public AccountDoesNotExistException() : this("Account does not exist")
        {
            Type = Protocol.ErrorType.AccountDoesNotExist;
        }

        public AccountDoesNotExistException(string message) : base(message)
        {
            Type = Protocol.ErrorType.AccountDoesNotExist;
        }
    }
}
