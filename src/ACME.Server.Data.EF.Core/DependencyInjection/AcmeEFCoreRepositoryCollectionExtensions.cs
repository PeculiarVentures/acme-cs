using System;
using Microsoft.EntityFrameworkCore;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;
using PeculiarVentures.ACME.Server.Data.EF.Core;
using PeculiarVentures.ACME.Server.Data.EF.Core.Repositories;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AcmeEFCoreRepositoryCollectionExtensions
    {
        public static IServiceCollection AddAcmeEFCoreRepositories(this IServiceCollection collection, Action<DbContextOptionsBuilder> options)
        {
            return collection
                .AddDbContext<AcmeContext>(options, ServiceLifetime.Transient, ServiceLifetime.Transient)
                // repositories
                .AddTransient<IAuthorizationRepository, AuthorizationRepository>()
                .AddSingleton<INonceRepository, NonceRepository>() // Memory
                .AddTransient<IAccountRepository, AccountRepository>()
                .AddTransient<IOrderRepository, OrderRepository>()
                .AddTransient<IOrderAuthorizationRepository, OrderAuthorizationRepository>()
                .AddTransient<IChallengeRepository, ChallengeRepository>()
                .AddTransient<IExternalAccountRepository, DefaultExternalAccountRepository>();
        }
    }
}
