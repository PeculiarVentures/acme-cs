using System.Linq;
using Microsoft.EntityFrameworkCore;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;
using PeculiarVentures.ACME.Server.Data.EF.Core.Models;

namespace PeculiarVentures.ACME.Server.Data.EF.Core.Repositories
{
    public class AuthorizationRepository : BaseRepository<IAuthorization, Authorization>, IAuthorizationRepository
    {
        public AuthorizationRepository(AcmeContext context) : base(context)
        {
        }

        public override DbSet<Authorization> Records => Context.Authorizations;

        public override IAuthorization GetById(int id)
        {
            return Records.FirstOrDefault(o => o.Id == id);
        }

        public IAuthorization GetByIdentifier(int accountId, Protocol.Identifier identifier)
        {
            return Records
                .OrderByDescending(o => o.Id)
                .FirstOrDefault(o => o.AccountId == accountId && o.IdentifierValue.Type == identifier.Type && o.IdentifierValue.Value == identifier.Value);
        }
    }
}
