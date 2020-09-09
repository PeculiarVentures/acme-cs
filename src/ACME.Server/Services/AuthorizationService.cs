using System;
using System.Linq;
using Microsoft.Extensions.Options;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;

namespace PeculiarVentures.ACME.Server.Services
{
    /// <summary>
    /// Authorization service
    /// </summary>
    public class AuthorizationService : BaseService, IAuthorizationService
    {
        public AuthorizationService(
            IAuthorizationRepository authorizationRepository,
            IChallengeService challengeService,
            IAccountService accountService,
            IAccountSecurityService accountSecurityService,
            IOptions<ServerOptions> options)
            : base(options)
        {
            AuthorizationRepository = authorizationRepository ?? throw new ArgumentNullException(nameof(authorizationRepository));
            ChallengeService = challengeService ?? throw new ArgumentNullException(nameof(challengeService));
            AccountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            AccountSecurityService = accountSecurityService ?? throw new ArgumentNullException(nameof(accountSecurityService));
        }

        public IAuthorizationRepository AuthorizationRepository { get; }
        public IChallengeService ChallengeService { get; }
        public IAccountService AccountService { get; }
        public IAccountSecurityService AccountSecurityService { get; }

        /// <inheritdoc/>
        public virtual IAuthorization Create(int accountId, Identifier identifier)
        {
            // Create Authorization
            var authz = AuthorizationRepository.Create();

            // Fill params
            OnCreateParams(authz, accountId, identifier);

            // Save authorization
            var addedAuthz = AuthorizationRepository.Add(authz);

            // Add challenges
            var http = ChallengeService.Create(addedAuthz.Id, "http-01");
            //var tls = ChallengeService.Create(addedAuthz.Id, "tls-01");
            //var dns = ChallengeService.Create(addedAuthz.Id, "dns-01");

            Logger.Info("Authorization {id} created", authz.Id);

            return addedAuthz;
        }

        /// <summary>
        /// Fills parameters
        /// </summary>
        /// <param name="authz"><see cref="IAuthorization"/></param>
        /// <param name="accountId"><see cref="IAccount"/> identifier</param>
        /// <param name="identifier"><see cref="Identifier"/></param>
        protected virtual void OnCreateParams(IAuthorization authz, int accountId, Identifier identifier)
        {
            authz.Identifier.Type = identifier.Type;
            authz.Identifier.Value = identifier.Value;
            authz.AccountId = accountId;
            authz.Status = AuthorizationStatus.Pending;
            authz.Expires = DateTime.UtcNow.AddDays(Options.ExpireAuthorizationDays);
        }

        /// <inheritdoc/>
        public IAuthorization GetActual(int accountId, Identifier identifier)
        {
            // Get auth from repository
            var authz = AuthorizationRepository.GetByIdentifier(accountId, identifier);
            if (authz == null)
            {
                return null;
            }

            var updatedAuthz = RefreshStatus(authz);

            if (authz.Status == AuthorizationStatus.Invalid
                || authz.Status == AuthorizationStatus.Expired
                || authz.Status == AuthorizationStatus.Revoked
                || authz.Status == AuthorizationStatus.Deactivated)
            {
                return null;
            }

            return updatedAuthz;
        }

        /// <inheritdoc/>
        public IAuthorization GetById(int accountId, int authzId)
        {
            var authz = AuthorizationRepository.GetById(authzId);

            if (authz == null)
            {
                throw new MalformedException("Authorization doesn't exist");
            }

            var updatedAuthz = RefreshStatus(authz);

            AccountSecurityService.CheckAccess(new AccountAccess
            {
                Account = AccountService.GetById(accountId),
                Target = updatedAuthz,
            });

            return updatedAuthz;
        }

        /// <inheritdoc/>
        public IAuthorization RefreshStatus(IAuthorization item)
        {
            if (item.Status != AuthorizationStatus.Pending && item.Status != AuthorizationStatus.Valid)
            {
                return item;
            }

            if (item.Expires != null
                && item.Expires < DateTime.UtcNow)
            {
                // Check Expire
                item.Status = AuthorizationStatus.Expired;
                AuthorizationRepository.Update(item);
            }
            else
            {
                // Check status
                var challenges = ChallengeService.GetByAuthorization(item.Id);
                if (challenges.Any(o => o.Status == ChallengeStatus.Valid))
                {
                    item.Status = AuthorizationStatus.Valid;
                    AuthorizationRepository.Update(item);
                }
                else if (challenges.Any(o => o.Status == ChallengeStatus.Invalid))
                {
                    item.Status = AuthorizationStatus.Invalid;
                    AuthorizationRepository.Update(item);
                }
            }

            Logger.Info("Authorization {id} status updated to {status}", item.Id, item.Status);

            return item;
        }
    }
}