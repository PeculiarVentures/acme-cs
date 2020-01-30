namespace PeculiarVentures.ACME.Server.Services
{
    public interface INonceService
    {
        string Create();
        void Validate(string nonce);
    }
}