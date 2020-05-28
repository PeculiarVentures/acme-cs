namespace PeculiarVentures.ACME.Server.Services
{
    public interface INonceService
    {
        /// <summary>
        /// Creates new nonce
        /// </summary>
        string Create();

        /// <summary>
        /// Validates nonce
        /// </summary>
        /// <param name="nonce"></param>
        /// <exception cref="BadNonceException"/>
        void Validate(string nonce);
    }
}