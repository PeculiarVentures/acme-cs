using System.Collections.Generic;
using PeculiarVentures.ACME.Helpers;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;

namespace PeculiarVentures.ACME.Server.Data.EF.Core.Repositories
{
    public class NonceRepository : INonceRepository
    {
        private const int NONCE_SIZE = 20;

        private List<string> _items = new List<string>();

        public bool Contains(string value)
        {
            return _items.Contains(value);
        }

        public string Create()
        {
            var nonce = Generate(NONCE_SIZE);
            _items.Add(nonce);

            return nonce;
        }

        public void Remove(string value)
        {
            _items.Remove(value);
        }

        private string Generate(int size)
        {
            var rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            var buffer = new byte[size];
            rng.GetBytes(buffer);
            var nonce = Base64Url.Encode(buffer);

            return nonce;
        }
    }
}
