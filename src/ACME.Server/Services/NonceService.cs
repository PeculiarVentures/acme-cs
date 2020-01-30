﻿using System;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Server.Services
{
    public class NonceService : INonceService
    {
        public INonceRepository NonceRepository { get; }

        public NonceService(INonceRepository nonceRepository)
        {
            NonceRepository = nonceRepository ?? throw new ArgumentNullException(nameof(nonceRepository));
        }

        public string Create()
        {
            return NonceRepository.Create();
        }

        public void Validate(string nonce)
        {
            if (NonceRepository.Contains(nonce))
            {
                throw new BadNonceException();
            }
            NonceRepository.Remove(nonce);
        }
    }
}
