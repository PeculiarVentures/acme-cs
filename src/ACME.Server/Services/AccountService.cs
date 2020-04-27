using System;
using Microsoft.Extensions.Options;
using NLog;
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

        /// <summary>
        /// Gets an account by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IAccount GetById(int id)
        {
            var account = AccountRepository.GetById(id);
            if (account == null)
            {
                throw new AccountDoesNotExistException();
            }

            return account;
        }

        /// <summary>
        /// Gets an account by public key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IAccount GetByPublicKey(JsonWebKey key)
        {
            var account = FindByPublicKey(key);

            if (account == null)
            {
                throw new AccountDoesNotExistException();
            }

            return account;
        }

        /// <summary>
        /// Finds an account by public key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Creates new account
        /// </summary>
        /// <param name="key"></param>
        /// <param name="params"></param>
        /// <returns></returns>
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

            // Creates account
            var account = AccountRepository.Create();
            OnCreateParams(account, key, @params);

            if (Options.ExternalAccountOptions.Type != ExternalAccountType.None)
            {
                // Uses external account binding
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
            // Adds account
            account = AccountRepository.Add(account);

            Logger.Info("Account {id} created", account.Id);

            return account;
        }

        /// <summary>
        /// Fills parameters
        /// </summary>
        /// <param name="account"></param>
        /// <param name="key"></param>
        /// <param name="params"></param>
        private static void OnCreateParams(IAccount account, JsonWebKey key, NewAccount @params)
        {
            account.Key = key;
            account.Contacts = @params.Contacts;
            account.TermsOfServiceAgreed = @params.TermsOfServiceAgreed ?? false;
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

            Logger.Info("Account {id} updated", account.Id);

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

            Logger.Info("Account {id} deactivated", account.Id);

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

            Logger.Info("Account {id} revoked", account.Id);

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

            Logger.Info("Account {id} key changed", account.Id);

            // Return JSON
            return account;
        }

    }
}
