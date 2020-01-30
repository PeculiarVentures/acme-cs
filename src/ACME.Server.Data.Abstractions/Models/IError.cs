using System;
using System.Collections.Generic;
using PeculiarVentures.ACME.Protocol;

namespace PeculiarVentures.ACME.Server.Data.Abstractions.Models
{
    public interface IError
    {
        ErrorType Type { get; set; }

        string Detail { get; set; }

        ICollection<Error> SubPropems { get; set; }
    }
}
