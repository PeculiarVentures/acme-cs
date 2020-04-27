using System;
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
            if (!NonceRepository.Contains(nonce))
            {
                throw new BadNonceException();
            }
            NonceRepository.Remove(nonce);
        }
    }
}
