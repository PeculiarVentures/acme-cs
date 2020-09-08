using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using AspNetCore.Server.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server;
using PeculiarVentures.ACME.Server.Services;

namespace AspNetCore.Server
{
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .AddNewtonsoftJson(o =>
                {
                    o.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    o.SerializerSettings.Formatting = Formatting.Indented;
                });

            var dn = "CN=Test Memory Root CA";
            var key = RSA.Create(2048);
            var rootCert = CertificateEnrollmentService.GenerateCertificate(dn, dn, key, key);
            rootCert = rootCert.CopyWithPrivateKey(key);

            var dn2 = "CN=Test CA";
            var key2 = RSA.Create(2048);
            var caCert = CertificateEnrollmentService.GenerateCertificate(dn, dn2, key, key2);
            caCert = caCert.CopyWithPrivateKey(key2);

            services.AddAcmeMemoryRepositories();
            services.AddAcmeServerServices(o =>
            {
                o.OrdersPageSize = 1;
                o.BaseAddress = "https://localhost:5003/";
                o.DownloadCertificateFormat = DownloadCertificateFormat.PemCertificateChain;
                o.ExtraCertificateStorage = new X509Certificate2Collection(new X509Certificate2[] { rootCert, caCert });
                o.ExternalAccountOptions = new ExternalAccountOptions
                {
                    Type = ExternalAccountType.None,
                };
            });
            services.Replace(ServiceDescriptor.Transient<ICertificateEnrollmentService, CertificateEnrollmentService>());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                logger.LogInformation("In Development environment");
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
