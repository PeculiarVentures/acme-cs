using System;
using Microsoft.Extensions.Options;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Protocol.Messages;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Server.Services
{
    public class AccountService : BaseService, IAccountService
    {
        public AccountService(
            IOptions<ServerOptions> options,
            IAccountRepository accountRepository,
            IExternalAccountService externalAccountService)
            : base(options)
        {
            AccountRepository = accountRepository
                ?? throw new ArgumentNullException(nameof(accountRepository));
            ExternalAccountService = externalAccountService
                ?? throw new ArgumentNullException(nameof(externalAccountService));
        }

        public IAccountRepository AccountRepository { get; }
        public IExternalAccountService ExternalAccountService { get; }

        public IAccount GetById(int id)
        {
            return AccountRepository.GetById(id)
                ?? throw new AccountDoesNotExistException();
        }

        public IAccount GetByPublicKey(JsonWebKey key)
        {
            #region Check arguments
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            #endregion

            return FindByPublicKey(key)
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
            #region Check arguments
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (@params is null)
            {
                throw new ArgumentNullException(nameof(@params));
            }
            #endregion

            var account = AccountRepository.Create(key, @params);

            if (Options.ExternalAccountOptions.Type != ExternalAccountType.None)
            {
                // Use external account binding
                if (Options.ExternalAccountOptions.Type == ExternalAccountType.Required
                    && @params.ExternalAccountBinding == null)
                {
                    throw new MalformedException("externalAccountBinding is required"); // TODO check rfc error
                }

                if (@params.ExternalAccountBinding != null)
                {
                    var eab = ExternalAccountService.Validate(key, @params.ExternalAccountBinding);
                    if (eab.Status == ExternalAccountStatus.Invalid)
                    {
                        throw new MalformedException("externalAccountBinding has wrong signature"); // TODO check rfc error
                    }
                    account.ExternalAccountId = eab.Id;
                }
            }
            
            account = AccountRepository.Add(account);
            return account;
        }

        public IAccount Update(int accountId, string[] contacts)
        {
            #region Check arguments
            if (contacts is null)
            {
                throw new ArgumentNullException(nameof(contacts));
            }
            #endregion

            // Get account
            var account = GetById(accountId);

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
            var account = GetById(accountId);

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
            var account = GetById(accountId);

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
            var account = GetById(accountId);

            // Check key
            if (AccountRepository.FindByPublicKey(key) != null)
            {
                throw new MalformedException(nameof(key));
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
