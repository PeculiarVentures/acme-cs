using System;
using System.Linq;
using Microsoft.Extensions.Options;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;

namespace PeculiarVentures.ACME.Server.Services
{
    public class AuthorizationService : BaseService, IAuthorizationService
    {
        public AuthorizationService(
            IAuthorizationRepository authorizationRepository,
            IChallengeService challengeService,
            IOptions<ServerOptions> options)
            : base(options)
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
            authz.Expires = DateTime.UtcNow.AddDays(Options.ExpireAuthorizationDays); 
            authz.AccountId = accountId;
            authz.Status = AuthorizationStatus.Pending;

            var addedAuthz = AuthorizationRepository.Add(authz);

            // Add challenges
            var http = ChallengeService.Create(addedAuthz.Id, "http-01");
            //var tls = ChallengeService.Create(addedAuthz.Id, "tls-01");
            //var dns = ChallengeService.Create(addedAuthz.Id, "dns-01");

            return addedAuthz;
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
                ?? throw new MalformedException("Authorization doesn't exist");

            if (authz.AccountId != accountId)
            {
                throw new MalformedException("Access denied");
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
                && item.Expires < DateTime.UtcNow)
            {
                item.Status = AuthorizationStatus.Expired;
                AuthorizationRepository.Update(item);
            }
            else
            {
                var challenges = ChallengeService.GetByAuthorization(item.Id);
                if (challenges.Any(o => o.Status == ChallengeStatus.Valid))
                {
                    item.Status = AuthorizationStatus.Valid;
                    AuthorizationRepository.Update(item);
                }
                else if (challenges.All(o => o.Status == ChallengeStatus.Invalid))
                {
                    item.Status = AuthorizationStatus.Invalid;
                    AuthorizationRepository.Update(item);
                }
            }

            return item;
        }
    }
}