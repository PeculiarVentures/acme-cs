using System;
using PeculiarVentures.ACME.Protocol;

namespace PeculiarVentures.ACME
{
    public class AccountDoesNotExistException : AcmeException
    {
        public AccountDoesNotExistException() : this("Account does not exist")
        {
        }

        public AccountDoesNotExistException(string message) : base(ErrorType.AccountDoesNotExist, message, System.Net.HttpStatusCode.BadRequest)
        {
        }
    }
}
