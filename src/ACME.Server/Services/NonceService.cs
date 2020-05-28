using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;

namespace PeculiarVentures.ACME.Server.Services
{
    /// <summary>
    /// Nonce service
    /// </summary>
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

        /// <inheritdoc/>
        public string Create()
        {
            var nonce = NonceRepository.Create();
            return nonce;
        }

        /// <inheritdoc/>
        public void Validate(string nonce)
        {
            AssertNonce(nonce);

            if (!NonceRepository.Contains(nonce))
            {
                throw new BadNonceException();
            }
            NonceRepository.Remove(nonce);
        }

        /// <summary>
        /// Asserts nonce
        /// </summary>
        /// <param name="nonce"></param>
        /// <exception cref="MalformedException"/>
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
