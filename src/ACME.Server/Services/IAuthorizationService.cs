using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;

namespace PeculiarVentures.ACME.Server.Services
{
    public interface IAuthorizationService
    {
        IAuthorization GetById(int accountId, int authzId);
        IAuthorization GetActual(int accountId, Identifier identifier);
        IAuthorization Create(int accountId, Identifier identifier);
        IAuthorization RefreshStatus(IAuthorization item);
    }
}