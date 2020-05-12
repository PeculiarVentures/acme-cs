using System;
using Microsoft.Extensions.DependencyInjection;

namespace PeculiarVentures.ACME.Server
{
    public static class Application
    {
        public const string BaseAddress = "https://test.server.com/acme/";

        public static ServiceProvider CreateProvider(Action<ServerOptions> action = null)
        {
            return new ServiceCollection()
                .AddAcmeMemoryRepositories()
                .AddAcmeServerServices(action ?? (o => {
                    o.BaseAddress = BaseAddress;
                }))
                .BuildServiceProvider();
        }
    }
}
