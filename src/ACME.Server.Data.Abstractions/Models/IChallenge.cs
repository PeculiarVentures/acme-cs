using System;
using PeculiarVentures.ACME.Protocol;

namespace PeculiarVentures.ACME.Server.Data.Abstractions.Models
{
    public interface IChallenge : IBaseObject
    {
        /// <summary>
        /// The type of challenge encoded in the object.
        /// </summary>
        string Type { get; set; }

        /// <summary>
        /// The status of this challenge.
        /// </summary>
        /// status (required, string)
        ChallengeStatus Status { get; set; }

        /// <summary>
        /// The time at which the server validated this challenge.
        /// </summary>
        DateTime? Validated { get; set; }

        /// <summary>
        /// Error that occurred while the server was validating the challenge.
        /// </summary>
        IError Error { get; set; }
        string Token { get; set; }        
        int AuthorizationId { get; set; }
    }
}
