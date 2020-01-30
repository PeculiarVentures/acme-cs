using System;
using System.Text.RegularExpressions;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Protocol.Messages;
using PeculiarVentures.ACME.Server.Services;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Server.Controllers
{
    public class AcmeController : IAcmeController
    {
        public AcmeController(
            IDirectoryService directoryService,
            INonceService nonceService,
            IAccountService accountService)
        {
            DirectoryService = directoryService ?? throw new ArgumentNullException(nameof(directoryService));
            NonceService = nonceService ?? throw new ArgumentNullException(nameof(nonceService));
            AccountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        }

        public IDirectoryService DirectoryService { get; }
        public INonceService NonceService { get; }
        public IAccountService AccountService { get; }

        public AcmeResponse CreateResponse()
        {
            return new AcmeResponse
            {
                StatusCode = 200, // OK
                ReplayNonce = NonceService.Create(),
            };
        }

        public AcmeResponse WrapAction(Action<AcmeResponse> action)
        {
            var response = CreateResponse();
            try
            {
                action.Invoke(response);
            }
            catch (Exception e)
            {
                response.StatusCode = 500;
                Error error = e;
                response.Content = error;
            }
            return response;
        }

        public AcmeResponse GetDirectory()
        {
            return WrapAction((response) =>
            {
                response.Content = DirectoryService.GetDirectory();
                response.StatusCode = 200; // Ok
            });
        }

        public AcmeResponse GetNonce(AcmeRequest request)
        {
            return WrapAction((response) =>
            {
                response.ReplayNonce = NonceService.Create();
                if (request.Method == null
                    || !request.Method.Equals("head", StringComparison.CurrentCultureIgnoreCase))
                {
                    response.StatusCode = 204; // No content
                }
            });
        }

        #region Account management

        /// <summary>
        /// Gets account by kid
        /// </summary>
        /// <param name="kid">Key identifier link</param>
        /// <returns>Account</returns>
        /// <exception cref="MalformedException"/>
        /// <exception cref="AccountDoesNotExistException"/>
        protected Account GetAccount(string kid)
        {
            // Get Id from http link
            var regEx = new Regex("\\/(\\d+)");
            var match = regEx.Match(kid);
            if (!match.Success)
            {
                throw new MalformedException($"Cannot get Key Id from link {kid}");
            }

            var account = AccountService.GetById(int.Parse(match.Groups[1].Value));
            if (account == null)
            {
                throw new AccountDoesNotExistException();
            }
            return account;
        }

        public AcmeResponse CreateAccount(AcmeRequest request)
        {
            return WrapAction(response =>
            {
                var @params = request.GetContent<NewAccount>();

                Account account = AccountService.GetByPublicKey(request.PublicKey);

                if (@params.OnlyReturnExisting == true)
                {
                    response.Content = account
                        ?? throw new AccountDoesNotExistException();
                    response.StatusCode = 200; // Ok
                }
                else
                {
                    if (account == null)
                    {
                        // Create new account
                        response.Content = account = AccountService.Create(request.PublicKey, @params);
                        response.StatusCode = 201; // Created
                    }
                    else
                    {
                        // Existing account
                        response.Content = account;
                        response.StatusCode = 200; // Ok
                    }
                }

                // Set headers
                response.Location = $"acct/{account.Id}";
            });
        }

        public AcmeResponse PostAccount(AcmeRequest request)
        {
            return WrapAction((response) =>
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

                    response.Content = AccountService.Deactivate(account.Id);
                }
                else if (@params.Contacts != null)
                {
                    // Update
                    response.Content = AccountService.Update(account.Id, @params.Contacts);
                }
                else
                {
                    response.Content = account;
                }
            });
        }

        public void AssertAccountStatus(Account account)
        {
            // If a server receives a POST or POST-as-GET from a
            // deactivated account, it MUST return an error response with status
            // code 401(Unauthorized) and type "urn:ietf:params:acme:error:unauthorized"
            if (account.Status == AccountStatus.Deactivated)
            {
                throw new UnauthorizedException("Account deactivated");
            }
        }

        #endregion

        #region Order management

        public AcmeResponse CreateOrder(AcmeRequest request)
        {
            return WrapAction((response) =>
            {
                throw new NotImplementedException();
            });
        }

        public AcmeResponse PostOrder(AcmeRequest request)
        {
            return WrapAction((response) =>
            {
                throw new NotImplementedException();
            });
        }

        #endregion
    }
}
