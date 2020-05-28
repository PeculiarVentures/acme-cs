using System;
using Microsoft.Extensions.Options;
using PeculiarVentures.ACME.Protocol;

namespace PeculiarVentures.ACME.Server.Services
{
    /// <summary>
    /// Directory service
    /// </summary>
    public class DirectoryService : BaseService, IDirectoryService
    {
        public DirectoryService(
            IOptions<ServerOptions> options)
            : base(options)
        {
        }

        /// <inheritdoc/>
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

        /// <summary>
        /// Creates new JSON <see cref="Directory"/>
        /// </summary>
        protected virtual Directory OnDirectoryBefore()
        {
            return new Directory();
        }

        /// <summary>
        /// Assign URLs to JSON <see cref="Directory"/>.
        /// For expended objects need add assign values
        /// </summary>
        /// <param name="directory">JSON <see cref="Directory"/></param>
        protected virtual Directory OnDirectoryConvert(Directory directory)
        {
            var baseUri = new Uri(Options.BaseAddress);
            directory.NewNonce = directory.NewNonce ?? new Uri(baseUri, "new-nonce").ToString();
            directory.NewAccount = directory.NewAccount ?? new Uri(baseUri, "new-acct").ToString();
            directory.NewOrder = directory.NewOrder ?? new Uri(baseUri, "new-order").ToString();
            directory.KeyChange= directory.KeyChange ?? new Uri(baseUri, "key-change").ToString();
            directory.RevokeCertificate = directory.RevokeCertificate ?? new Uri(baseUri, "revoke").ToString();

            return directory;
        }
    }
}
