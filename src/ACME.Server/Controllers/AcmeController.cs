﻿using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Protocol.Messages;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Services;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Server.Controllers
{
    public class AcmeController : IAcmeController
    {
        public AcmeController(
            IDirectoryService directoryService,
            INonceService nonceService,
            IAccountService accountService,
            IOrderService orderService,
            IChallengeService challengeService,
            IAuthorizationService authorizationService,
            IConverterService converterService,
            IOptions<ServerOptions> options)
        {
            DirectoryService = directoryService ?? throw new ArgumentNullException(nameof(directoryService));
            NonceService = nonceService ?? throw new ArgumentNullException(nameof(nonceService));
            AccountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            OrderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            ChallengeService = challengeService ?? throw new ArgumentNullException(nameof(challengeService));
            AuthorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            ConverterService = converterService ?? throw new ArgumentNullException(nameof(converterService));
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public IDirectoryService DirectoryService { get; }
        public INonceService NonceService { get; }
        public IAccountService AccountService { get; }
        public IOrderService OrderService { get; }
        public IChallengeService ChallengeService { get; }
        public IAuthorizationService AuthorizationService { get; }
        public IConverterService ConverterService { get; }
        public IOptions<ServerOptions> Options { get; }

        public AcmeResponse CreateResponse()
        {
            return new AcmeResponse
            {
                StatusCode = 200, // OK
                ReplayNonce = NonceService.Create(),
            };
        }

        /// <summary>
        /// Wraps controller action
        /// </summary>
        /// <param name="action">Action</param>
        /// <param name="request">ACME request. If presents wrapper validates JWS</param>
        /// <returns></returns>
        public AcmeResponse WrapAction(Action<AcmeResponse> action, AcmeRequest request = null)
        {
            var response = CreateResponse();

            try
            {

                if (request != null)
                {
                    if (request.Method == "POST")
                    {
                        // TODO Check JWS

                        #region Check Nonce

                        var nonce = request.Token.GetProtected().Nonce ?? throw new BadNonceException();
                        NonceService.Validate(nonce);

                        #endregion
                    }

                }

                // Invoke action
                action.Invoke(response);
            }
            catch (Exception e)
            {
                response.StatusCode = 500; // Internal Server Error
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

        /// <summary>
        /// Recieves Id from URI
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        /// <exception cref="MalformedException"/>
        public int GetIdFromLink(string url)
        {
            var regEx = new Regex("\\/(\\d+)");
            var match = regEx.Match(url);
            if (!match.Success)
            {
                throw new MalformedException($"Cannot get Id from link {url}");
            }
            return int.Parse(match.Groups[1].Value);
        }

        #region Account management

        /// <summary>
        /// Gets account by kid
        /// </summary>
        /// <param name="kid">Key identifier link</param>
        /// <returns>Account</returns>
        /// <exception cref="MalformedException"/>
        /// <exception cref="AccountDoesNotExistException"/>
        protected IAccount GetAccount(string kid)
        {
            var account = AccountService.GetById(GetIdFromLink(kid));
            if (account == null)
            {
                throw new AccountDoesNotExistException();
            }

            if (account.Status == AccountStatus.Deactivated)
            {
                throw new UnauthorizedException("Account deactivated");
            }

            return account;
        }

        public AcmeResponse CreateAccount(AcmeRequest request)
        {
            return WrapAction(response =>
            {
                var @params = request.GetContent<NewAccount>();
                var account = AccountService.FindByPublicKey(request.Key);

                if (@params.OnlyReturnExisting == true)
                {
                    if (account == null)
                    {
                        throw new AccountDoesNotExistException();
                    }
                    response.Content = ConverterService.ToAccount(account);
                    response.StatusCode = 200; // Ok
                }
                else
                {
                    if (account == null)
                    {
                        // Create new account
                        account = AccountService.Create(request.Key, @params);
                        response.Content = ConverterService.ToAccount(account);
                        response.StatusCode = 201; // Created
                    }
                    else
                    {
                        // Existing account
                        response.Content = ConverterService.ToAccount(account);
                        response.StatusCode = 200; // Ok
                    }
                }

                // Set headers
                response.Location = $"{account.Id}";
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

        public void AssertAccountStatus(IAccount account)
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
                var account = GetAccount(reqProtected.KeyID);
                var jws = new JsonWebSignature();
                if (!jws.Verify(account.Key.GetPublicKey()))
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
                if (!jws.Verify(jwk.GetPublicKey()))
                {
                    throw new MalformedException("The inner JWT not verified");
                }

                // Check that the payload of the inner JWS is a well-formed keyChange object (as described above).
                if (innerJWS.TryGetPayload(out ChangeKey param))
                {
                    throw new MalformedException("The payload of the inner JWS is not a well-formed keyChange object");
                }

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
                var testAccount = AccountService.FindByPublicKey(param.Key);
                if (testAccount == null || testAccount.Id != account.Id)
                {
                    throw new MalformedException("The 'oldKey' is the not same as the account key");
                }

                // TODO Check that no account exists whose account key is the same as the key in the "jwk" header parameter of the inner JWS.
                // in repository

                var updatedAccount = AccountService.ChangeKey(account.Id, jwk);
                response.Content = ConverterService.ToAccount(updatedAccount);
                response.StatusCode = 200; // Ok

            }, request);
        }

        #endregion

        #region Order management

        public AcmeResponse CreateOrder(AcmeRequest request)
        {
            return WrapAction((response) =>
            {
                var account = GetAccount(request.KeyId);
                var @params = request.GetContent<NewOrder>();

                var order = OrderService.GetActual(account.Id, @params.Identifiers.ToArray());
                if (order == null)
                {
                    order = OrderService.Create(account.Id, @params);
                    response.StatusCode = 201; // Created
                }
                response.Location = $"{order.Id}";
                response.Content = ConverterService.ToOrder(order);
            });
        }

        public AcmeResponse PostOrder(AcmeRequest request, int orderId)
        {
            return WrapAction((response) =>
            {
                var account = GetAccount(request.KeyId);
                var order = OrderService.GetById(account.Id, orderId)
                    ?? throw new MalformedException("Order not found");

                response.Content = ConverterService.ToOrder(order);
            }, request);
        }

        public AcmeResponse FinalizeOrder(AcmeRequest request)
        {
            return WrapAction((response) =>
            {
                var account = GetAccount(request.KeyId);
                var @params = request.GetContent<FinalizeOrder>();

                var order = OrderService.EnrollCertificate(
                    accountId: account.Id,
                    orderId: GetIdFromLink(request.Url),
                    @params: @params);

                response.Content = ConverterService.ToOrder(order);
            }, request);
        }

        #endregion

        #region Challenge
        public AcmeResponse PostChallenge(AcmeRequest request, int challengeId)
        {
            return WrapAction((response) =>
            {
                var account = GetAccount(request.KeyId);

                var challenge = ChallengeService.GetById(challengeId);
                if (challenge == null)
                {
                    // TODO Check RFC
                    throw new MalformedException("Challenge not found");
                }
                if (challenge.AccountId != account.Id)
                {
                    // TODO Check RFC
                    throw new MalformedException("Access denied");
                }

                if (request.Token.IsPayloadEmptyObject)
                {
                    ChallengeService.Validate(challenge);
                }

                response.Content = ConverterService.ToChallenge(challenge);
            }, request);
        }

        #endregion

        public AcmeResponse PostAuthorization(AcmeRequest request, int authzId)
        {
            return WrapAction((response) =>
            {
                var account = GetAccount(request.KeyId);

                var authz = AuthorizationService.GetById(account.Id, authzId);

                response.Content = ConverterService.ToAuthorization(authz);
            }, request);
        }

        public AcmeResponse FinalizeOrder(AcmeRequest request, int orderId)
        {
            return WrapAction((response) =>
            {
                var account = GetAccount(request.KeyId);
                var @params = request.GetContent<FinalizeOrder>();
                var order = OrderService.EnrollCertificate(account.Id, orderId, @params);

                response.Content = ConverterService.ToOrder(order);
            }, request);
        }

        public AcmeResponse GetCertificate(AcmeRequest request, string thumbprint)
        {
            return WrapAction((response) =>
            {
                var account = GetAccount(request.KeyId);
                var certs = OrderService.GetCertificate(account.Id, thumbprint);

                switch (Options.Value.DownloadCertificateFormat)
                {
                    case DownloadCertificateFormat.PemCertificateChain:
                        {
                            var pem = PemConverter.Encode(certs.Select(o => o.RawData).ToArray(), "certificate");
                            response.Content = new MediaTypeContent("application/pem-certificate-chain", pem);
                        }
                        break;
                    case DownloadCertificateFormat.PkixCert:
                        {
                            var cert = new X509Certificate2(certs[0].RawData);
                            response.Content = new MediaTypeContent("application/pkix-cert", cert.RawData);
                        }
                        break;
                    case DownloadCertificateFormat.Pkcs7Mime:
                        {
                            var x509Certs = certs.Select(o => new X509Certificate2(o.RawData)).ToArray();
                            var x509Collection = new X509Certificate2Collection(x509Certs);
                            response.Content = new MediaTypeContent("application/pkcs7-mime", x509Collection.Export(X509ContentType.Pkcs7));
                        }
                        break;
                }
            }, request);
        }
    }
}