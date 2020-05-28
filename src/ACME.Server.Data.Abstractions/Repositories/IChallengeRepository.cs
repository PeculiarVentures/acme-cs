using PeculiarVentures.ACME.Server.Data.Abstractions.Models;

namespace PeculiarVentures.ACME.Server.Data.Abstractions.Repositories
{
    public interface IChallengeRepository : IBaseRepository<IChallenge>
    {
        /// <summary>
        /// Returns the challenge by identifier of authorization
        /// </summary>
        /// <param name="authzId"></param>
        /// <returns></returns>
        IChallenge[] GetByAuthorization(int authzId);
    }
}
