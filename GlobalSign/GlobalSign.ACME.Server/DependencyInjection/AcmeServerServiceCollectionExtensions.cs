using System;
using GlobalSign.ACME.Server.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PeculiarVentures.ACME.Server;
using PeculiarVentures.ACME.Server.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AcmeServerServiceCollectionExtensions
    {
        public static IServiceCollection AddGsAcmeServerServices(this IServiceCollection collection, Action<ServerOptions> options)
        {
            return collection.AddAcmeServerServices(options)
                .Replace(ServiceDescriptor.Transient<IOrderService, GsOrderService>())
                .Replace(ServiceDescriptor.Transient<IConverterService, GsConverterService>())
                .Replace(ServiceDescriptor.Transient<IDirectoryService, GsDirectoryService>())
                .Replace(ServiceDescriptor.Transient<ICertificateEnrollmentService, DefaultGsCertificateEnrollmentService>())
                .AddTransient<ITemplateService, DefaultTemplateService>();
        }
    }
}
