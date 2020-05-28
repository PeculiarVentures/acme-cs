using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;

namespace PeculiarVentures.ACME.Server.Services
{
    public interface IAuthorizationService
    {
        /// <summary>
        /// Returns <see cref="IAuthorization"/> by specified Id
        /// </summary>
        /// <param name="accountId"><see cref="IAccount"/> identifier</param>
        /// <param name="authzId"><see cref="IAuthorization"/> identifier</param>
        /// <exception cref="MalformedException"/>
        IAuthorization GetById(int accountId, int authzId);

        /// <summary>
        /// Returns actual <see cref="IAuthorization"/> by specified Id 
        /// </summary>
        /// <param name="accountId"><see cref="IAccount"/> identifier</param>
        /// <param name="identifier"><see cref="IAuthorization"/> identifier</param>
        IAuthorization GetActual(int accountId, Identifier identifier);

        /// <summary>
        /// Creates new authorization
        /// </summary>
        /// <param name="accountId"><see cref="IAccount"/> identifier</param>
        /// <param name="identifier"><see cref="IAuthorization"/> identifier</param>
        IAuthorization Create(int accountId, Identifier identifier);

        /// <summary>
        /// Refreshes status an <see cref="IAuthorization"/> 
        /// </summary>
        /// <param name="item"><see cref="IAuthorization"/></param>
        IAuthorization RefreshStatus(IAuthorization item);
    }
}