using System;
namespace PeculiarVentures.ACME.Server.Data.Abstractions.Models
{
    public interface IIdentifier
    {
        /// <summary>
        /// The type of identifier.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The identifier itself.
        /// </summary>
        public string Value { get; set; }
    }
}
