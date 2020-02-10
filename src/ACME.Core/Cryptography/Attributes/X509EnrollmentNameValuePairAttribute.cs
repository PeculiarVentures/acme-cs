using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;

namespace PeculiarVentures.ACME.Cryptography
{
    /// <summary>
    /// This attribute MUST be a collection of zero or more name-value pairs.
    /// </summary>
    /// <see cref="https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-wcce/92f07a54-2889-45e3-afd0-94b60daa80ec"/>
    public class X509EnrollmentNameValuePairAttribute : Attribute
    {
        #region Constructors
        public X509EnrollmentNameValuePairAttribute(byte[] rawData) : base(rawData) => Import(rawData);
        public X509EnrollmentNameValuePairAttribute() : base() => Reset();
        public X509EnrollmentNameValuePairAttribute(X509EnrollmentNameValuePairParams param) : base()
        {
            #region Sets propeties from param
            var seqList = new List<Asn1Encodable>();

            if (param.CertificateTemplate != null)
            {
                seqList.Add(CreateSequence(EnrollmentNameValuePairsEnum.CertificateTemplate.ToString(), param.CertificateTemplate));
            }
            if (param.CDC != null)
            {
                seqList.Add(CreateSequence(EnrollmentNameValuePairsEnum.CDC.ToString(), param.CDC));
            }
            if (param.CertFile != null)
            {
                seqList.Add(CreateSequence(EnrollmentNameValuePairsEnum.CertFile.ToString(), param.CertFile));
            }
            if (param.CertificateUsage != null)
            {
                string oidString = string.Join(", ", new List<Oid>(param.CertificateUsage).ConvertAll(i => i.Value.ToString()).ToArray());
                seqList.Add(CreateSequence(EnrollmentNameValuePairsEnum.CertificateUsage.ToString(), oidString));
            }
            if (param.CertType != null)
            {
                seqList.Add(CreateSequence(EnrollmentNameValuePairsEnum.CertType.ToString(), param.CertType.Value.ToString()));
            }
            if (param.Challenge != null)
            {
                seqList.Add(CreateSequence(EnrollmentNameValuePairsEnum.Challenge.ToString(), param.Challenge));
            }
            if (param.ExpirationDate != null)
            {
                seqList.Add(CreateSequence(EnrollmentNameValuePairsEnum.ExpirationDate.ToString(), param.ExpirationDate.Value.ToString()));
            }
            if (param.Other != null)
            {
                seqList.Add(CreateSequence(EnrollmentNameValuePairsEnum.Other.ToString(), param.Other));
            }
            if (param.RequesterName != null)
            {
                seqList.Add(CreateSequence(EnrollmentNameValuePairsEnum.RequesterName.ToString(), param.RequesterName));
            }
            if (param.RequestId != null)
            {
                seqList.Add(CreateSequence(EnrollmentNameValuePairsEnum.RequestId.ToString(), param.RequestId.Value.ToString()));
            }
            if (param.RMD != null)
            {
                seqList.Add(CreateSequence(EnrollmentNameValuePairsEnum.RMD.ToString(), param.RMD));
            }
            if (param.SAN != null)
            {
                seqList.Add(CreateSequence(EnrollmentNameValuePairsEnum.SAN.ToString(), param.SAN));
            }
            if (param.ValidityPeriod != null)
            {
                seqList.Add(CreateSequence(EnrollmentNameValuePairsEnum.ValidityPeriod.ToString(), param.ValidityPeriod.Value.ToString()));
            }
            if (param.ValidityPeriodUnits != null)
            {
                seqList.Add(CreateSequence(EnrollmentNameValuePairsEnum.ValidityPeriodUnits.ToString(), param.ValidityPeriodUnits.Value.ToString()));
            }
            #endregion

            var attr = new AttributePkcs(new DerObjectIdentifier(OidAttributes.EnrollmentNameValuePair), new DerSet(seqList.ToArray()));
            Import(attr.GetDerEncoded());
        }
        #endregion

        #region Properties
        /// <summary>
        /// This attribute MUST be used along with a Netscape KEYGEN request. It MUST define the type of certificate that the client needs.
        /// </summary>
        public CertTypeEnum? CertType
        {
            get;
            private set;
        }

        /// <summary>
        /// The request OIDs for use in the ExtendedKeyUsage extension, as specified in [RFC3280] section 4.2.1.13.
        /// </summary>
        public Oid[] CertificateUsage
        {
            get;
            private set;
        }

        /// <summary>
        /// The validity period of the request MUST be defined in two values: number and units.
        /// </summary>
        public ValidityPeriodEnum? ValidityPeriod
        {
            get;
            private set;
        }

        /// <summary>
        /// This value MUST define the number units used for the validity period. The units are defined in the ValidityPeriod attribute.
        /// </summary>
        public int? ValidityPeriodUnits
        {
            get;
            private set;
        }

        /// <summary>
        /// This value MUST define the exact request expiration time of the requested certificate in the format defined in section 3.3 of the [RFC2616].
        /// </summary>
        public DateTime? ExpirationDate
        {
            get;
            private set;
        }

        /// <summary>
        /// An Active Directory server FQDN.
        /// </summary>
        public string CDC
        {
            get;
            private set;
        }

        /// <summary>
        /// The requesting machine FQDN.
        /// </summary>
        public string RMD
        {
            get;
            private set;
        }

        /// <summary>
        /// This value MUST define the certificate template that was used by the client to construct the certificate request.
        /// </summary>
        public string CertificateTemplate
        {
            get;
            private set;
        }

        /// <summary>
        /// This value MUST contain a collection of one or more name-value pairs for the SubjectAltName extension. The format for the internal collection MUST be: "name1=value1&name2=value".
        /// </summary>
        public string SAN
        {
            get;
            private set;
        }

        /// <summary>
        /// This attribute MUST be passed only with a Netscape KEYGEN request format. The value of the attribute MUST be the challenge (password) string associated with the request.
        /// </summary>
        public string Challenge
        {
            get;
            private set;
        }

        /// <summary>
        /// The identity of the user whose information MUST be used to construct the subject information of an issued certificate. It is used along with a ROBO for a different subject.
        /// </summary>
        public string RequesterName
        {
            get;
            private set;
        }

        /// <summary>
        /// A valid RDN string SHOULD be used to pass subject names for a certificate request generated by using the KEYGEN format on a Netscape browser.
        /// </summary>
        public string Other
        {
            get;
            private set;
        }

        /// <summary>
        /// The client requests that the server publish the issued certificate to the Universal Naming Convention (UNC) path that is specified in the value for this attribute.
        /// </summary>
        public string CertFile
        {
            get;
            private set;
        }

        /// <summary>
        /// The request ID of the request that is pending the attestation Challenge Response.
        /// </summary>
        public int? RequestId
        {
            get;
            private set;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Resets properties on null
        /// </summary>
        protected override void OnReset()
        {
            CertType = null;
            CertificateUsage = null;
            ValidityPeriod = null;
            ValidityPeriodUnits = null;
            ExpirationDate = null;
            CDC = null;
            RMD = null;
            CertificateTemplate = null;
            SAN = null;
            Challenge = null;
            RequesterName = null;
            Other = null;
            CertFile = null;
            RequestId = null;
        }

        /// <summary>
        /// Sets attribute properties from raw data
        /// </summary>
        protected override void OnImport()
        {
            foreach (var item in Values)
            {
                var seq = (DerSequence)Asn1Object.FromByteArray(item.RawData);

                var name = ((DerBmpString)seq[0]).GetString();
                var value = ((DerBmpString)seq[1]).GetString();
                var paramName = (EnrollmentNameValuePairsEnum)Enum.Parse(typeof(EnrollmentNameValuePairsEnum), name, true);

                #region Sets propertie
                switch (paramName)
                {
                    case EnrollmentNameValuePairsEnum.CertType:
                        if (CertTypeEnum.Server.ToString() == value)
                        {
                            CertType = CertTypeEnum.Server;
                        }
                        break;
                    case EnrollmentNameValuePairsEnum.CertificateUsage:
                        var oidsStrig = value.Replace(" ", "");
                        var oidsArray = oidsStrig.Split(',');
                        var oids = oidsArray.Select(o => new Oid(o));
                        CertificateUsage = oids.ToArray();
                        break;
                    case EnrollmentNameValuePairsEnum.ValidityPeriod:
                        ValidityPeriod = (ValidityPeriodEnum)Enum.Parse(typeof(ValidityPeriodEnum), value, true);
                        break;
                    case EnrollmentNameValuePairsEnum.ValidityPeriodUnits:
                        ValidityPeriodUnits = int.Parse(value);
                        break;
                    case EnrollmentNameValuePairsEnum.ExpirationDate:
                        ExpirationDate = Convert.ToDateTime(value);
                        break;
                    case EnrollmentNameValuePairsEnum.CDC:
                        CDC = value;
                        break;
                    case EnrollmentNameValuePairsEnum.RMD:
                        RMD = value;
                        break;
                    case EnrollmentNameValuePairsEnum.CertificateTemplate:
                        CertificateTemplate = value;
                        break;
                    case EnrollmentNameValuePairsEnum.SAN:
                        SAN = value;
                        break;
                    case EnrollmentNameValuePairsEnum.Challenge:
                        Challenge = value;
                        break;
                    case EnrollmentNameValuePairsEnum.RequesterName:
                        RequesterName = value;
                        break;
                    case EnrollmentNameValuePairsEnum.Other:
                        Other = value;
                        break;
                    case EnrollmentNameValuePairsEnum.CertFile:
                        CertFile = value;
                        break;
                    case EnrollmentNameValuePairsEnum.RequestId:
                        RequestId = int.Parse(value);
                        break;
                    default:
                        throw new ArgumentException($"Not implemented, unsupported parameter name {paramName}");
                }
                #endregion
            }
        }
        #endregion
    }

}
