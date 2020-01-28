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
        bool Contains(string value);
        void Remove(string value);
    }
}
