using System;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Protocol.Messages;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Server.Data.Abstractions.Repositories
{
    public interface IAccountRepository : IBaseRepository<IAccount>
    {
        IAccount GetByPublicKey(JsonWebKey publicKey);
        IAccount Create(JsonWebKey key, NewAccount @params);

        Account Convert(IAccount account);
    }
}
