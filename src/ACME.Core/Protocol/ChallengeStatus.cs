using System.Runtime.Serialization;

namespace PeculiarVentures.ACME.Protocol
{
    public enum ChallengeStatus
    {
        [EnumMember(Value = "pending")]
        Pending,

        [EnumMember(Value = "processing")]
        Processing,

        [EnumMember(Value = "valid")]
        Valid,

        [EnumMember(Value = "invalid")]
        Invalid,
    }
}