using System;
using GlobalSign.ACME.Server.Data.EF.Core;
using GlobalSign.ACME.Server.Data.EF.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;
using PeculiarVentures.ACME.Server.Data.EF.Core.Repositories;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AcmeEFCoreRepositoryCollectionExtensions
    {
        public static IServiceCollection AddGsAcmeEFCoreRepositories(this IServiceCollection collection, Action<DbContextOptionsBuilder> options)
        {
            return collection.AddAcmeEFCoreRepositories(options)
                .AddDbContext<GsAcmeContext>(options, ServiceLifetime.Transient, ServiceLifetime.Transient)
                // repositories
                .AddTransient<IAuthorizationRepository, AuthorizationRepository>()
                .AddSingleton<INonceRepository, NonceRepository>() // Memory
                .AddTransient<IAccountRepository, AccountRepository>()
                .AddTransient<IOrderRepository, GsOrderRepository>()
                .AddTransient<IOrderAuthorizationRepository, OrderAuthorizationRepository>()
                .AddTransient<IChallengeRepository, ChallengeRepository>()
                .AddTransient<IExternalAccountRepository, DefaultExternalAccountRepository>();
        }
    }
}
