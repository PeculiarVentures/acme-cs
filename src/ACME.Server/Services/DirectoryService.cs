using System;
using Microsoft.Extensions.Options;
using PeculiarVentures.ACME.Protocol;

namespace PeculiarVentures.ACME.Server.Services
{
    public class DirectoryService : BaseService, IDirectoryService
    {
        public DirectoryService(
            ITemplateService templateService,
            IOptions<ServerOptions> options)
            : base(options)
        {
            TemplateService = templateService
                ?? throw new ArgumentNullException(nameof(templateService));
        }

        public ITemplateService TemplateService { get; }

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
            if (!(TemplateService is DefaultTemplateService))
            {
                directory.GsGetTemplates = new Uri(new Uri(Options.BaseAddress), "templates").ToString();
                directory.GsGetExchange = new Uri(new Uri(Options.BaseAddress), "exchange").ToString();
            }
            return directory;
        }
    }
}
