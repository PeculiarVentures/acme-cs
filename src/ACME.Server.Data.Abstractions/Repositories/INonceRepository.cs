using System;
namespace PeculiarVentures.ACME.Server.Data.Abstractions.Repositories
{
    public interface INonceRepository
    {
        /// <summary>
        /// Creates random bytes
        /// </summary>
        /// <returns>Byte array</returns>
        string Create();

        /// <summary>
        /// Checks the nonce for existence
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        bool Contains(string value);

        /// <summary>
        /// Removes nonce
        /// </summary>
        /// <param name="value"></param>
        void Remove(string value);
    }
}
