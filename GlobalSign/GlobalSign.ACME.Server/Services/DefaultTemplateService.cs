using System;
using GlobalSign.ACME.Protocol;
using PeculiarVentures.ACME;

namespace GlobalSign.ACME.Server.Services
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
