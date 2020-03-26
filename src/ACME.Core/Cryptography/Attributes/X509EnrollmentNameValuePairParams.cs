using System;
namespace PeculiarVentures.ACME.Cryptography
{
    public class CertificateRequestParams
    {
        public CertificateRequestParams()
        {
        }

        public SignatureAlgorithmEnum SignatureAlgorithm
        {
            get;
            set;
        } = SignatureAlgorithmEnum.Sha256WithRsa;

        public string CommonName
        {
            get;
            set;
        }

        public Attribute[] Attributes
        {
            get;
            set;
        } = new Attribute[0];
    }
}
