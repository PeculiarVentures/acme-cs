using System.Runtime.Serialization;

namespace PeculiarVentures.ACME.Web.Jwe
{
    public enum Algorithms
    {
        //RSAES with PKCS #1 v1.5 padding, RFC 3447
        RSA1_5,

        //RSAES using Optimal Asymmetric Jwe Padding, RFC 3447
        [EnumMember(Value = "RSA-OAEP")]
        RSA_OAEP,

        //RSAES with SHA-256 using Optimal Asymmetric Jwe Padding, RFC 3447
        [EnumMember(Value = "RSA-OAEP-256")]
        RSA_OAEP_256,

        //Direct use of pre-shared symmetric key
        [EnumMember(Value = "dir")]
        DIR,

        //AES Key Wrap Algorithm using 128 bit keys, RFC 3394
        A128KW,

        //AES Key Wrap Algorithm using 192 bit keys, RFC 3394
        A192KW,

        //AES Key Wrap Algorithm using 256 bit keys, RFC 3394 
        A256KW,

        //Elliptic Curve Diffie Hellman key agreement
        [EnumMember(Value = "ECDH-ES")]
        ECDH_ES,

        //Elliptic Curve Diffie Hellman key agreement with AES Key Wrap using 128 bit key
        [EnumMember(Value = "ECDH-ES+A128KW")]
        ECDH_ES_A128KW,

        //Elliptic Curve Diffie Hellman key agreement with AES Key Wrap using 192 bit key
        [EnumMember(Value = "ECDH-ES+A192KW")]
        ECDH_ES_A192KW,

        //Elliptic Curve Diffie Hellman key agreement with AES Key Wrap using 256 bit key
        [EnumMember(Value = "ECDH-ES+A256KW")]
        ECDH_ES_A256KW,

        //Password Based Jwe using PBES2 schemes with HMAC-SHA and AES Key Wrap using 128 bit key
        [EnumMember(Value = "PBES2-HS256+A128KW")]
        PBES2_HS256_A128KW,

        //Password Based Jwe using PBES2 schemes with HMAC-SHA and AES Key Wrap using 192 bit key
        [EnumMember(Value = "PBES2-HS384+A192KW")]
        PBES2_HS384_A192KW,

        //Password Based Jwe using PBES2 schemes with HMAC-SHA and AES Key Wrap using 256 bit key
        [EnumMember(Value = "PBES2-HS512+A256KW")]
        PBES2_HS512_A256KW,

        //AES GCM Key Wrap Algorithm using 128 bit keys
        A128GCMKW,

        //AES GCM Key Wrap Algorithm using 192 bit keys
        A192GCMKW,

        //AES GCM Key Wrap Algorithm using 256 bit keys
        A256GCMKW,
    }
}
