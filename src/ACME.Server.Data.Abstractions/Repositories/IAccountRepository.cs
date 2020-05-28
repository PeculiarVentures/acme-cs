using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Server.Data.Abstractions.Repositories
{
    public interface IAccountRepository : IBaseRepository<IAccount>
    {
        /// <summary>
        /// Returns the account or null by public key
        /// </summary>
        /// <param name="publicKey">JSON Web Key (JWK)</param>
        /// <returns></returns>
        IAccount FindByPublicKey(JsonWebKey publicKey);
    }
}
