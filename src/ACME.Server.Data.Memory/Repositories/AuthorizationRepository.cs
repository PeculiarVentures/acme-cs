using System;
using System.Linq;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;

namespace PeculiarVentures.ACME.Server.Data.Memory.Repositories
{
    public class AuthorizationRepository : BaseRepository<IAuthorization>, IAuthorizationRepository
    {
        public override IAuthorization Create()
        {
            return new Models.Authorization();
        }

        public IAuthorization GetById(int accountId, int authzId)
        {
            return Items.FirstOrDefault(o => o.AccountId == accountId && o.Id == authzId);
        }
        
        public IAuthorization GetByIdentifier(int accountId, Identifier identifier)
        {
            if (identifier is null)
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            return Items
                .Where(o => o.AccountId == accountId)
                .Where(o => o.Identifier.Type.Equals(identifier.Type, StringComparison.CurrentCultureIgnoreCase)
                    && o.Identifier.Value.Equals(identifier.Value, StringComparison.CurrentCultureIgnoreCase))
                .LastOrDefault();
        }
    }
}
