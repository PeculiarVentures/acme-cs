using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;

namespace PeculiarVentures.ACME.Server.Data.EF.Core.Models
{
    public class Challenge : BaseObject, IChallenge
    {
        [Required]
        public string Type { get; set; }

        [Required]
        public ChallengeStatus Status { get; set; } = ChallengeStatus.Processing;

        public DateTime? Validated { get; set; }

        public int? ErrorId { get; set; }

        [ForeignKey(nameof(ErrorId))]
        public virtual Error ErrorValue { get; set; }

        [NotMapped]
        public virtual IError Error
        {
            get => ErrorValue;
            set => ErrorValue = (Error)value;
        }

        [Required]
        public string Token { get; set; }

        public int AuthorizationId { get; set; }

        [ForeignKey(nameof(AuthorizationId))]
        public Authorization Authorization { get; set; }
    }
}
