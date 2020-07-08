using System;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;

namespace PeculiarVentures.ACME.Server.Services
{

    public class AccountAccess
    {
        public IAccount Account { get; set; }
        public  IAccountId Target { get; set; }
    }

    public interface IAccountSecurityService
    {
        void CheckAccess(AccountAccess data);
    }
}
