using System;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using PeculiarVentures.ACME.Helpers;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;

namespace PeculiarVentures.ACME.Server.Services
{
    /// <summary>
    /// Challenge service
    /// </summary>
    public class ChallengeService : BaseService, IChallengeService
    {
        public ChallengeService(IChallengeRepository challengeRepository,
                                IAuthorizationRepository authorizationRepository,
                                IAccountService accountService,
                                IOptions<ServerOptions> options)
            : base(options)
        {
            ChallengeRepository = challengeRepository
                ?? throw new ArgumentNullException(nameof(challengeRepository));
            AuthorizationRepository = authorizationRepository
                ?? throw new ArgumentNullException(nameof(authorizationRepository));
            AccountService = accountService
                ?? throw new ArgumentNullException(nameof(accountService));
        }

        private IChallengeRepository ChallengeRepository { get; }
        private IAuthorizationRepository AuthorizationRepository { get; }
        private IAccountService AccountService { get; }

        /// <inheritdoc/>
        public IChallenge Create(int authzId, string type)
        {
            var challenge = ChallengeRepository.Create();
            OnCreateParams(challenge, authzId, type);

            ChallengeRepository.Add(challenge);

            Logger.Info("Challenge {id} created", challenge.Id);

            return challenge;
        }

        /// <summary>
        /// Fills parameters
        /// </summary>
        /// <param name="challenge"></param>
        /// <param name="authzId"></param>
        /// <param name="type"></param>
        protected virtual void OnCreateParams(IChallenge challenge, int authzId, string type)
        {
            challenge.Type = type;
            challenge.AuthorizationId = authzId;
            challenge.Status = ChallengeStatus.Pending;
            var httpToken = new byte[20];
            new System.Security.Cryptography.RNGCryptoServiceProvider().GetBytes(httpToken);
            challenge.Token = Base64Url.Encode(httpToken);
        }

        /// <inheritdoc/>
        public IChallenge[] GetByAuthorization(int id)
        {
            var chall = ChallengeRepository.GetByAuthorization(id);
            if (chall == null)
            {
                throw new MalformedException("Challenge does not exist");
            }
            return chall;
        }

        /// <inheritdoc/>
        public IChallenge GetById(int id)
        {
            var chall = ChallengeRepository.GetById(id);
            if (chall == null)
            {
                throw new MalformedException("Challenge does not exist");
            }
            return chall;
        }

        /// <inheritdoc/>
        public void Validate(IChallenge challenge)
        {
            if (challenge.Status == ChallengeStatus.Pending)
            {
                challenge.Status = ChallengeStatus.Processing;
                ChallengeRepository.Update(challenge);

                Logger.Info("Challenge {id} status updated to {status}", challenge.Id, challenge.Status);

                Task
                    .Run(() =>
                    {
                        // validate challenge
                        switch (challenge.Type)
                        {
                            case "http-01":
                                ValidateHttpChallenge(challenge);
                                break;
                            default:
                                throw new Exception($"Unsupported Challenge type '{challenge.Type}'");
                        }
                    })
                    .ContinueWith(t =>
                    {
                        // Fault validate
                        if (t.IsFaulted)
                        {
                            Error err = t.Exception.InnerException;
                            challenge.Error = ChallengeRepository.CreateError();
                            challenge.Error.Detail = err.Detail;
                            challenge.Error.Type = err.Type;
                            challenge.Status = ChallengeStatus.Invalid;
                            ChallengeRepository.Update(challenge);
                        }

                        //Complete validate 
                        if (t.IsCompleted)
                        {
                            if (challenge.Status == ChallengeStatus.Processing)
                            {
                                challenge.Status = ChallengeStatus.Valid;
                                challenge.Validated = DateTime.UtcNow;
                                ChallengeRepository.Update(challenge);
                            }
                        }

                        Logger.Info("Challenge {id} status updated to {status}", challenge.Id, challenge.Status);
                    });
            }
            else
            {
                throw new MalformedException("Wrong challenge status");
            }

        }

        /// <summary>
        /// Validates the http challenge
        /// </summary>
        /// <param name="challenge"><see cref="IChallenge"/></param>
        private void ValidateHttpChallenge(IChallenge challenge)
        {
            var authz = AuthorizationRepository.GetById(challenge.AuthorizationId) ?? throw new MalformedException("Cannot get Authorization by Id");
            var url = $"http://{authz.Identifier.Value}/.well-known/acme-challenge/{challenge.Token}";
            var request = (HttpWebRequest)WebRequest.Create(url);

#if !DEBUG
            var response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var stream = response.GetResponseStream();
                var reader = new StreamReader(stream);
                var text = reader.ReadToEnd();

                //Accounts.GetById(challenge.Authorization.AccountId
                var account = AccountService.GetById(authz.AccountId);
                var thumbprint = Base64Url.Encode(account.Key.GetThumbprint());
                var controlValue = $"{challenge.Token}.{thumbprint}";

                if (!controlValue.Equals(text))
                {
                    var errMessage = "The key authorization file from the server did not match this challenge.";
                    throw new UnauthorizedException(errMessage);
                }
            }
            else
            {
                throw new Exception("Respons status is not 200(OK)");
            }
#else
            Logger.Warn("HTTP challenge validation is disabled fo DEBUG mode");
#endif
        }


    }
}
