using System.Linq;
using Microsoft.EntityFrameworkCore;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;
using PeculiarVentures.ACME.Server.Data.EF.Core.Models;

namespace PeculiarVentures.ACME.Server.Data.EF.Core.Repositories
{
    public class ChallengeRepository : BaseRepository<IChallenge, Challenge>, IChallengeRepository
    {
        public ChallengeRepository(AcmeContext context) : base(context)
        {
        }

        public override DbSet<Challenge> Records => Context.Challenges;

        public IChallenge[] GetByAuthorization(int id)
        {
            return Records.Where(o => o.AuthorizationId == id).ToArray();
        }

        public override IChallenge GetById(int id)
        {
            return Records.FirstOrDefault(o => o.Id == id);
        }
    }
}
