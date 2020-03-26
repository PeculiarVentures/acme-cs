using System;
using Microsoft.Extensions.Options;
using PeculiarVentures.ACME.Server;
using PeculiarVentures.ACME.Server.Controllers;
using PeculiarVentures.ACME.Server.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AcmeServerServiceCollectionExtensions
    {
        public static IServiceCollection AddAcmeServerServices(this IServiceCollection collection, Action<ServerOptions> options)
        {
            return collection
                .Configure(options)
                .AddTransient<IConverterService, ConverterService>()
                .AddTransient<IAcmeController, AcmeController>()
                .AddTransient<IDirectoryService, DirectoryService>()
                .AddTransient<INonceService, NonceService>()
                .AddTransient<IOrderService, OrderService>()
                .AddTransient<IAuthorizationService, AuthorizationService>()
                .AddTransient<IChallengeService, ChallengeService>()
                .AddTransient<IAccountService, AccountService>()
                .AddTransient<IExternalAccountService, ExternalAccountService>()
                .AddTransient<ICertificateEnrollmentService, DefaultCertificateEnrollmentService>()
                .AddTransient<ITemplateService, DefaultTemplateService>();

        }
    }
}
