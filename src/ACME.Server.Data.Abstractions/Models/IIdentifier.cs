using System;
namespace PeculiarVentures.ACME.Server.Data.Abstractions.Models
{
    public interface IIdentifier
    {
        /// <summary>
        /// The type of identifier.
        /// </summary>
        string Type { get; set; }

        /// <summary>
        /// The identifier itself.
        /// </summary>
        string Value { get; set; }
    }
}
