using System;
using System.Collections.Generic;
using System.Linq;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server.Services;

namespace AspNetCore.Server.Services
{
    public class TemplateService : ITemplateService
    {
        public List<Template> Templates = new List<Template>() {
            new Template {
                Identifier = "1.3.6.1.4.1.311.21.8.4659868.14977541.11682196.5068793.8777523.134.13916487.4686474",
                Name = "Test",
                Profile = new TemplateProfile()
                {
                    ValidityPeriod = 1000,
                    EKUs = new string[] { },
                    Identifiers = new List<TemplateIdentifierType>(),
                },
                Requirements = new TemplateRequirements
                {
                    Archival = new TemplateRequirementArchival
                    {
                        Algorithm = PeculiarVentures.ACME.Web.AlgorithmsEnum.A128GCM,
                    }
                }
            }
        };

        public TemplateService(IAccountService accountService)
        {
            AccountService = accountService
                ?? throw new ArgumentNullException(nameof(accountService));
        }

        public IAccountService AccountService { get; }

        public Template GetById(int accountId, string templateId)
        {
            return Templates.FirstOrDefault(o => o.Identifier == templateId);
        }

        public TemplateCollection GetTemplates(int accountId)
        {
            return new TemplateCollection()
            {
                Templates = Templates,
            };
        }
    }
}
