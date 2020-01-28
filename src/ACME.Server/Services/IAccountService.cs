using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Server.Services
{
    public interface IAccountService
    {
        AcmeResponse ChangeKey(AcmeRequest request);
        AcmeResponse Create(AcmeRequest request);
        AcmeResponse Update(AcmeRequest request);
    }
}