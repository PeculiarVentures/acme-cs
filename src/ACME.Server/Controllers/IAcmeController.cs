using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Server.Controllers
{
    public interface IAcmeController
    {
        AcmeResponse CreateAccount(AcmeRequest request);
        AcmeResponse CreateOrder(AcmeRequest request);
        AcmeResponse GetDirectory();
        AcmeResponse GetNonce(AcmeRequest request);
        AcmeResponse PostAccount(AcmeRequest request);
        AcmeResponse PostOrder(AcmeRequest request);
    }
}