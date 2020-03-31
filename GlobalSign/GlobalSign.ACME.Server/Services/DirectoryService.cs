using System;
using Microsoft.Extensions.Options;
using PeculiarVentures.ACME.Server.Services;
using PeculiarVentures.ACME.Server;
using PeculiarVentures.ACME.Protocol;

namespace GlobalSign.ACME.Server.Services
{
    public class GsDirectoryService : DirectoryService
    {
        public GsDirectoryService(
            ITemplateService templateService,
            IOptions<ServerOptions> options)
            : base(options)
        {
            TemplateService = templateService
                ?? throw new ArgumentNullException(nameof(templateService));
        }

        public ITemplateService TemplateService { get; }

        public override Directory OnDirectoryBefore()
        {
            return new Protocol.Directory();
        }
        public override Directory OnDirectoryConvert(Directory directory)
        {
            var gsDirectory = (Protocol.Directory)directory;

            if (!(TemplateService is DefaultTemplateService))
            {
                gsDirectory.GsGetTemplates = new Uri(new Uri(Options.BaseAddress), "templates").ToString();
                gsDirectory.GsGetExchange = new Uri(new Uri(Options.BaseAddress), "exchange").ToString();
            }
            return gsDirectory;
        }
    }
}
