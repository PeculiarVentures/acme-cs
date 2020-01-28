using System;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Server.Services
{
    public class NonceService : AcmeService, INonceService
    {
        public NonceService(
            INonceRepository nonceRepository,
            IAccountRepository accountRepository)
            : base(nonceRepository, accountRepository)
        {
        }

        public AcmeResponse NewNonce()
        {
            return WrapAction((response) =>
            {
                response.ReplayNonce = NonceRepository.Create();
            });
        }
    }
}
