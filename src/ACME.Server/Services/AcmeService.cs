using System;
using System.Text.RegularExpressions;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Server.Services
{
    public abstract class AcmeService
    {
        protected AcmeService(
            INonceRepository nonceRepository,
            IAccountRepository accountRepository)
        {
            NonceRepository = nonceRepository ?? throw new ArgumentNullException(nameof(nonceRepository));
            AccountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        }

        protected IAccountRepository AccountRepository { get; }
        protected INonceRepository NonceRepository { get; }

        public void AssertRequest(AcmeRequest request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            // The JWS MUST be in the Flattened JSON Serialization[RFC7515]

            // The JWS MUST NOT have multiple signatures

            // The JWS Unencoded Payload Option[RFC7797] MUST NOT be used

            // The JWS Unprotected Header[RFC7515] MUST NOT be used

            // The JWS Payload MUST NOT be detached

            // The JWS Protected Header MUST include the following fields 'alg', 'nonce', 'url'
            AssertProtectedRequiredFields(request);
            AssertSignatureAlgorithm(request);
            AssertSignature(request);
        }

        private void AssertSignatureAlgorithm(AcmeRequest request)
        {
            var header = request.GetProtected();
            switch (header.Algorithm)
            {
                case AlgorithmsEnum.RS1:
                case AlgorithmsEnum.RS256:
                case AlgorithmsEnum.RS384:
                case AlgorithmsEnum.RS512:
                case AlgorithmsEnum.PS1:
                case AlgorithmsEnum.PS256:
                case AlgorithmsEnum.PS384:
                case AlgorithmsEnum.PS512:
                case AlgorithmsEnum.ES256:
                case AlgorithmsEnum.ES384:
                case AlgorithmsEnum.ES512:
                    break;
                default:
                    throw new AcmeException(ErrorType.BadSignatureAlgorithm);
            }
        }

        private void AssertProtectedRequiredFields(AcmeRequest request)
        {
            var header = request.GetProtected();
            if (header.Algorithm == AlgorithmsEnum.None)
            {
                throw new MalformedException($"Cannot get required field 'alg'");
            }
            if (header.Nonce == null)
            {
                throw new MalformedException($"Cannot get required field 'nonce'");
            }
            if (header.Url == null)
            {
                throw new MalformedException($"Cannot get required field 'url'");
            }
        }

        private void AssertSignature(AcmeRequest request)
        {
            var header = request.GetProtected();
            JsonWebKey key = null;
            if (header.Key != null)
            {
                key = header.Key;
            }
            else if (header.KeyID != null)
            {
                var account = GetAccount(header.KeyID);
                key = account.PublicKey;
            }

            if (request.Verify(key.GetPublicKey()))
            {
                throw new AcmeException(ErrorType.BadPublicKey);
            }

        }

        /// <summary>
        /// Gets account by kid
        /// </summary>
        /// <param name="kid">Key identifier link</param>
        /// <returns>Account</returns>
        /// <exception cref="MalformedException"/>
        /// <exception cref="AccountDoesNotExistException"/>
        public IAccount GetAccount(string kid)
        {
            // Get Id from http link
            var regEx = new Regex("\\/(\\d+)");
            var match = regEx.Match(kid);
            if (!match.Success)
            {
                throw new MalformedException($"Cannot get Key Id from link {kid}");
            }

            var account = AccountRepository.GetById(int.Parse(match.Groups[0].Value));
            if (account == null)
            {
                throw new AccountDoesNotExistException();
            }
            return account;
        }

        public AcmeResponse CreateResponse()
        {
            // TODO: Add directory link
            return new AcmeResponse
            {
                StatusCode = 200, // OK
                ReplayNonce = NonceRepository.Create(),
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
    }
}