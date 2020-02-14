using System;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;

namespace PeculiarVentures.ACME.Server.Services
{
    public interface IChallengeService
    {
        IChallenge GetById(int id);
        void Validate(IChallenge challenge);
        IChallenge Create(int authzId, string type);
        IChallenge[] GetByAuthorization(int id);
    }
}
