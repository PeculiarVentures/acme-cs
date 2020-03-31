using System;
using System.Linq;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Protocol.Messages;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Server.Data.Memory.Repositories
{
    public class AccountRepository : BaseRepository<IAccount>, IAccountRepository
    {
        public override IAccount Create()
        {
            return new Models.Account();
        }
        
        public IAccount FindByPublicKey(JsonWebKey publicKey)
        {
            return Items.FirstOrDefault(o => o.Key.Equals(publicKey));
        }
    }
}
