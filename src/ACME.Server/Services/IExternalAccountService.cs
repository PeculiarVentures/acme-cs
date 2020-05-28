using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Server.Services
{
    public interface IExternalAccountService
    {
        /// <summary>
        /// Creates new <see cref="IExternalAccount"/>
        /// </summary>
        /// <param name="data"></param>
        IExternalAccount Create(object data);

        /// <summary>
        /// Returns <see cref="IExternalAccount"/> by specific id
        /// </summary>
        /// <param name="id"><see cref="IExternalAccount"/> specific id</param>
        IExternalAccount GetById(int id);

        /// <summary>
        /// Returns <see cref="IExternalAccount"/> by kid
        /// </summary>
        /// <param name="kid"><see cref="IExternalAccount"/> kid</param>
        /// <returns></returns>
        IExternalAccount GetById(string kid);

        /// <summary>
        /// Validates <see cref="IExternalAccount"/>
        /// </summary>
        /// <param name="accountKey">JSON Web Key (JWK)</param>
        /// <param name="token">JSON Web Signature (JWS)</param>
        /// <returns></returns>
        IExternalAccount Validate(JsonWebKey accountKey, JsonWebSignature token);
    }
}
