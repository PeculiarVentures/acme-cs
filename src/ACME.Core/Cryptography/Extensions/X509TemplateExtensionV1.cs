using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Asn1;

namespace PeculiarVentures.ACME.Cryptography
{
    /// <summary>
    /// Defines methods and properties that can be used to initialize or retrieve a CertificateTemplate extension V1
    /// </summary>
    /// <see cref="https://technet.microsoft.com/en-us/aa378276(v=vs.71)"/>
    public class X509TemplateExtensionV1 : X509Extension, IAsnEncodableData
    {
        #region Constructors
        /// <summary>
        /// Creates extension from raw data
        /// </summary>
        /// <param name="rawData">Asn1Sequence</param>
        /// <param name="critical"></param>
        public X509TemplateExtensionV1(byte[] rawData, bool critical) : base(new Oid(OidExtensions.TemplateV1), rawData, critical)
        {
            Import(rawData);
        }

        /// <summary>
        /// Creates extension from params
        /// </summary>
        /// <param name="certificateTemplate">Certificate template name</param>
        /// <param name="isCritical"></param>
        public X509TemplateExtensionV1(string certificateTemplate, bool isCritical = false)
        {
            Oid = new Oid(OidExtensions.TemplateV2);
            Critical = isCritical;
            CertificateTemplate = certificateTemplate ?? throw new ArgumentNullException(nameof(certificateTemplate));
            RawData = Export();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Certificate template name
        /// </summary>
        public string CertificateTemplate
        {
            get;
            private set;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns the extension in raw data
        /// </summary>
        /// <returns></returns>
        public byte[] Export()
        {
            if(string.IsNullOrEmpty(CertificateTemplate))
            {
                throw new ArgumentException();
            }

            var param = new List<Asn1Encodable>();
            param.Add(new DerBmpString(CertificateTemplate));
            
            var seq = new DerSequence(param.ToArray());
            return seq.GetEncoded();
        }

        /// <summary>
        /// Parses Extension from raw data and sets properties
        /// </summary>
        /// <param name="rawData">Asn1Sequence</param>
        /// <exception cref="ArgumentException"/>
        public void Import(byte[] rawData)
        {
            Reset();
            var value = Asn1Object.FromByteArray(rawData);

            if (!(value is DerBmpString))
            {
                throw new ArgumentException();
            }
            CertificateTemplate = value.ToString();
        }

        /// <summary>
        /// Resets all properties on null
        /// </summary>
        public void Reset()
        {
            CertificateTemplate = null;
        }
        #endregion
    }
}
