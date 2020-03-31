using GlobalSign.ACME.Protocol;

namespace GlobalSign.ACME.Server.Services
{
    public interface ITemplateService
    {
        TemplateCollection GetTemplates(int accountId);
        Template GetById(int accountId, string gsTemplate);
    }
}