using System;
using System.Threading.Tasks;
using PeculiarVentures.ACME.Helpers;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;

namespace PeculiarVentures.ACME.Server.Services
{
    public class ChallengeService : IChallengeService
    {
        public ChallengeService(IChallengeRepository challengeRepository)
        {
            ChallengeRepository = challengeRepository ?? throw new ArgumentNullException(nameof(challengeRepository));
        }

        private IChallengeRepository ChallengeRepository { get; }

        public IChallenge Create(int accountId, string type)
        {
            var challenge = ChallengeRepository.Create();
            challenge.Type = type;
            challenge.AccountId = accountId;
            challenge.Status = ChallengeStatus.Pending;
            var httpToken = new byte[20];
            new System.Security.Cryptography.RNGCryptoServiceProvider().GetBytes(httpToken);
            challenge.Token = Base64Url.Encode(httpToken);

            ChallengeRepository.Add(challenge);

            return challenge;
        }

        public IChallenge GetById(int id)
        {
            return ChallengeRepository.GetById(id);
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
                        // TODO Implement challemge validation
                        //      Otherwise all validations will be valid
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


    }
}
