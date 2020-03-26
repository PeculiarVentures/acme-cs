using System.Runtime.Serialization;

namespace PeculiarVentures.ACME.Protocol
{
    public enum TemplateIdentifierType
    {
        [EnumMember(Value = "rfc822")]
        RFC822,

        [EnumMember(Value = "dns")]
        DNS,

        [EnumMember(Value = "iPAddress")]
        IPAddress,

        [EnumMember(Value = "commonName")]
        CommonName,

        [EnumMember(Value = "upn")]
        UserPrincipalName,
    }
}