using System;
using System.Linq;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;

namespace PeculiarVentures.ACME.Server.Services
{
    public class ConverterService : IConverterService
    {
        public Account ToAccount(IAccount data)
        {
            return new Account
            {
                Id = data.Id,
                Contacts = data.Contacts.ToArray(),
                Status = data.Status,
                TermsOfServiceAgreed = data.TermsOfServiceAgreed,
                Key = data.Key,
                CreatedAt = data.CreatedAt.ToString(),
                ExternalAccountBinding = data.ExternalAccountBinding,
            };
        }

        public Challenge ToChallenge(IChallenge data)
        {
            return new Challenge
            {
                Status = data.Status,
                Type = data.Type,
                Validated = data.Validated,
                Error = data.Error != null ? ToError(data.Error) : null,
                Token = data.Token,
                Url = data.Id.ToString(),
            };
        }

        public Error ToError(IError data)
        {
            return new Error
            {
                Detail = data.Detail,
                Type = data.Type,
                SubProblems = data.SubProblems.Select(o => ToError(o)).ToArray(),
            };
        }

        public Authorization ToAuthorization(IAuthorization data)
        {
            return new Authorization
            {
                Expires = data.Expires,
                Identifier = new Identifier
                {
                    Type = data.Identifier.Type,
                    Value = data.Identifier.Value,
                },
                Status = data.Status,
                Wildcard = data.Wildcard,
                Challenges = data.Challenges.Select(o =>
                    ToChallenge(o))
                    .ToList(),
            };
        }

        public Order ToOrder(IOrder data)
        {
            return new Order
            {
                Id = data.Id,
                Identifiers = data.Authorizations.Select(o =>
                    new Identifier(o.Identifier.Type, o.Identifier.Value)).ToArray(),
                Authorizations = data.Authorizations.Select(o => $"{o.Id}").ToArray(),
                Status = data.Status,
                NotBefore = data.NotBefore,
                NotAfter = data.NotAfter,
                Expires = data.Expires,
                Error = data.Error == null ? null : ToError(data.Error),
                Finalize = $"{data.Id}",
                Certificate = data.Certificate == null ? null : $"{data.Certificate.Thumbprint}"
            };
        }
    }
}
