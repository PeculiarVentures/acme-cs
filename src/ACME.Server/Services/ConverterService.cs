using System;
using System.Linq;
using Microsoft.Extensions.Options;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;

namespace PeculiarVentures.ACME.Server.Services
{
    public class ConverterService : BaseService, IConverterService
    {
        public ConverterService(
            IAuthorizationRepository authorizationRepository,
            IChallengeRepository challengeRepository,
            IOrderAuthorizationRepository orderAuthorizationRepository,
            IExternalAccountRepository externalAccountRepository,
            IOptions<ServerOptions> options)
            : base(options)
        {
            AuthorizationRepository = authorizationRepository
                ?? throw new ArgumentNullException(nameof(authorizationRepository));
            ChallengeRepository = challengeRepository
                ?? throw new ArgumentNullException(nameof(challengeRepository));
            OrderAuthorizationRepository = orderAuthorizationRepository
                ?? throw new ArgumentNullException(nameof(orderAuthorizationRepository));
            ExternalAccountRepository = externalAccountRepository
                ?? throw new ArgumentNullException(nameof(externalAccountRepository));
        }

        private IExternalAccountRepository ExternalAccountRepository { get; }
        private IAuthorizationRepository AuthorizationRepository { get; }
        private IChallengeRepository ChallengeRepository { get; }
        private IOrderAuthorizationRepository OrderAuthorizationRepository { get; }

        public Account ToAccount(IAccount data)
        {
            var account = new Account
            {
                Id = data.Id,
                Contacts = data.Contacts.ToArray(),
                Status = data.Status,
                TermsOfServiceAgreed = data.TermsOfServiceAgreed,
                Key = data.Key,
                CreatedAt = data.CreatedAt,
            };
            if (ExternalAccountRepository != null && data.ExternalAccountId != null)
            {
                account.ExternalAccountBinding = ExternalAccountRepository.GetById(account.Id).Account;
            }
            return account;
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
            var challenges = ChallengeRepository.GetByAuthorization(data.Id);
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
                Challenges = challenges.Select(o =>
                    ToChallenge(o))
                    .ToList(),
            };
        }

        public Order ToOrder(IOrder data)
        {
            var authzs = OrderAuthorizationRepository.GetByOrder(data.Id)
                .Select(o => AuthorizationRepository.GetById(o.AuthorizationId))
                .ToArray();

            return new Order
            {
                Id = data.Id,
                Identifiers = authzs.Select(o =>
                    new Identifier(o.Identifier.Type, o.Identifier.Value)).ToArray(),
                Authorizations = authzs.Select(o => $"{o.Id}").ToArray(),
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
