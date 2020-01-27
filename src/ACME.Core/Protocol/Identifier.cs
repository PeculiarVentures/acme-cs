namespace PeculiarVentures.ACME.Protocol
{
    /// <summary>
    /// JSON ACME Identifir object
    /// </summary>
    public class Identifier
    {
        /// <summary>
        /// The type of identifier.
        /// </summary>
        /// type (required, string)
        public string Type { get; set; }

        /// <summary>
        /// The identifier itself.
        /// </summary>
        /// value (required, string)
        public string Value { get; set; }
    }
}