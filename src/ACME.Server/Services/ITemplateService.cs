using PeculiarVentures.ACME.Protocol;

namespace PeculiarVentures.ACME.Server.Services
{
    public interface ITemplateService
    {
        TemplateCollection GetTemplates(int accountId);
        Template GetById(int accountId, string gsTemplate);
    }
}