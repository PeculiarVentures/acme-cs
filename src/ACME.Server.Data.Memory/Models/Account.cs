﻿using System;
using System.Collections.Generic;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Protocol.Messages;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Server.Data.Memory.Models
{
    public class Account : BaseObject, IAccount
    {
        public AccountStatus Status { get; set; }
        public JsonWebKey Key { get; set; }
        public bool? TermsOfServiceAgreed { get; set; }
        public int? ExternalAccountId { get; set; }
        public ICollection<string> Contacts { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public static implicit operator Account(NewAccount @params)
        {
            var account = new Account
            {
                Status = AccountStatus.Valid,
            };

            if (@params.Contacts != null)
            {
                foreach (var item in @params.Contacts)
                {
                    account.Contacts.Add(item);
                }
            }

            return account;
        }
    }
}
