using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;

namespace PeculiarVentures.ACME.Server.Data.Abstractions.Repositories
{
    public interface IAuthorizationRepository : IBaseRepository<IAuthorization>
    {
        /// <summary>
        /// Returns the last authz with specified identifier
        /// </summary>
        /// <param name="accountId">Identifier of account</param>
        /// <param name="identifier">Identifier</param>
        /// <returns>Authorization or Null</returns>
        IAuthorization GetByIdentifier(int accountId, Identifier identifier);
    }
}
