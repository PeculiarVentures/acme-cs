using System;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Protocol.Messages;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
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

        public IAccount GetById(int id)
        {
            return AccountRepository.GetById(id)
                ?? throw new AccountDoesNotExistException();
        }

        public IAccount FindByPublicKey(JsonWebKey key)
        {
            #region Check arguments
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            #endregion

            return AccountRepository.FindByPublicKey(key);
        }

        public IAccount Create(JsonWebKey key, NewAccount @params)
        {
            var account = AccountRepository.Create(key, @params);
            account = AccountRepository.Add(account);
            return account;
        }

        public IAccount Update(int accountId, string[] contacts)
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
            return account;
        }

        public IAccount Deactivate(int accountId)
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
            return account;
        }

        public IAccount Revoke(int accountId)
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
            return account;
        }

        public IAccount ChangeKey(int accountId, JsonWebKey key)
        {
            #region Check arguments
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            #endregion

            // Get account
            var account = AccountRepository.GetById(accountId);
            if (account is null)
            {
                throw new AccountDoesNotExistException();
            }

            // Check key
            if (AccountRepository.FindByPublicKey(key) != null)
            {
                // TODO ACME Exception. Use RFC
                throw new ArgumentException(nameof(key));
            }

            // Change key
            account.Key = key;

            // Save changes
            AccountRepository.Update(account);

            // Return JSON
            return account;
        }
    }
}
