using System.Runtime.Serialization;

namespace PeculiarVentures.ACME.Protocol
{
    public enum ErrorType
    {
        /// <summary>
        /// The request specified an account that
        /// does not exist
        /// </summary>
        [EnumMember(Value = "urn:ietf:params:acme:error:accountDoesNotExist")]
        AccountDoesNotExist,
        /// <summary>
        /// The request specified a certificate to
        /// be revoked that has already been
        /// revoked
        /// </summary>
        [EnumMember(Value = "urn:ietf:params:acme:error:alreadyRevoked")]
        AlreadyRevoked,
        /// <summary>
        /// The CSR is unacceptable (e.g., due to a
        /// short key)
        /// </summary>
        [EnumMember(Value = "urn:ietf:params:acme:error:badCSR")]
        BadCSR,
        /// <summary>
        /// The client sent an unacceptable anti-
        /// replay nonce
        /// </summary>
        [EnumMember(Value = "urn:ietf:params:acme:error:badNonce")]
        BadNonce,
        /// <summary>
        /// The JWS was signed by a public key the
        /// server does not support
        /// </summary>
        [EnumMember(Value = "urn:ietf:params:acme:error:badPublicKey")]
        BadPublicKey,
        /// <summary>
        /// The revocation reason provided is not
        /// allowed by the server
        /// </summary>
        [EnumMember(Value = "urn:ietf:params:acme:error:badRevocationReason")]
        BadRevocationReason,
        /// <summary>
        /// The JWS was signed with an algorithm
        /// the server does not support
        /// </summary>
        [EnumMember(Value = "urn:ietf:params:acme:error:badSignatureAlgorithm")]
        BadSignatureAlgorithm,
        /// <summary>
        /// Certification Authority Authorization
        /// (CAA) records forbid the CA from
        /// issuing a certificate
        /// </summary>
        [EnumMember(Value = "urn:ietf:params:acme:error:caa")]
        Caa,
        /// <summary>
        /// Specific error conditions are indicated
        /// in the "subproblems" array
        /// </summary>
        [EnumMember(Value = "urn:ietf:params:acme:error:compound")]
        Compound,
        /// <summary>
        /// The server could not connect to
        /// validation target
        /// </summary>
        [EnumMember(Value = "urn:ietf:params:acme:error:connection")]
        Connection,
        /// <summary>
        /// There was a problem with a DNS query
        /// during identifier validation
        /// </summary>
        [EnumMember(Value = "urn:ietf:params:acme:error:dns")]
        Dns,
        /// <summary>
        /// The request must include a value for
        /// the "externalAccountBinding" field
        /// </summary>
        [EnumMember(Value = "urn:ietf:params:acme:error:externalAccountRequired")]
        ExternalAccountRequired,
        /// <summary>
        /// Response received didn't match the
        /// challenge's requirements
        /// </summary>
        [EnumMember(Value = "urn:ietf:params:acme:error:incorrectResponse")]
        IncorrectResponse,
        /// <summary>
        /// A contact URL for an account was
        /// invalid
        /// </summary>
        [EnumMember(Value = "urn:ietf:params:acme:error:invalidContact")]
        InvalidContact,
        /// <summary>
        /// The request message was malformed
        /// </summary>
        [EnumMember(Value = "urn:ietf:params:acme:error:malformed")]
        Malformed,
        /// <summary>
        /// The request attempted to finalize an
        /// order that is not ready to be finalized
        /// </summary>
        [EnumMember(Value = "urn:ietf:params:acme:error:orderNotReady")]
        OrderNotReady,
        /// <summary>
        /// The request exceeds a rate limit
        /// </summary>
        [EnumMember(Value = "urn:ietf:params:acme:error:rateLimited")]
        RateLimited,
        /// <summary>
        /// The server will not issue certificates
        /// for the identifier
        /// </summary>
        [EnumMember(Value = "urn:ietf:params:acme:error:rejectedIdentifier")]
        RejectedIdentifier,
        /// <summary>
        /// The server experienced an internal
        /// error
        /// </summary>
        [EnumMember(Value = "urn:ietf:params:acme:error:serverInternal")]
        ServerInternal,
        /// <summary>
        /// The server received a TLS error during
        /// validation
        /// </summary>
        [EnumMember(Value = "urn:ietf:params:acme:error:tls")]
        Tls,
        /// <summary>
        /// The client lacks sufficient
        /// authorization
        /// </summary>
        [EnumMember(Value = "urn:ietf:params:acme:error:unauthorized")]
        Unauthorized,
        /// <summary>
        /// A contact URL for an account used an
        /// unsupported protocol scheme
        /// </summary>
        [EnumMember(Value = "urn:ietf:params:acme:error:unsupportedContact")]
        UnsupportedContact,
        /// <summary>
        /// An identifier is of an unsupported type
        /// </summary>
        [EnumMember(Value = "urn:ietf:params:acme:error:unsupportedIdentifier")]
        UnsupportedIdentifier,
        /// <summary>
        /// Visit the "instance" URL and take
        /// actions specified there
        /// </summary>
        [EnumMember(Value = "urn:ietf:params:acme:error:userActionRequired")]
        UserActionRequired,
    }
}