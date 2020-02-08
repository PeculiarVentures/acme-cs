using PeculiarVentures.ACME.Protocol.Messages;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Server.Services
{
    public interface IAccountService
    {
        /// <summary>
        /// Creates a new account
        /// </summary>
        /// <param name="key"></param>
        /// <param name="params"></param>
        /// <returns></returns>
        IAccount Create(JsonWebKey key, NewAccount @params);
        IAccount Deactivate(int accountId);
        /// <summary>
        /// Returns account by specified Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Account</returns>
        /// <exception cref="AccountDoesNotExistException"/>
        IAccount GetById(int id);
        /// <summary>
        /// Returns account by specified JWK
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Account or Null</returns>
        IAccount FindByPublicKey(JsonWebKey key);
        IAccount Revoke(int accountId);
        /// <summary>
        /// Updates an account
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="contacts"></param>
        /// <returns></returns>
        /// <exception cref="AccountDoesNotExistException"/>
        IAccount Update(int accountId, string[] contacts);
        IAccount ChangeKey(int accountId, JsonWebKey key);
    }
}