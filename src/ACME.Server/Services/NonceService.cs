using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Server.Services
{
    public class NonceService : BaseService, INonceService
    {
        public INonceRepository NonceRepository { get; }

        public NonceService(
            INonceRepository nonceRepository,
            IOptions<ServerOptions> options)
            : base(options)
        {
            NonceRepository = nonceRepository
                ?? throw new ArgumentNullException(nameof(nonceRepository));
        }

        public string Create()
        {
            var nonce = NonceRepository.Create();

            return nonce;
        }

        public void Validate(string nonce)
        {
            AssertNonce(nonce);

            if (!NonceRepository.Contains(nonce))
            {
                throw new BadNonceException();
            }
            NonceRepository.Remove(nonce);
        }

        protected void AssertNonce(string nonce)
        {
            if (nonce is null)
            {
                throw new MalformedException("'nonce' is empty");
            }

            // If the value of a "nonce" header parameter is not valid
            // according to this encoding, then the verifier MUST reject the JWS as
            // malformed.
            var regex = new Regex("^[a-zA-Z0-9_-]+$");
            if (!regex.Match(nonce).Success)
            {
                throw new MalformedException("'nonce' is not valid encoded value");
            }
        }
    }
}
