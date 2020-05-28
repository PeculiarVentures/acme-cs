using System;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;

namespace PeculiarVentures.ACME.Server.Services
{
    public interface IChallengeService
    {
        /// <summary>
        /// Returns <see cref="IChallenge"/> by identifier
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <exception cref="MalformedException"/>
        IChallenge GetById(int id);

        /// <summary>
        /// Validates a challenge
        /// </summary>
        /// <param name="challenge"><see cref="IChallenge"/></param>
        /// <exception cref="MalformedException"/>
        /// <exception cref="Exception"/>
        void Validate(IChallenge challenge);

        /// <summary>
        /// Creates new <see cref="IChallenge"/>
        /// </summary>
        /// <param name="authzId">The identifier of <see cref="IAuthorization"/></param>
        /// <param name="type">THe type of <see cref="IChallenge"/></param>
        IChallenge Create(int authzId, string type);

        /// <summary>
        /// Returns array of <see cref="IChallenge"/>
        /// </summary>
        /// <param name="id">The identifier of <see cref="IAuthorization"/></param>
        /// <exception cref="MalformedException"/>
        IChallenge[] GetByAuthorization(int id);
    }
}
