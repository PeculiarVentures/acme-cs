using System;
using PeculiarVentures.ACME.Server.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AcmeServerServiceCollectionExtensions
    {
        public static IServiceCollection AddAcmeServerServices(this IServiceCollection collection)
        {
            return collection
                .AddScoped<INonceService, NonceService>()
                .AddScoped<IDirectoryService, DirectoryService>()
                .AddScoped<IAccountService, AccountService>();
        }
    }
}
