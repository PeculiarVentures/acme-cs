using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Server.Controllers
{
    public interface IAcmeController
    {
        /// <summary>
        /// Server Options
        /// </summary>
        ServerOptions Options { get; }

        /// <summary>
        /// Returns <see cref="Directory"/> with controllers URLs
        /// </summary>
        /// <param name="request"></param>
        AcmeResponse GetDirectory(AcmeRequest request);

        /// <summary>
        /// Returns new nonce
        /// </summary>
        /// <param name="request"></param>
        AcmeResponse GetNonce(AcmeRequest request);

        /// <summary>
        /// Returns existing or new Account.
        /// If OnlyReturnExisting field is present with the value "true",
        /// then the server returns exception if account does not already exist.  
        /// </summary>
        /// <param name="request"></param>
        AcmeResponse CreateAccount(AcmeRequest request);

        /// <summary>
        /// Updates Account
        /// </summary>
        /// <param name="request"></param>
        AcmeResponse PostAccount(AcmeRequest request);

        /// <summary>
        /// Changes JSON Web Key (JWK) for account
        /// </summary>
        /// <param name="acmeRequest"></param>
        AcmeResponse ChangeKey(AcmeRequest acmeRequest);

        /// <summary>
        /// Returns existing or new order
        /// </summary>
        /// <param name="request"></param>
        AcmeResponse CreateOrder(AcmeRequest request);

        /// <summary>
        /// Returns order
        /// </summary>
        /// <param name="request"></param>
        /// <param name="orderId">Specific order id</param>
        AcmeResponse PostOrder(AcmeRequest request, int orderId);

        /// <summary>
        /// Returns order list
        /// </summary>
        /// <param name="request"></param>
        AcmeResponse PostOrders(AcmeRequest request);

        /// <summary>
        /// Returns authorization
        /// </summary>
        /// <param name="acmeRequest"></param>
        /// <param name="authzId">Specific authorization id</param>
        AcmeResponse PostAuthorization(AcmeRequest acmeRequest, int authzId);

        /// <summary>
        /// Returns challenge
        /// </summary>
        /// <param name="request"></param>
        /// <param name="challengeId">Specific challenge id</param>
        AcmeResponse PostChallenge(AcmeRequest request, int challengeId);

        /// <summary>
        /// Finalizes order
        /// </summary>
        /// <param name="request"></param>
        /// <param name="orderId">Specific order id</param>
        AcmeResponse FinalizeOrder(AcmeRequest request, int orderId);

        /// <summary>
        /// Returns certificate
        /// </summary>
        /// <param name="acmeRequest"></param>
        /// <param name="id"></param>
        AcmeResponse GetCertificate(AcmeRequest acmeRequest, string id);

        /// <summary>
        /// Revokes certificate
        /// </summary>
        /// <param name="request"></param>
        AcmeResponse RevokeCertificate(AcmeRequest request);
    }
}