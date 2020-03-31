using Microsoft.Extensions.Options;
using PeculiarVentures.ACME.Protocol;

namespace PeculiarVentures.ACME.Server.Services
{
    public interface IDirectoryService
    {
        Directory GetDirectory();
    }
}