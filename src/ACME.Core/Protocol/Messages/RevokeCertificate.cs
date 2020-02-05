using Newtonsoft.Json;

namespace PeculiarVentures.ACME.Protocol.Messages
{
    /// <summary>
    /// Certificate Revocation.
    /// </summary>
    /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-18#section-7.6"/>
    public class RevokeCertificate : BaseObject
    {
        /// <summary>
        /// The certificate to be revoked, in the base64url-encoded version of the DER format.
        /// </summary>
        /// certificate (required, string)
        [JsonProperty("certificate")]
        [JsonRequired]
        public string Certificate { get; set; }

        /// <summary>
        /// One of the revocation reasonCodes to be used when generating OCSP responses and CRLs.
        /// </summary>
        /// reason (optional, int)
        [JsonProperty("reason")]
        public RevokeReason Reason { get; set; } = RevokeReason.Unspecified;
    }
}
