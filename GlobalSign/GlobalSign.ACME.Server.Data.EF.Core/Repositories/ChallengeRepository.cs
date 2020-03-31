using System.Linq;
using Microsoft.EntityFrameworkCore;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;
using PeculiarVentures.ACME.Server.Data.EF.Core.Models;

namespace GlobalSign.ACME.Server.Data.EF.Core.Repositories
{
    public class GsChallengeRepository : GsBaseRepository<IChallenge, Challenge>, IChallengeRepository
    {
        public GsChallengeRepository(GsAcmeContext context) : base(context)
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
