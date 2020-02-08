using System;
using System.Linq;
using PeculiarVentures.ACME.Helpers;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;

namespace PeculiarVentures.ACME.Server.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        public AuthorizationService(IAuthorizationRepository authorizationRepository, IChallengeService challengeService)
        {
            AuthorizationRepository = authorizationRepository ?? throw new ArgumentNullException(nameof(authorizationRepository));
            ChallengeService = challengeService ?? throw new ArgumentNullException(nameof(challengeService));
        }

        public IAuthorizationRepository AuthorizationRepository { get; }
        public IChallengeService ChallengeService { get; }

        public virtual IAuthorization Create(int accountId, Identifier identifier)
        {
            var authz = AuthorizationRepository.Create();
            authz.Identifier.Type = identifier.Type;
            authz.Identifier.Value = identifier.Value;
            authz.AccountId = accountId;
            authz.Status = AuthorizationStatus.Pending;

            // Add challenges
            var http = ChallengeService.Create(accountId, "http-01");
            var tls = ChallengeService.Create(accountId, "tls-01");
            var dns = ChallengeService.Create(accountId, "dns-01");

            authz.Challenges.Add(http);
            authz.Challenges.Add(tls);
            authz.Challenges.Add(dns);

            return AuthorizationRepository.Add(authz);
        }

        public IAuthorization GetActual(int accountId, Identifier identifier)
        {
            // Get auth from repository
            var authz = AuthorizationRepository.GetByIdentifier(accountId, identifier);
            if (authz == null)
            {
                return null;
            }

            var updatedAuthz = RefreshStatus(authz);

            return updatedAuthz;
        }

        public IAuthorization GetById(int accountId, int authzId)
        {
            var authz = AuthorizationRepository.GetById(authzId)
                ?? throw new MalformedException("Authorization doesn't exist"); // TODO Check RFC

            if (authz.AccountId != accountId)
            {
                throw new MalformedException("Access denied"); // TODO Check RFC
            }

            var updatedAuthz = RefreshStatus(authz);
            return updatedAuthz;
        }

        public IAuthorization RefreshStatus(IAuthorization item)
        {
            if (item.Status != AuthorizationStatus.Pending && item.Status != AuthorizationStatus.Valid)
            {
                return item;
            }

            if (item.Expires != null
                && item.Expires < DateTime.Now)
            {
                item.Status = AuthorizationStatus.Expired;
                AuthorizationRepository.Update(item);
            }
            else
            {
                if (item.Challenges.Any(o => o.Status == ChallengeStatus.Valid))
                {
                    item.Status = AuthorizationStatus.Valid;
                    AuthorizationRepository.Update(item);
                }
                else if (item.Challenges.All(o => o.Status == ChallengeStatus.Invalid))
                {
                    item.Status = AuthorizationStatus.Invalid;
                    AuthorizationRepository.Update(item);
                }
            }

            return item;
        }
    }
}