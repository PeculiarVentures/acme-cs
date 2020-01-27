using System.Runtime.Serialization;

namespace PeculiarVentures.ACME.Protocol
{
    public enum OrderStatus
    {
        [EnumMember(Value = "pending")]
        Pending,

        [EnumMember(Value = "ready")]
        Ready,

        [EnumMember(Value = "processing")]
        Processing,

        [EnumMember(Value = "valid")]
        Valid,

        [EnumMember(Value = "invalid")]
        Invalid,
    }
}