using PeculiarVentures.ACME.Protocol.Messages;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;

namespace PeculiarVentures.ACME.Server
{
    /// <summary>
    /// Certificate enroll params
    /// </summary>
    public class CertificateEnrollParams
    {
        /// <summary>
        /// <see cref="IOrder"/>
        /// </summary>
        public IOrder Order { get; set; }

        /// <summary>
        /// Params to finalize
        /// </summary>
        public FinalizeOrder Params { get; set; }

        /// <summary>
        /// Any object.
        /// Allows transfer data from OnEnrollCertificateBefore to OnEnrollCertificateTask
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// Flag to cancel standard enroll certificate
        /// </summary>
        public bool Cancel { get; set; }
    }

}
