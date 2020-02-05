﻿using System;
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
        public Account Convert(IAccount account)
        {
            if (account is Models.Account item)
            {
                return new Account
                {
                    Id = item.Id,
                    Contacts = item.Contacts.ToArray(),
                    Status = item.Status,
                    TermsOfServiceAgreed = item.TermsOfServiceAgreed,
                    Key = item.PublicKey,
                };
            }
            throw new InvalidCastException("Incomming parameter doesn't belong to current repository");
        }

        public IAccount Create(JsonWebKey key, NewAccount @params)
        {
            Models.Account account = @params;
            account.PublicKey = key;
            return account;
        }

        public IAccount FindByPublicKey(JsonWebKey publicKey)
        {
            return Items.FirstOrDefault(o => o.PublicKey.Equals(publicKey));
        }
    }
}
