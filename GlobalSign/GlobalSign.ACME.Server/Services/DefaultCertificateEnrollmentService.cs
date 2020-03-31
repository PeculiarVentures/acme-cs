using GlobalSign.ACME.Server.Data.EF.Core.Models;
using PeculiarVentures.ACME;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;

namespace GlobalSign.ACME.Server.Services
{
    public class DefaultGsCertificateEnrollmentService : PeculiarVentures.ACME.Server.Services.DefaultCertificateEnrollmentService
    {
        public IExchangeItem GetExchangeItem(IAccount account)
        {
            throw new MalformedException("Method not implemented");
        }
    }
}
