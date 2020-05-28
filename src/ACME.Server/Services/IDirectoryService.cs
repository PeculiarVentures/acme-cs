using Microsoft.Extensions.Options;
using PeculiarVentures.ACME.Protocol;

namespace PeculiarVentures.ACME.Server.Services
{
    public interface IDirectoryService
    {
        /// <summary>
        /// Returns <see cref="Directory"/>
        /// </summary>
        Directory GetDirectory();
    }
}