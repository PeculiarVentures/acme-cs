using System;
using PeculiarVentures.ACME.Protocol.Messages;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Server.Services
{
    public interface IAccountService
    {
        /// <summary>
        /// Creates a new <see cref="IAccount"/>
        /// </summary>
        /// <param name="key">Account's JSON web key</param>
        /// <param name="params">Params to create a new <see cref="IAccount"/></param>
        /// <returns><see cref="IAccount"/></returns>
        /// <exception cref="ArgumentNullException"/>
        IAccount Create(JsonWebKey key, NewAccount @params);
        /// <summary>
        /// Deactivates an <see cref="IAccount"/> by specified Id
        /// </summary>
        /// <param name="accountId"><see cref="IAccount"/> identifier</param>
        /// <returns></returns>
        /// <exception cref="AccountDoesNotExistException"/>
        IAccount Deactivate(int accountId);
        /// <summary>
        /// Returns <see cref="IAccount"/> by specified Id
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns>Account</returns>
        /// <exception cref="AccountDoesNotExistException"/>
        IAccount GetById(int accountId);
        /// <summary>
        /// Returns <see cref="IAccount"/> by specified JWK
        /// </summary>
        /// <param name="key">JSO Web Key</param>
        /// <returns><see cref="IAccount"/></returns>
        /// <exception cref="AccountDoesNotExistException"/>
        /// <exception cref="ArgumentNullException"/>
        IAccount GetByPublicKey(JsonWebKey key);
        /// <summary>
        /// Returns <see cref="IAccount"/> by specified JWK
        /// </summary>
        /// <param name="key">JSON web key</param>
        /// <returns><see cref="IAccount"/> or <see cref="null"/></returns>
        /// <exception cref="ArgumentNullException"/>
        IAccount FindByPublicKey(JsonWebKey key);
        /// <summary>
        /// Revokes an <see cref="IAccount"/>
        /// </summary>
        /// <param name="accountId"><see cref="IAccount"/> identifier</param>
        /// <returns><see cref="IAccount"/></returns>
        /// <exception cref="AccountDoesNotExistException"/>
        IAccount Revoke(int accountId);
        /// <summary>
        /// Updates an <see cref="IAccount"/>
        /// </summary>
        /// <param name="accountId"><see cref="IAccount"/> identifier</param>
        /// <param name="contacts">List of contacts</param>
        /// <returns><see cref="IAccount"/></returns>
        /// <exception cref="AccountDoesNotExistException"/>
        /// <exception cref="ArgumentNullException"/>
        IAccount Update(int accountId, string[] contacts);
        /// <summary>
        /// Changes key for <see cref="IAccount"/>
        /// </summary>
        /// <param name="accountId"><see cref="IAccount"/> identifier</param>
        /// <param name="key">A new key of <see cref="IAccount"/></param>
        /// <returns><see cref="IAccount"/></returns>
        /// <exception cref="AccountDoesNotExistException"/>
        /// <exception cref="MalformedException"/>
        /// <exception cref="ArgumentNullException"/>
        IAccount ChangeKey(int accountId, JsonWebKey key);

        IExternalAccount CreateExternalAccount(object data);
    }
}