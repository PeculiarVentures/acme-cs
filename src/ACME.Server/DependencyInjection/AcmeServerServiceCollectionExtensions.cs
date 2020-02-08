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
                .AddScoped<IConverterService, ConverterService>()
                .AddScoped<IAcmeController, AcmeController>()
                .AddScoped<IDirectoryService, DirectoryService>()
                .AddScoped<INonceService, NonceService>()
                .AddScoped<IOrderService, OrderService>()
                .AddScoped<IAuthorizationService, AuthorizationService>()
                .AddScoped<IChallengeService, ChallengeService>()
                .AddScoped<IAccountService, AccountService>();

        }
    }
}
