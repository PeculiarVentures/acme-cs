using System;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;

namespace PeculiarVentures.ACME.Server.Data.Abstractions.Repositories
{
    public interface IChallengeRepository : IBaseRepository<IChallenge>
    {
        IChallenge[] GetByAuthorization(int authzId);
    }
}
