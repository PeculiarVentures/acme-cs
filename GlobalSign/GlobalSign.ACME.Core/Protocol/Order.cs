using System;

namespace GlobalSign.ACME.Protocol
{
    /// <summary>
    /// JSON ACME account object.
    /// See <see href="https://tools.ietf.org/html/rfc8555#section-7.1.3">RFC8555</see>
    /// </summary>
    public class Order : PeculiarVentures.ACME.Protocol.Order
    {
        /// <summary>
        /// A URK for them template
        /// </summary>
        public string Template { get; set; }
    }
}
