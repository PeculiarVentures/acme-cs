using System;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Protocol.Messages;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Server.Services
{
    public class AccountService : AcmeService
    {
        public AccountService(
            INonceRepository nonceRepository,
            IAccountRepository accountRepository) : base(nonceRepository, accountRepository)
        {
        }

        public AcmeResponse NewAccount(AcmeRequest request)
        {
            return WrapAction(response =>
            {
                var @params = request.GetPayload<NewAccount>();

                if (@params.OnlyReturnExisting == true)
                {
                    var account = AccountRepository.GetByPublicKey(request.PublicKey);
                    response.Content = account ?? throw new AccountDoesNotExistException();
                    response.StatusCode = 200; // Ok
                }
                else
                {
                    // Create new account
                    var header = request.GetProtected();
                    var account = AccountRepository.Create(header.Key, @params);
                    AccountRepository.Add(account);

                    response.Content = account;
                    response.StatusCode = 201; // Created
                }
            });
        }

        public AcmeResponse Update(AcmeRequest request)
        {
            return WrapAction(response =>
            {
                var @params = request.GetPayload<UpdateAccount>();

                response.Content = GetAccount(request.KeyId);
            });
        }
    }
}
