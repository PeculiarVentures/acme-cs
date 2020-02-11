using System;
using System.Runtime.Serialization;

namespace PeculiarVentures.ACME.Protocol
{
    public enum ChallengeType
    {
        [EnumMember(Value = "http-01")]
        http,
        [EnumMember(Value = "tls-01")]
        tls,
        [EnumMember(Value = "dns-01")]
        dns
    }
}
