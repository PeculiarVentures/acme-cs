﻿using Microsoft.EntityFrameworkCore;
using PeculiarVentures.ACME.Helpers;
using PeculiarVentures.ACME.Protocol.Messages;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;
using PeculiarVentures.ACME.Server.Data.EF.Core.Models;
using PeculiarVentures.ACME.Web;
using System.Linq;

namespace GlobalSign.ACME.Server.Data.EF.Core.Repositories
{
    public class GsAccountRepository : GsBaseRepository<IAccount, Account>, IAccountRepository
    {
        public GsAccountRepository(GsAcmeContext context) : base(context)
        {
        }

        public override DbSet<Account> Records => Context.Accounts;

        public IAccount FindByPublicKey(JsonWebKey publicKey)
        {
            var thumbprint = Base64Url.Encode(publicKey.GetThumbprint());
            return Records.FirstOrDefault(o => o.KeyIdentifier == thumbprint);
        }

        public override IAccount GetById(int id)
        {
            return Records.FirstOrDefault(o => o.Id == id);
        }
    }
}
