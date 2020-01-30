using System;
using PeculiarVentures.ACME.Protocol;

namespace PeculiarVentures.ACME.Server.Data.Abstractions.Models
{
    public interface IChallenge : IBaseObject
    {
        /// <summary>
        /// The type of challenge encoded in the object.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The status of this challenge.
        /// </summary>
        /// status (required, string)
        public ChallengeStatus Status { get; set; }

        /// <summary>
        /// The time at which the server validated this challenge.
        /// </summary>
        public DateTime? Validated { get; set; }

        /// <summary>
        /// Error that occurred while the server was validating the challenge.
        /// </summary>
        public IError Error { get; set; }
    }
}
