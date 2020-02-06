using System.Runtime.Serialization;

namespace PeculiarVentures.ACME.Web
{
    /// <summary>
    /// 
    /// </summary>
    /// <see cref="https://www.iana.org/assignments/jose/jose.xhtml#web-key-elliptic-curve"/>
    public enum EllipticCurvesEnum
    {
        [EnumMember]
        Default,

        [EnumMember(Value = "P-256")]
        P256,

        [EnumMember(Value = "P-384")]
        P384,

        [EnumMember(Value = "P-512")]
        P512,

        [EnumMember(Value = "Ed25519")]
        Ed25519,

        [EnumMember(Value = "Ed448")]
        Ed448,

        [EnumMember(Value = "X25519")]
        X25519,

        [EnumMember(Value = "X448")]
        X448,
    }
}
