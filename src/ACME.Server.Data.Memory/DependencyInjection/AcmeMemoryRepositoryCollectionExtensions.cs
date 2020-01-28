using System;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;
using PeculiarVenturs.ACME.Server.Data.Memory.Repositories;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AcmeMemoryRepositoryCollectionExtensions
    {
        public static IServiceCollection AddAcmeMemoryRepositories(this IServiceCollection collection)
        {
            return collection
                .AddSingleton<INonceRepository, NonceRepository>()
                .AddSingleton<IAccountRepository, AccountRepository>();
        }
    }
}
