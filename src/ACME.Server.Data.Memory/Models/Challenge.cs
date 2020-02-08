using System;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;

namespace PeculiarVentures.ACME.Server.Data.Memory.Models
{
    public class Challenge : IChallenge
    {
        public string Type { get; set; }
        public ChallengeStatus Status { get; set; }
        public DateTime? Validated { get; set; }
        public IError Error { get; set; }
        public int Id { get; set; }
        public string Token { get; set; }
        public int AccountId { get; set; }
    }
}
