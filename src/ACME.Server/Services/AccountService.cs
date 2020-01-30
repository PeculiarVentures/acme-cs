using System;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Protocol.Messages;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Server.Services
{
    public class AccountService : IAccountService
    {
        public AccountService(IAccountRepository accountRepository)
        {
            AccountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        }

        public IAccountRepository AccountRepository { get; }

        public Account GetById(int id)
        {
            var account = AccountRepository.GetById(id);
            if (account == null)
            {

                throw new AccountDoesNotExistException();
            }

            return account == null
                ? null
                : AccountRepository.Convert(account);
        }

        public Account GetByPublicKey(JsonWebKey key)
        {
            #region Check arguments
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            #endregion

            var account = AccountRepository.GetByPublicKey(key);

            return account == null
                ? null
                : AccountRepository.Convert(account);
        }

        public Account Create(JsonWebKey key, NewAccount @params)
        {
            var account = AccountRepository.Create(key, @params);
            account = AccountRepository.Add(account);
            return AccountRepository.Convert(account);
        }

        public Account Update(int accountId, string[] contacts)
        {
            // Get account
            var account = AccountRepository.GetById(accountId);
            if (account == null)
            {
                throw new AccountDoesNotExistException();
            }

            // Assign values
            account.Contacts = contacts;

            // Save changes
            account = AccountRepository.Update(account);

            // Return JSON
            return AccountRepository.Convert(account);
        }

        public Account Deactivate(int accountId)
        {
            // Get account
            var account = AccountRepository.GetById(accountId);
            if (account == null)
            {
                throw new AccountDoesNotExistException();
            }

            // Assign values
            account.Status = AccountStatus.Deactivated;

            // Save changes
            account = AccountRepository.Update(account);

            // Return JSON
            return AccountRepository.Convert(account);
        }

        public Account Revoke(int accountId)
        {
            // Get account
            var account = AccountRepository.GetById(accountId);
            if (account == null)
            {
                throw new AccountDoesNotExistException();
            }

            // Assign values
            account.Status = AccountStatus.Revoked;

            // Save changes
            account = AccountRepository.Update(account);

            // Return JSON
            return AccountRepository.Convert(account);
        }

    }
}
