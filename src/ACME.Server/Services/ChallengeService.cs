using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using PeculiarVentures.ACME.Helpers;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;

namespace PeculiarVentures.ACME.Server.Services
{
    public class ChallengeService : IChallengeService
    {

        public ChallengeService(IChallengeRepository challengeRepository,
                                IAccountService accountService,
                                IAuthorizationRepository authorizationRepository)
        {
            ChallengeRepository = challengeRepository
                ?? throw new ArgumentNullException(nameof(challengeRepository));
            AccountService = accountService
                ?? throw new ArgumentNullException(nameof(accountService));
            AuthorizationRepository = authorizationRepository
                ?? throw new ArgumentNullException(nameof(authorizationRepository));
        }

        private IChallengeRepository ChallengeRepository { get; }
        private IAccountService AccountService { get; }
        private IAuthorizationRepository AuthorizationRepository { get; }

        public IChallenge Create(int authzId, string type)
        {
            var challenge = ChallengeRepository.Create();
            challenge.Type = type;
            challenge.AuthorizationId = authzId;
            challenge.Status = ChallengeStatus.Pending;
            var httpToken = new byte[20];
            new System.Security.Cryptography.RNGCryptoServiceProvider().GetBytes(httpToken);
            challenge.Token = Base64Url.Encode(httpToken);

            ChallengeRepository.Add(challenge);

            return challenge;
        }

        public IChallenge[] GetByAuthorization(int id)
        {
            return ChallengeRepository.GetByAuthorization(id) ?? throw new MalformedException("Challenge does not exist");
        }

        public IChallenge GetById(int id)
        {
            return ChallengeRepository.GetById(id) ?? throw new MalformedException("Challenge does not exist");
        }

        public void Validate(IChallenge challenge)
        {
            if (challenge.Status == ChallengeStatus.Pending)
            {
                challenge.Status = ChallengeStatus.Processing;
                ChallengeRepository.Update(challenge);
                
                Task
                    .Run(() =>
                    {
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
                        if (t.IsFaulted)
                        {
                            // TODO Optimize Error assignment
                            Error err = t.Exception.InnerException;
                            challenge.Error = ChallengeRepository.CreateError();
                            challenge.Error.Detail = err.Detail;
                            challenge.Error.Type = err.Type;
                            challenge.Status = ChallengeStatus.Invalid;
                            ChallengeRepository.Update(challenge);
                        }
                        if (t.IsCompleted)
                        {
                            if (challenge.Status == ChallengeStatus.Processing)
                            {
                                challenge.Status = ChallengeStatus.Valid;
                                challenge.Validated = DateTime.UtcNow;
                                ChallengeRepository.Update(challenge);
                            }
                        }
                    });
            }
            else
            {
                throw new MalformedException("Wrong challenge status");
            }

        }

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
#endif
        }


    }
}
