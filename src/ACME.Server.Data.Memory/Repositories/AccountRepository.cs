using System;
using System.Linq;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Protocol.Messages;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;
using PeculiarVentures.ACME.Web;

namespace PeculiarVenturs.ACME.Server.Data.Memory.Repositories
{
    public class AccountRepository : BaseRepository<IAccount>, IAccountRepository
    {
        public IAccount Create(JsonWebKey key, NewAccount @params)
        {
            Models.Account account = @params;
            account.PublicKey = key;
            return account;
        }

        public IAccount GetByPublicKey(JsonWebKey publicKey)
        {
            return Items.FirstOrDefault(o => o.PublicKey.Equals(publicKey));
        }
    }
}
