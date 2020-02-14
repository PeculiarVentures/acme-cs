using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using PeculiarVentures.ACME.Helpers;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Web;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Collections.ObjectModel;

namespace PeculiarVentures.ACME.Server.Data.EF.Core.Models
{
    public class Account : BaseObject, IAccount
    {
        public AccountStatus Status { get; set; } = AccountStatus.Valid;

        public string KeyIdentifier { get; set; }

        [Column(nameof(Key))]
        [Required]
        public string KeyValue { get; set; }

        [NotMapped]
        public JsonWebKey Key
        {
            get
            {
                if (KeyValue == null)
                {
                    return null;
                }
                return JsonConvert.DeserializeObject<JsonWebKey>(KeyValue);
            }
            set
            {
                KeyIdentifier = Base64Url.Encode(value.GetThumbprint());
                KeyValue = JsonConvert.SerializeObject(value);
            }
        }

        public bool TermsOfServiceAgreed { get; set; }

        public int? ExternalAccountId { get; set; }

        [Column(nameof(Contacts))]
        public string ContactsValue { get; set; }

        [NotMapped]
        public ICollection<string> Contacts
        {
            get => ContactsValue == null
                ? new List<string>()
                : JsonConvert.DeserializeObject<string[]>(ContactsValue).ToList();
            set => ContactsValue = JsonConvert.SerializeObject(value);
        }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<Authorization> Authorizations { get; set; } = new Collection<Authorization>();
    }
}
