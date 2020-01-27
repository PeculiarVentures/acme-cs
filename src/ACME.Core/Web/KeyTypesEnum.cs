using System.Runtime.Serialization;

namespace PeculiarVentures.ACME.Web
{
    /// <summary>
    /// 
    /// </summary>
    /// <see cref="https://www.iana.org/assignments/jose/jose.xhtml#web-key-types"/>
    public enum KeyTypesEnum
    {
        [EnumMember(Value = null)]
        NONE,

        [EnumMember(Value = "EC")]
        EC,

        [EnumMember(Value = "RSA")]
        RSA,

        [EnumMember(Value = "oct")]
        OctetSequence,

        [EnumMember(Value = "OKP")]
        OctetStringKeyPairs,
    }
}
