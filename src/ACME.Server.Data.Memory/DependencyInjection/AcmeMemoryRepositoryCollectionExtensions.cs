using System;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;
using PeculiarVentures.ACME.Server.Data.Memory.Repositories;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AcmeMemoryRepositoryCollectionExtensions
    {
        public static IServiceCollection AddAcmeMemoryRepositories(this IServiceCollection collection)
        {
            return collection
                .AddSingleton<INonceRepository, NonceRepository>()
                .AddSingleton<IAccountRepository, AccountRepository>()
                .AddSingleton<IExternalAccountRepository, ExternalAccountRepository>()
                .AddSingleton<IAuthorizationRepository, AuthorizationRepository>()
                .AddSingleton<IOrderRepository, OrderRepository>()
                .AddSingleton<IOrderAuthorizationRepository, OrderAuthorizationRepository>()
                .AddSingleton<IChallengeRepository, ChallengeRepository>();
        }
    }
}
