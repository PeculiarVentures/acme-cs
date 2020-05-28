using System;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using PeculiarVentures.ACME.Helpers;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Server.Services
{
    /// <summary>
    /// External account service
    /// </summary>
    public class ExternalAccountService : BaseService, IExternalAccountService
    {
        public ExternalAccountService(
            IExternalAccountRepository externalAccountRepository,
            IOptions<ServerOptions> options)
            : base(options)
        {
            ExternalAccountRepository = externalAccountRepository
                ?? throw new ArgumentNullException(nameof(externalAccountRepository));
        }

        public IExternalAccountRepository ExternalAccountRepository { get; }

        /// <inheritdoc/>
        public IExternalAccount Create(object data)
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
            OnCreateParams(account, data, macKey);
            account = ExternalAccountRepository.Add(account);

            Logger.Info("External account {id} created", account.Id);

            return account;
        }

        /// <summary>
        /// Fills parameters
        /// </summary>
        /// <param name="account"></param>
        /// <param name="data"></param>
        /// <param name="macKey"></param>
        protected virtual void OnCreateParams(IExternalAccount account, object data, byte[] macKey)
        {
            account.Key = Base64Url.Encode(macKey);
            account.Account = data;
            account.Status = Protocol.ExternalAccountStatus.Pending;
            if (Options.ExternalAccountOptions.ExpiresMinutes != 0)
            {
                account.Expires = DateTime.UtcNow.AddMinutes(Options.ExternalAccountOptions.ExpiresMinutes);
            }
        }

        /// <inheritdoc/>
        public IExternalAccount Validate(JsonWebKey accountKey, JsonWebSignature token)
        {
            #region Check arguments
            if (accountKey is null)
            {
                throw new ArgumentNullException(nameof(accountKey));
            }
            if (token is null)
            {
                throw new ArgumentNullException(nameof(token));
            }
            #endregion

            var @prtoected = token.GetProtected();

            var eabPayload = token.GetPayload<JsonWebKey>();
            if (!eabPayload.Equals(accountKey))
            {
                throw new MalformedException("Signed content in externalAccountBinding doesn't match to requirement"); // TODO check rfc error
            }

            var externalAccount = GetById(@prtoected.KeyID);
            if (externalAccount.Status != Protocol.ExternalAccountStatus.Pending)
            {
                throw new MalformedException("External account has wrong status"); // TODO check rfc error
            }

            var key = Base64Url.Decode(externalAccount.Key);

            externalAccount.Status = token.Verify(key)
                ? Protocol.ExternalAccountStatus.Valid
                : Protocol.ExternalAccountStatus.Invalid;
            ExternalAccountRepository.Update(externalAccount);

            Logger.Info("External account {id} status updated to {status}", externalAccount.Id, externalAccount.Status);

            return externalAccount;
        }

        /// <inheritdoc/>
        public IExternalAccount GetById(string kid)
        {
            var id = GetKeyIdentifier(kid);
            return GetById(id);

        }

        /// <inheritdoc/>
        public IExternalAccount GetById(int id)
        {
            var externalAccount = ExternalAccountRepository.GetById(id)
                ?? throw new MalformedException("External account does not exist");
            if (externalAccount.Status == Protocol.ExternalAccountStatus.Pending && externalAccount.Expires < DateTime.UtcNow)
            {
                externalAccount.Status = Protocol.ExternalAccountStatus.Expired;
                ExternalAccountRepository.Update(externalAccount);

                Logger.Info("External account {id} status updated to {status}", externalAccount.Id, externalAccount.Status);
            }
            return externalAccount;
        }
    }
}
