using System;
using PeculiarVentures.ACME.Server.Controllers;
using PeculiarVentures.ACME.Server.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AcmeServerServiceCollectionExtensions
    {
        public static IServiceCollection AddAcmeServerServices(this IServiceCollection collection)
        {
            return collection
                .AddScoped<IAcmeController, AcmeController>()
                .AddScoped<IDirectoryService, DirectoryService>()
                .AddScoped<INonceService, NonceService>()
                .AddScoped<IAccountService, AccountService>();
        }
    }
}
