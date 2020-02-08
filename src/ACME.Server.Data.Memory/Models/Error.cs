using System;
using System.Collections.Generic;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;

namespace PeculiarVentures.ACME.Server.Data.Memory.Models
{
    public class Error : IError
    {
        public ErrorType Type { get; set; }
        public string Detail { get; set; }
        public ICollection<IError> SubProblems { get; set; } = new List<IError>();
    }
}
