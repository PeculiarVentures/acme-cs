using System.Linq;
using Microsoft.EntityFrameworkCore;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;
using PeculiarVentures.ACME.Server.Data.EF.Core.Models;

namespace GlobalSign.ACME.Server.Data.EF.Core.Repositories
{
    public class GsAuthorizationRepository : GsBaseRepository<IAuthorization, Authorization>, IAuthorizationRepository
    {
        public GsAuthorizationRepository(GsAcmeContext context) : base(context)
        {
        }

        public override DbSet<Authorization> Records => Context.Authorizations;

        public override IAuthorization GetById(int id)
        {
            return Records.FirstOrDefault(o => o.Id == id);
        }

        public IAuthorization GetByIdentifier(int accountId, PeculiarVentures.ACME.Protocol.Identifier identifier)
        {
            return Records
                .OrderByDescending(o => o.Id)
                .FirstOrDefault(o => o.AccountId == accountId && o.IdentifierValue.Type == identifier.Type && o.IdentifierValue.Value == identifier.Value);
        }
    }
}
