using System;
using System.Runtime.Serialization;

namespace PeculiarVentures.ACME.Protocol
{
    public enum ExternalAccountStatus
    {
        /// <summary>
        /// External Account was created and ready for binding to ACME Account
        /// </summary>
        [EnumMember(Value = "pending")]
        Pending,

        /// <summary>
        /// External Account Key expired
        /// </summary>
        [EnumMember(Value = "expired")]
        Expired,

        /// <summary>
        /// External Account key verified
        /// </summary>
        [EnumMember(Value = "valid")]
        Valid,

        [EnumMember(Value = "invalid")]
        Invalid,
    }
}
