using System;
using System.Linq;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;
using PeculiarVentures.ACME.Server.Data.Memory.Models;

namespace PeculiarVentures.ACME.Server.Data.Memory.Repositories
{
    public class ChallengeRepository : BaseRepository<IChallenge>, IChallengeRepository
    {
        public override IChallenge Create()
        {
            return new Challenge();
        }

        public IChallenge[] GetByAuthorization(int id)
        {
            return Items
                .Where(o => o.AuthorizationId == id)
                .ToArray();
        }
    }
}
