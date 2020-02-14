namespace PeculiarVentures.ACME.Server
{
    public class ExternalAccountOptions
    {
        /// <summary>
        /// MAC key size in bits. Default value is 256
        /// </summary>
        public int KeyLength { get; set; } = 256;
        /// <summary>
        /// Time when MAC key expires in minutes. Default value is 0
        /// </summary>
        public int ExpiresMinutes { get; set; } = 0;

        public ExternalAccountType Type { get; set; } = ExternalAccountType.None;
    }
}