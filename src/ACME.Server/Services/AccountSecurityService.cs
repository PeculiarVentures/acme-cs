using Microsoft.Extensions.Options;

namespace PeculiarVentures.ACME.Server.Services
{
    public class AccountSecurityService : BaseService, IAccountSecurityService
    {
        public AccountSecurityService(IOptions<ServerOptions> options) : base(options)
        {
        }

        public virtual void CheckAccess(AccountAccess data)
        {
            #region Check arguments
            if (data is null)
            {
                throw new MalformedException("Argument data is null");
            }
            if (data.Account is null)
            {
                throw new MalformedException("Argument data.Account is null");
            }
            if (data.Target is null)
            {
                throw new MalformedException("Argument data.Target is null");
            }
            #endregion

            if (data.Target.AccountId != data.Account.Id)
            {
                throw new MalformedException("Access denied");
            }
        }
    }
}
