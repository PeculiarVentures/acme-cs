using System;
using System.Security.Cryptography;

namespace PeculiarVentures.ACME.Cryptography
{
    public class X509EnrollmentNameValuePairParams
    {
        public X509EnrollmentNameValuePairParams()
        {
        }

        public CertTypeEnum? CertType
        {
            get;
            set;
        }

        public Oid[] CertificateUsage
        {
            get;
            set;
        }
        public ValidityPeriodEnum? ValidityPeriod
        {
            get;
            set;
        }
        public int? ValidityPeriodUnits
        {
            get;
            set;
        }
        public DateTime? ExpirationDate
        {
            get;
            set;
        }
        public string CDC
        {
            get;
            set;
        }
        public string RMD
        {
            get;
            set;
        }
        public string CertificateTemplate
        {
            get;
            set;
        }
        public string SAN
        {
            get;
            set;
        }
        public string Challenge
        {
            get;
            set;
        }
        public string RequesterName
        {
            get;
            set;
        }
        public string Other
        {
            get;
            set;
        }
        public string CertFile
        {
            get;
            set;
        }
        public int? RequestId
        {
            get;
            set;
        }
    }
}
