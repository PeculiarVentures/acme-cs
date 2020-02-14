using System;
using Microsoft.Extensions.Options;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Server.Services
{
    public class DirectoryService : IDirectoryService
    {
        public DirectoryService(IOptions<ServerOptions> options)
        {
            Options = options?.Value
                ?? throw new ArgumentNullException(nameof(options));
        }

        public ServerOptions Options { get; }

        public Directory GetDirectory()
        {
            var directory = new Directory();
            if (Options.ExternalAccountOptions.Type != ExternalAccountType.None) {
                if (directory.Meta == null)
                {
                    directory.Meta = new DirectoryMetadata();
                }
                directory.Meta.ExternalAccountRequired = true;
            }
            return directory;
        }
    }
}
