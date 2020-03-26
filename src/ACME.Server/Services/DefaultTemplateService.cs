using System;
using PeculiarVentures.ACME.Protocol;

namespace PeculiarVentures.ACME.Server.Services
{
    public sealed class DefaultTemplateService : ITemplateService
    {
        public Template GetById(int accountId, string gsTemplate)
        {
            throw new MalformedException("Method not supported");
        }

        public TemplateCollection GetTemplates(int accountId)
        {
            throw new MalformedException("Method not supported");
        }
    }
}
