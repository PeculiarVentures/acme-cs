using System.Runtime.Serialization;

namespace PeculiarVentures.ACME.Web.Jwe
{
    public enum Encryptions
    {
        //AES_128_CBC_HMAC_SHA_256 authenticated encryption using a 256 bit key.
        [EnumMember(Value = "A128CBC-HS256")]
        A128CBC_HS256,

        //AES_192_CBC_HMAC_SHA_384 authenticated encryption using a 384 bit key.
        [EnumMember(Value = "A192CBC-HS384")]
        A192CBC_HS384,

        //AES_256_CBC_HMAC_SHA_512 authenticated encryption using a 512 bit key.
        [EnumMember(Value = "A256CBC-HS512")]
        A256CBC_HS512,

        A128GCM,
        A192GCM,
        A256GCM,
    }
}
