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

        public AcmeResponse WrapAction(Action<AcmeResponse> action, AcmeRequest request = null)
        {
            var response = CreateResponse();

            // check nonce
            if (request != null)
            {
                if (request.Method == "POST")
                {
                    var nonce = request.Token.GetProtected().Nonce ?? throw new BadNonceException();
                    NonceService.Validate(nonce);
                }
            }

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
            }, request);
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
            }, request);
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

        public AcmeResponse ChangeKey(AcmeRequest request)
        {
            return WrapAction((response) =>
            {
                var reqProtected = request.Token.GetProtected();

                // TODO need check this checking
                // Validate the POST request belongs to a currently active account, as described in Section 6.
                var acc = GetAccount(reqProtected.KeyID);
                var js = new JsonWebSignature();
                if (!js.Verify(acc.Key.GetPublicKey()))
                {
                    throw new MalformedException();
                }

                // Check that the payload of the JWS is a well - formed JWS object(the "inner JWS").
                var innerJWS = request.Token.GetPayload<JsonWebSignature>();
                var innerProtected = innerJWS.GetProtected();

                // Check that the JWS protected header of the inner JWS has a "jwk" field.
                var jwk = innerProtected.Key;
                if (jwk == null)
                {
                    throw new MalformedException("The inner JWS has't a 'jwk' field");
                }

                // Check that the inner JWS verifies using the key in its "jwk" field.
                if (!js.Verify(jwk.GetPublicKey()))
                {
                    throw new MalformedException("The inner JWT not verified");
                }

                // Check that the payload of the inner JWS is a well-formed keyChange object (as described above).
                ChangeKey param = innerJWS.GetPayload<ChangeKey>();

                // Check that the "url" parameters of the inner and outer JWSs are the same.
                if (reqProtected.Url != innerProtected.Url)
                {
                    throw new MalformedException("The 'url' parameters of the inner and outer JWSs are not the same");
                }

                // Check that the "account" field of the keyChange object contains the URL for the account matching the old key (i.e., the "kid" field in the outer JWS).
                if (reqProtected.KeyID != param.Account)
                {
                    throw new MalformedException("The 'account' field not contains the URL for the account matching the old key");
                }

                // Check that the "oldKey" field of the keyChange object is the same as the account key for the account in question.
                var account = AccountService.GetByPublicKey(param.Key);
                var account2 = GetAccount(reqProtected.KeyID);
                if (account.Id != account2.Id)
                {
                    throw new MalformedException("The 'oldKey' is the not same as the account key");
                }

                // Check that no account exists whose account key is the same as the key in the "jwk" header parameter of the inner JWS.
                // in repository


                response.Content = AccountService.ChangeKey(acc.Id, jwk);
                response.StatusCode = 200; // Ok

            }, request);
        }

        #endregion

        #region Order management

        public AcmeResponse CreateOrder(AcmeRequest request)
        {
            return WrapAction((response) =>
            {
                throw new NotImplementedException();
            }, request);
        }

        public AcmeResponse PostOrder(AcmeRequest request)
        {
            return WrapAction((response) =>
            {
                throw new NotImplementedException();
            }, request);
        }

        #endregion
    }
}
