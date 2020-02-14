using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;

namespace PeculiarVentures.ACME.Server.Data.EF.Core.Models
{
    public class Error : BaseObject, IError
    {
        [Required]
        public ErrorType Type { get; set; }

        [Required]
        public string Detail { get; set; }

        public ICollection<Error> SubProblemsValue { get; set; } = new Collection<Error>();

        [NotMapped]
        public ICollection<IError> SubProblems { get; set; } = new Collection<IError>();
    }
}
