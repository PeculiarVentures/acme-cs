using System.Collections.Generic;
using PeculiarVentures.ACME.Protocol;

namespace PeculiarVentures.ACME.Server.Data.Abstractions.Models
{
    public interface IError
    {
        /// <summary>
        /// The type of this error
        /// </summary>
        ErrorType Type { get; set; }

        /// <summary>
        /// Detail of this error
        /// </summary>
        string Detail { get; set; }

        /// <summary>
        /// Array of errors
        /// </summary>
        ICollection<IError> SubProblems { get; set; }
    }
}
