using System.Runtime.Serialization;

namespace PeculiarVentures.ACME.Protocol
{
    public enum AuthorizationStatus
    {
        [EnumMember(Value = "pending")]
        Pending,

        [EnumMember(Value = "valid")]
        Valid,

        [EnumMember(Value = "invalid")]
        Invalid,

        [EnumMember(Value = "deactivated")]
        Deactivated,

        [EnumMember(Value = "expired")]
        Expired,

        [EnumMember(Value = "revoked")]
        Revoked,
    }
}