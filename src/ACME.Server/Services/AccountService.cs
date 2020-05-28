using System;
using Microsoft.Extensions.Options;
using PeculiarVentures.ACME.Exceptions;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Protocol.Messages;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Server.Services
{
    /// <summary>
    /// Account service
    /// </summary>
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

        /// <inheritdoc/>
        public IAccount GetById(int id)
        {
            var account = AccountRepository.GetById(id);
            if (account == null)
            {
                throw new AccountDoesNotExistException();
            }
            return account;
        }

        /// <inheritdoc/>
        public IAccount GetByPublicKey(JsonWebKey key)
        {
            var account = FindByPublicKey(key);
            if (account == null)
            {
                throw new AccountDoesNotExistException();
            }
            return account;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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
            if (@params.Contacts != null && !ValidateContacts(@params.Contacts))
            {
                throw new UnsupportedContactException();
            }

            /// If the server wishes to require the client to agree to terms under
            /// which the ACME service is to be used, it MUST indicate the URL where
            /// such terms can be accessed in the "termsOfService" subfield of the
            /// "meta" field in the directory object, and the server MUST reject
            /// newAccount requests that do not have the "termsOfServiceAgreed" field
            /// set to "true". Clients SHOULD NOT automatically agree to terms by
            /// default. Rather, they SHOULD require some user interaction for
            /// agreement to terms.
            if (Options.TermsOfService != null && (!@params.TermsOfServiceAgreed.HasValue || !@params.TermsOfServiceAgreed.Value))
            {
                throw new MalformedException("Must agree to terms of service");
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
        /// Validates array of contacts
        /// </summary>
        /// <param name="contacts">Array of contacts</param>
        /// <exception cref="ArgumentNullException"/>
        /// <returns cref="bool"></returns>
        protected bool ValidateContacts(string[] contacts)
        {
            #region Check arguments
            if (contacts is null)
            {
                throw new ArgumentNullException(nameof(contacts));
            }
            #endregion

            foreach (var contact in contacts)
            {
                try
                {
                    if (!OnValidateContact(contact))
                    {
                        return false;
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Validates contact
        /// </summary>
        /// <param name="contact">Contact</param>
        /// <returns cref="bool"/>
        protected virtual bool OnValidateContact(string contact)
        {
            return IsMailto(contact);
        }

        /// <summary>
        /// Validates mail
        /// </summary>
        /// <param name="contact">Mail</param>
        /// <returns cref="bool"/>
        protected bool IsMailto(string contact)
        {
            try
            {
                var url = new Uri(contact);
                return url.Scheme == Uri.UriSchemeMailto;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return false;
            }
        }

        /// <summary>
        /// Fills parameters
        /// </summary>
        /// <param name="account">Account</param>
        /// <param name="key">JSON Web Key (JWK)</param>
        /// <param name="params">New account parameters</param>
        protected virtual void OnCreateParams(IAccount account, JsonWebKey key, NewAccount @params)
        {
            account.Key = key;
            account.Contacts = @params.Contacts;
            account.TermsOfServiceAgreed = @params.TermsOfServiceAgreed;
        }

        /// <inheritdoc/>
        public IAccount Update(int accountId, UpdateAccount @params)
        {
            #region Check arguments
            if (@params is null)
            {
                throw new ArgumentNullException(nameof(@params));
            }
            if (@params.Contacts != null && !ValidateContacts(@params.Contacts))
            {
                throw new UnsupportedContactException();
            }
            #endregion

            // Get account
            var account = GetById(accountId);

            // Assign values
            OnUpdateParams(account, @params);

            // Save changes
            account = AccountRepository.Update(account);

            Logger.Info("Account {id} updated", account.Id);

            // Return JSON
            return account;
        }

        /// <summary>
        /// Fills parameters
        /// </summary>
        /// <param name="account">Account</param>
        /// <param name="params">Params to update</param>
        protected virtual void OnUpdateParams(IAccount account, UpdateAccount @params)
        {
            account.Contacts = @params.Contacts;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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
                throw new AcmeException(
                    ErrorType.Malformed,
                    "Key conflict",
                    System.Net.HttpStatusCode.Conflict);
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
