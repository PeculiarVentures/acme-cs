using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Server.Services
{
    public interface INonceService
    {
        AcmeResponse NewNonce();
    }
}