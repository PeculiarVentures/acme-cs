using System;
using Microsoft.Extensions.Options;
using PeculiarVentures.ACME.Protocol;

namespace PeculiarVentures.ACME.Server.Services
{
    public class DirectoryService : BaseService, IDirectoryService
    {
        public DirectoryService(
            IOptions<ServerOptions> options)
            : base(options)
        {
        }

        public Directory GetDirectory()
        {
            var directory = OnDirectoryBefore();
            if (Options.ExternalAccountOptions.Type != ExternalAccountType.None) {
                if (directory.Meta == null)
                {
                    directory.Meta = new DirectoryMetadata();
                }
                directory.Meta.ExternalAccountRequired = true;
            }
            return OnDirectoryConvert(directory);
        }

        public virtual Directory OnDirectoryBefore()
        {
            return new Directory();
        }
        public virtual Directory OnDirectoryConvert(Directory directory)
        {
            return directory;
        }
    }
}
