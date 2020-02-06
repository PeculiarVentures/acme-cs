using System.Runtime.Serialization;

namespace PeculiarVentures.ACME.Web
{
    /// <summary>
    /// 
    /// </summary>
    /// <see cref="https://www.iana.org/assignments/jose/jose.xhtml#web-signature-encryption-algorithms"/>
    public enum AlgorithmsEnum
    {
        [EnumMember(Value = "HS256")]
        HS256,

        [EnumMember(Value = "HS384")]
        HS384,

        [EnumMember(Value = "HS512")]
        HS512,

        [EnumMember(Value = "RS256")]
        RS256,

        [EnumMember(Value = "RS384")]
        RS384,

        [EnumMember(Value = "RS512")]
        RS512,

        [EnumMember(Value = "ES256")]
        ES256,

        [EnumMember(Value = "ES384")]
        ES384,

        [EnumMember(Value = "ES512")]
        ES512,

        [EnumMember(Value = "PS1")]
        PS1,

        [EnumMember(Value = "PS256")]
        PS256,

        [EnumMember(Value = "PS384")]
        PS384,

        [EnumMember(Value = "PS512")]
        PS512,

        [EnumMember(Value = "RSA1_5")]
        RSA1_5,

        [EnumMember(Value = "RSA-OAEP")]
        RSA_OAEP,

        [EnumMember(Value = "RSA-OAEP-256")]
        RSA_OAEP_256,

        [EnumMember(Value = "A128KW")]
        A128KW,

        [EnumMember(Value = "A192KW")]
        A192KW,

        [EnumMember(Value = "A256KW")]
        A256KW,

        [EnumMember(Value = "dir")]
        Dir,

        [EnumMember(Value = "ECDH-ES")]
        ECDH_ES,

        [EnumMember(Value = "ECDH-ES+A128KW")]
        ECDH_ES_A128KW,

        [EnumMember(Value = "ECDH-ES+A192KW")]
        ECDH_ES_A192KW,

        [EnumMember(Value = "ECDH-ES+A256KW")]
        ECDH_ES_A256KW,

        [EnumMember(Value = "A128GCMKW")]
        A128GCMKW,

        [EnumMember(Value = "A192GCMKW")]
        A192GCMKW,

        [EnumMember(Value = "A256GCMKW")]
        A256GCMKW,

        [EnumMember(Value = "PBES2-HS256+A128KW")]
        PBES2_HS256_A128KW,

        [EnumMember(Value = "PBES2-HS384+A192KW")]
        PBES2_HS384_A192KW,

        [EnumMember(Value = "PBES2-HS512+A256KW")]
        PBES2_HS512_A256KW,

        [EnumMember(Value = "A128CBC-HS256")]
        A128CBC_HS256,

        [EnumMember(Value = "A192CBC-HS384")]
        A192CBC_HS384,

        [EnumMember(Value = "A256CBC-HS512")]
        A256CBC_HS512,

        [EnumMember(Value = "A128GCM")]
        A128GCM,

        [EnumMember(Value = "A192GCM")]
        A192GCM,

        [EnumMember(Value = "A256GCM")]
        A256GCM,

        [EnumMember(Value = "EdDSA")]
        EdDSA,

        [EnumMember(Value = "RS1")]
        RS1,

        [EnumMember(Value = "RSA-OAEP-384")]
        RSA_OAEP_384,

        [EnumMember(Value = "RSA-OAEP-512")]
        RSA_OAEP_512,

        [EnumMember(Value = "A128CBC")]
        A128CBC,

        [EnumMember(Value = "A192CBC")]
        A192CBC,

        [EnumMember(Value = "A256CBC")]
        A256CBC,

        [EnumMember(Value = "A128CTR")]
        A128CTR,

        [EnumMember(Value = "A192CTR")]
        A192CTR,

        [EnumMember(Value = "A256CTR")]
        A256CTR,

        [EnumMember(Value = "HS1")]
        HS1,
    }
}
