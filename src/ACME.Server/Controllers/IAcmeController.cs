using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Server.Controllers
{
    public interface IAcmeController
    {
        ServerOptions Options { get; }
        AcmeResponse CreateAccount(AcmeRequest request);
        AcmeResponse CreateOrder(AcmeRequest request);
        AcmeResponse GetDirectory();
        AcmeResponse GetNonce(AcmeRequest request);
        AcmeResponse PostAccount(AcmeRequest request);
        AcmeResponse PostOrder(AcmeRequest request, int orderId);
        AcmeResponse PostAuthorization(AcmeRequest acmeRequest, int authzId);
        AcmeResponse PostChallenge(AcmeRequest request, int challengeId);
        AcmeResponse FinalizeOrder(AcmeRequest request, int orderId);
        AcmeResponse GetCertificate(AcmeRequest request, string id);
        AcmeResponse RevokeCertificate(AcmeRequest request);
    }
}