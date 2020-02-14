using System;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using PeculiarVentures.ACME.Helpers;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Protocol.Messages;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Server.Services
{
    public class AccountService : IAccountService
    {
        public AccountService(
            IOptions<ServerOptions> options,
            IAccountRepository accountRepository,
            IExternalAccountRepository externalAccountRepository)
        {
            Options = options?.Value
                ?? throw new ArgumentNullException(nameof(options));
            AccountRepository = accountRepository
                ?? throw new ArgumentNullException(nameof(accountRepository));
            ExternalAccountRepository = externalAccountRepository
                ?? throw new ArgumentNullException(nameof(externalAccountRepository));
        }

        public ServerOptions Options { get; }
        public IAccountRepository AccountRepository { get; }
        public IExternalAccountRepository ExternalAccountRepository { get; }

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
                    if (!ValidateExternalAccount(@params.ExternalAccountBinding))
                    {
                        throw new MalformedException("externalAccountBinding has wrong signature"); // TODO check rfc error
                    }
                    var eabPayload = @params.ExternalAccountBinding.GetPayload<JsonWebKey>();
                    if (!eabPayload.Equals(key))
                    {
                        throw new MalformedException("Signed content in externalAccountBinding doesn't match to requirement"); // TODO check rfc error
                    }
                    var eabProtected = @params.ExternalAccountBinding.GetProtected();
                    var eab = GetExternalAccountById(eabProtected.KeyID);
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

        public IExternalAccount CreateExternalAccount(object data)
        {
            #region Check arguments
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            #endregion

            var macKey = new byte[Options.ExternalAccountOptions.KeyLength >> 3];
            new RNGCryptoServiceProvider().GetBytes(macKey);

            var account = ExternalAccountRepository.Create();
            account.Key = Base64Url.Encode(macKey);
            account.Account = data;
            if (Options.ExternalAccountOptions.ExpiresMinutes != 0)
            {
                account.Expires = DateTime.UtcNow.AddMinutes(Options.ExternalAccountOptions.ExpiresMinutes);
            }
            account = ExternalAccountRepository.Add(account);

            return account;
        }

        public bool ValidateExternalAccount(JsonWebSignature token)
        {
            #region Check arguments
            if (token is null)
            {
                throw new ArgumentNullException(nameof(token));
            }
            #endregion

            var @prtoected = token.GetProtected();

            var externalAccount = GetExternalAccountById(@prtoected.KeyID);
            var key = Base64Url.Decode(externalAccount.Key);

            return token.Verify(key);
        }

        public IExternalAccount GetExternalAccountById(string kid)
        {
            var id = GetKeyIdentifier(kid);
            return GetExternalAccountById(id);
            
        }
        public IExternalAccount GetExternalAccountById(int id)
        {
            var externalAccount = ExternalAccountRepository.GetById(id)
                ?? throw new MalformedException("External account does not exist");
            if (externalAccount.Expires < DateTime.UtcNow)
            {
                throw new MalformedException("External expired");
            }
            return externalAccount;
        }

        public int GetKeyIdentifier(string kid)
        {
            var match = new Regex("([0-9]+)$").Match(kid);
            if (!match.Success)
            {
                throw new MalformedException("Cannot get key identifier from the 'kid'");
            }
            return int.Parse(match.Groups[1].Value);
        }
    }
}
