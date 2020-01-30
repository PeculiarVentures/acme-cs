using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Protocol.Messages;
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
        Account Create(JsonWebKey key, NewAccount @params);
        Account Deactivate(int accountId);
        /// <summary>
        /// Returns account by specified Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Account GetById(int id);
        /// <summary>
        /// Returns account by specified JWK
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Account GetByPublicKey(JsonWebKey key);
        Account Revoke(int accountId);
        /// <summary>
        /// Updates an account
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="contacts"></param>
        /// <returns></returns>
        /// <exception cref="AccountDoesNotExistException"/>
        Account Update(int accountId, string[] contacts);
    }
}