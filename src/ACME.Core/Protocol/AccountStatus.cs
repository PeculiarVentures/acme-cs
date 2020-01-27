using System.Runtime.Serialization;

namespace PeculiarVentures.ACME.Protocol
{
    public enum AccountStatus
    {
        [EnumMember(Value = "valid")]
        Valid,

        [EnumMember(Value = "deactivated")]
        Deactivated,

        [EnumMember(Value = "revoked")]
        Revoked,
    }
}