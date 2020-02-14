using System;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;
using PeculiarVentures.ACME.Server.Data.Memory.Models;

namespace PeculiarVentures.ACME.Server.Data.Memory.Repositories
{
    public class ExternalAccountRepository : BaseRepository<IExternalAccount>, IExternalAccountRepository
    {
        public override IExternalAccount Create()
        {
            return new ExternalAccount();
        }
    }
}
