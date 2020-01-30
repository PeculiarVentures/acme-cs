using System;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Server.Services
{
    public class DirectoryService : IDirectoryService
    {
        public Directory GetDirectory()
        {
            return new Directory();
        }
    }
}
