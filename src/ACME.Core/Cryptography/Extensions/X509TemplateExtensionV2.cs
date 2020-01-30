using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Asn1;

namespace PeculiarVentures.ACME.Cryptography
{
    /// <summary>
    /// Defines methods and properties that can be used to initialize or retrieve a CertificateTemplate extension V2
    /// </summary>
    /// <see cref="https://technet.microsoft.com/en-us/aa378274(v=vs.71)"/>
    public class X509TemplateExtensionV2 : X509Extension, IAsnEncodableData
    {
        #region Constructors
        /// <summary>
        /// Creates extension from raw data
        /// </summary>
        /// <param name="rawData">Asn1Sequence</param>
        /// <param name="critical"></param>
        public X509TemplateExtensionV2(byte[] rawData, bool critical) : base(new Oid(OidExtensions.TemplateV2), rawData, critical)
        {
            Import(rawData);
        }

        /// <summary>
        /// Creates extension from params
        /// </summary>
        /// <param name="id">Template identifier</param>
        /// <param name="majorVersion">Number major version</param>
        /// <param name="minorVersion">Number minor version</param>
        /// <param name="isCritical"></param>
        public X509TemplateExtensionV2(Oid id, int majorVersion, int minorVersion = 0, bool isCritical = false)
        {
            if (majorVersion < 0 || minorVersion < 0)
            {
                throw new ArgumentException("Version can not be less than 0");
            }
            Oid = new Oid(OidExtensions.TemplateV2);
            Critical = isCritical;
            Id = id ?? throw new ArgumentNullException(nameof(id));
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
            RawData = Export();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Extension identifier
        /// </summary>
        public Oid Id
        {
            get;
            private set;
        }

        /// <summary>
        /// Number major version
        /// </summary>
        public int MajorVersion
        {
            get;
            private set;
        }

        /// <summary>
        /// Number minor version
        /// </summary>
        public int MinorVersion
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
            if(Id == null)
            {
                throw new ArgumentException();
            }

            var param = new List<Asn1Encodable>();
            param.Add(new DerObjectIdentifier(Id.Value));
            param.Add(new DerInteger(MajorVersion));
            param.Add(new DerInteger(MinorVersion));
            
            var seq = new DerSequence(param.ToArray());
            return seq.GetEncoded();
        }

        /// <summary>
        /// Parse Extension from raw data and sets properties
        /// </summary>
        /// <param name="rawData">Asn1Sequence</param>
        /// <exception cref="ArgumentException"/>
        public void Import(byte[] rawData)
        {
            Reset();
            var value = Asn1Object.FromByteArray(rawData);

            if (!(value is Asn1Sequence))
            {
                throw new ArgumentException("Incorrect");
            }

            var seq = Asn1Sequence.GetInstance(value);

            if (!(seq.Count == 2 || seq.Count == 3))
            {
                throw new ArgumentException();
            }

            if (!(seq[0] is DerObjectIdentifier))
            {
                throw new ArgumentException();
            }
            Id = new Oid(DerObjectIdentifier.GetInstance(seq[0]).Id);

            if (!(seq[1] is DerInteger))
            {
                throw new ArgumentException();
            }
            MajorVersion = DerInteger.GetInstance(seq[1]).Value.IntValue;

            if (seq.Count == 3)
            {
                if (!(seq[2] is DerInteger))
                {
                    throw new ArgumentException();
                }
                MinorVersion = DerInteger.GetInstance(seq[2]).Value.IntValue;
            }
        }

        /// <summary>
        /// Resets all params on null
        /// </summary>
        public void Reset()
        {
            Id = null;
            MajorVersion = 0;
            MinorVersion = 0;
        }
        #endregion
    }
}
