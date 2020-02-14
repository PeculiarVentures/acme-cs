using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using PeculiarVentures.ACME.Server;
using PeculiarVentures.ACME.Server.Data.EF.Core;

namespace AspNetCore.Server
{
    public class Startup
    {
        private const string RootCA = "MIIDiTCCAnGgAwIBAgIQHSvtFOqi9p5IxhblOJjMrDANBgkqhkiG9w0BAQsFADBLMRMwEQYKCZImiZPyLGQBGRYDY29tMRowGAYKCZImiZPyLGQBGRYKYWVnZG9tYWluMjEYMBYGA1UEAxMPQUVHRG9tYWluMiBNU0NBMB4XDTE5MTExMTA5MzYxOFoXDTI0MTExMTA5NDYxN1owSzETMBEGCgmSJomT8ixkARkWA2NvbTEaMBgGCgmSJomT8ixkARkWCmFlZ2RvbWFpbjIxGDAWBgNVBAMTD0FFR0RvbWFpbjIgTVNDQTCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAM0JsffHf3y3L6FP8pV/krfSRlpN1OosmOoO0Zs6/XHe5ZW08S/4On2vwUT/xq3AiRdrJbyeQzc5enqq/7K986UrrXV66BMNeBLp/lUd4SOAv7MUmNnHe5xPstpCHB42hYer317yBG4UyyWE3pK6CiRniijkdyNilcbEUeET7Kze4lwAPSOq7d1do1gP9DbXb79FwY2GsNB8ppSgCt4yYbEoLPIByG3O04/m1r8IkOMjhM3f3aoiYLdfZztqM4JPRG3qkF5B4g65mNGTO+4hZd/Xm/pRIL+ZZz/L8OGiljqiy4xg7Q0pQcYGwUxwR2iN00zGDg3sgEY2K/GmpNWBazcCAwEAAaNpMGcwEwYJKwYBBAGCNxQCBAYeBABDAEEwDgYDVR0PAQH/BAQDAgGGMA8GA1UdEwEB/wQFMAMBAf8wHQYDVR0OBBYEFLKeV/4Ov5GbjEaB2UwfRp66LUvJMBAGCSsGAQQBgjcVAQQDAgEAMA0GCSqGSIb3DQEBCwUAA4IBAQCfasqRTWxq3xDgSyvPxgctyihjF2WCgQ3T45gkdV1rm4AWudmLXauw8KVvJT91r1i2G5pDHIdOpxSBL/FJ4wphe4L5INLkVj9SRaOqHrsFy000+K2ePZtdGZvmaohZjQvKp9NsT3/kkyqvy1aYHVH4EqJoulD06/t/1ITLynu8bvyK08a6JetIK4GTY0LAkr5P/fIop9MMoJJaWMd/c3BJb9K09Uf0izNYf6VEWBSpnkglwGye+p4rwkO7Dw8uSystKy0TQbSSCoiH1K21Z/CjPSjI4iaz8mInEosbCy6CrnNS22BorY2vzR4PXD/FvVcgRJfUgfMNichmvhuM/2Aj";

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

            services.AddAcmeEFCoreRepositories(o => {
                o.UseNpgsql(Configuration.GetValue<string>("ConnectionStrings:DefaultConnection"));
            });
            services.AddAcmeServerServices(o => {
                o.EnrollmentHandler = new CertificateEnrollmentHandler();
                o.DownloadCertificateFormat = DownloadCertificateFormat.PemCertificateChain;
                o.ExtraCertificateStorage = new X509Certificate2Collection(new X509Certificate2(Convert.FromBase64String(RootCA)));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
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
