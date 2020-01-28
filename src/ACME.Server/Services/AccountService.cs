using System;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Protocol.Messages;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Server.Services
{
    public class AccountService : AcmeService, IAccountService
    {
        public AccountService(
            INonceRepository nonceRepository,
            IAccountRepository accountRepository) : base(nonceRepository, accountRepository)
        {
        }

        public AcmeResponse Create(AcmeRequest request)
        {
            return WrapAction(response =>
            {
                var @params = request.GetContent<NewAccount>();
                IAccount account;

                if (@params.OnlyReturnExisting == true)
                {
                    account = AccountRepository.GetByPublicKey(request.PublicKey)
                        ?? throw new AccountDoesNotExistException();
                    response.Content = AccountRepository.Convert(account);
                    response.StatusCode = 200; // Ok
                }
                else
                {
                    // Create new account
                    var header = request.Token.GetProtected();
                    account = AccountRepository.Create(header.Key, @params);
                    AccountRepository.Add(account);

                    response.Content = AccountRepository.Convert(account);
                    response.StatusCode = 201; // Created
                }

                response.Location = $"acct/{account.Id}";
            });
        }

        public AcmeResponse Update(AcmeRequest request)
        {
            return WrapAction(response =>
            {
                var @params = request.GetContent<UpdateAccount>();

                var account = GetAccount(request.KeyId);
                AssertAccountStatus(account);

                if (@params.Status != null)
                {
                    // Deactivate
                    if (@params.Status != AccountStatus.Deactivated)
                    {
                        throw new MalformedException("Request paramter status must be 'deactivated'");
                    }

                    account.Status = AccountStatus.Deactivated;
                }
                else
                {
                    // Update
                    account.Contacts = @params.Contacts;
                }
                AccountRepository.Update(account);

                response.Content = AccountRepository.Convert(account);
            });
        }


        public AcmeResponse ChangeKey(AcmeRequest request)
        {
            return WrapAction(response =>
            {
                throw new NotImplementedException();
            });
        }
    }
}
