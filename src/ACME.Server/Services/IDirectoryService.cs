using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Server.Services
{
    public interface IDirectoryService
    {
        AcmeResponse GetDirectory();
    }
}