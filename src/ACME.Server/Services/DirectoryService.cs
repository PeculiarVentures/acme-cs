using System;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Server.Services
{
    public class DirectoryService : IDirectoryService
    {
        public AcmeResponse GetDirectory()
        {
            return new AcmeResponse
            {
                StatusCode = 200,
                Content = new Directory(),
            };
        }
    }
}
