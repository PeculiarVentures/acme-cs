using System;
namespace PeculiarVentures.ACME.Server.Data.Abstractions.Models
{
    public interface IAccountId
    {
        /// <summary>
        /// The identifier of account
        /// </summary>
        int AccountId { get; set; }
    }
}
