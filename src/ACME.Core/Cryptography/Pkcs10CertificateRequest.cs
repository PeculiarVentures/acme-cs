using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Pkcs;
using System.Linq;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Security;

namespace PeculiarVentures.ACME.Cryptography
{
    /// <summary>
    /// 
    /// </summary>
    public class Pkcs10CertificateRequest : IAsnEncodableData
    {
        #region Cache
        private Pkcs10CertificationRequest _certReq;
        private Attribute[] _attributes;
        private X509ExtensionCollection _extensions;
        private bool _hasVersion;
        private int _version;
        private string _subject;
        private PublicKey _publicKey;
        private Oid _signatureAlgorithm;
        private bool _verify;
        #endregion

        #region Constructors
        public Pkcs10CertificateRequest(CertificateRequestParams param, RSA key) => Generate(param, key);
        public Pkcs10CertificateRequest(byte[] rawData) => Import(rawData);
        #endregion

        #region Properties
        /// <summary>
        /// Certificate request in raw date
        /// </summary>
        public byte[] RawData => Export();

        /// <summary>
        /// Version
        /// </summary>
        public int Version
        {
            get
            {
                if (!_hasVersion)
                {
                    _version = GetCertRequest().GetCertificationRequestInfo().Version.Value.IntValue;
                }
                return _version;
            }
        }

        /// <summary>
        /// Subject
        /// </summary>
        public string Subject
        {
            get
            {
                if (_subject == null)
                {
                    _subject = GetCertRequest().GetCertificationRequestInfo().Subject.ToString();
                }
                return _subject;
            }
        }

        /// <summary>
        /// Public key
        /// </summary>
        public PublicKey PublicKey
        {
            get
            {
                if (_publicKey == null)
                {
                    var SubjectPublicKeyInfo = GetCertRequest().GetCertificationRequestInfo().SubjectPublicKeyInfo;
                    var oid = new Oid(SubjectPublicKeyInfo.AlgorithmID.Algorithm.Id);
                    var keyValue = new AsnEncodedData(SubjectPublicKeyInfo.PublicKeyData.GetEncoded());
                    var parameters = new AsnEncodedData(SubjectPublicKeyInfo.AlgorithmID.Parameters.GetEncoded());
                    _publicKey = new PublicKey(oid, parameters, keyValue);
                }
                return _publicKey;
            }
        }

        /// <summary>
        /// Extensions
        /// </summary>
        public X509ExtensionCollection Extensions
        {
            get
            {
                if (_extensions == null)
                {
                    _extensions = GetExtensions();
                }
                return _extensions;
            }
        }

        /// <summary>
        /// Signature algorithm
        /// </summary>
        public Oid SignatureAlgorithm
        {
            get
            {
                if (_signatureAlgorithm == null)
                {
                    _signatureAlgorithm = new Oid(GetCertRequest().SignatureAlgorithm.Algorithm.Id);
                }
                return _signatureAlgorithm;
            }
        }

        /// <summary>
        /// Attributes
        /// </summary>
        public Attribute[] Attributes
        {
            get
            {
                if (_attributes == null)
                {
                    var attr = GetCertRequest().GetCertificationRequestInfo().Attributes;
                    if (attr == null)
                    {
                        return null;
                    }
                    _attributes = attr
                    .ToArray()
                    .Select(o => QualificationAttribute(o))
                    .ToArray();
                }
                return _attributes;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns extension by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public System.Security.Cryptography.X509Certificates.X509Extension GetExtension(string id)
        {
            if (id is null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            return GetExtension(new Oid(id));
        }

        /// <summary>
        /// Returns extension by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public System.Security.Cryptography.X509Certificates.X509Extension GetExtension(Oid id)
        {
            if (id is null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            foreach (var item in Extensions)
            {
                if (item.Oid.Value == id.Value)
                {
                    return item;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns all extensions
        /// </summary>
        /// <returns></returns>
        private X509ExtensionCollection GetExtensions()
        {
            var extensions = new X509ExtensionCollection();
            var attr = GetAttribute(OidAttributes.Extension);
            if (attr != null)
            {
                var asn1Object = Asn1Object.FromByteArray(attr.Values[0].RawData);
                var asn1Sequence = Asn1Sequence.GetInstance(asn1Object);
                foreach (Asn1Encodable item in asn1Sequence)
                {
                    var ext = QualificationExtension(item);
                    extensions.Add(ext);
                }
            }
            return extensions;
        }

        /// <summary>
        /// Qualification of attribute by type
        /// </summary>
        /// <param name="asn1Encodable"></param>
        /// <returns></returns>
        private Attribute QualificationAttribute(Asn1Encodable asn1Encodable)
        {
            var attr = new Attribute(asn1Encodable.GetEncoded());
            switch (attr.Type.Value)
            {
                case OidAttributes.СhallengePassword:
                    attr = new X509ChallengePasswordAttribute(asn1Encodable.GetEncoded());
                    break;
                case OidAttributes.EnrollmentNameValuePair:
                    attr = new X509EnrollmentNameValuePairAttribute(asn1Encodable.GetEncoded());
                    break;
                default:
                    break;
            }
            return attr;
        }

        /// <summary>
        /// Qualification of extension by type
        /// </summary>
        /// <param name="asn1Encodable"></param>
        /// <returns></returns>
        private System.Security.Cryptography.X509Certificates.X509Extension QualificationExtension(Asn1Encodable asn1Encodable)
        {
            Asn1Sequence s = Asn1Sequence.GetInstance(asn1Encodable.ToAsn1Object());

            if (s.Count < 2 || s.Count > 3)
                throw new ArgumentException("Bad sequence size: " + s.Count);

            DerObjectIdentifier oid = DerObjectIdentifier.GetInstance(s[0].ToAsn1Object());

            bool isCritical = s.Count == 3
            && DerBoolean.GetInstance(s[1].ToAsn1Object()).IsTrue;

            Asn1OctetString octets = Asn1OctetString.GetInstance(s[s.Count - 1].ToAsn1Object());

            var extension = new System.Security.Cryptography.X509Certificates.X509Extension(new Oid(oid.Id), octets.GetOctets(), isCritical);

            var value = Asn1Object.FromByteArray(octets.GetOctets());
            switch (extension.Oid.Value)
            {
                case OidExtensions.SubjectKeyIdentifier:
                    var spki = Asn1OctetString.GetInstance(value).GetOctets();
                    extension = new X509SubjectKeyIdentifierExtension(spki, isCritical);
                    break;
                case OidExtensions.ExtKeyUsage:
                    var bcEKU = ExtendedKeyUsage.GetInstance(value);
                    var collectionFlags = new OidCollection();
                    foreach (DerObjectIdentifier item in bcEKU.GetAllUsages())
                    {
                        collectionFlags.Add(new Oid(item.Id));
                    }
                    extension = new X509EnhancedKeyUsageExtension(collectionFlags, isCritical);
                    break;
                case OidExtensions.KeyUsage:
                    var bitString = DerBitString.GetInstance(value);
                    byte[] data = bitString.GetBytes();
                    var keyUsage = data.Length == 1
                        ? data[0] & 0xff
                        : (data[1] & 0xff) << 8 | (data[0] & 0xff);
                    extension = new X509KeyUsageExtension((X509KeyUsageFlags)keyUsage, isCritical);
                    break;
                case OidExtensions.BasicConstraints:
                    var bcBC = BasicConstraints.GetInstance(value);
                    extension = new X509BasicConstraintsExtension(bcBC.IsCA(), true, bcBC.PathLenConstraint.IntValue, isCritical);
                    break;
                case OidExtensions.TemplateV2:
                    extension = new X509TemplateExtensionV2(octets.GetOctets(), isCritical);
                    break;
                case OidExtensions.TemplateV1:
                    extension = new X509TemplateExtensionV1(octets.GetOctets(), isCritical);
                    break;
                default:
                    break;
            }
            return extension;
        }

        /// <summary>
        /// Parses certificate request from raw data and sets properties
        /// </summary>
        /// <param name="rawData"></param>
        public void Import(byte[] rawData)
        {
            if (rawData is null)
            {
                throw new ArgumentNullException(nameof(rawData));
            }

            try
            {
                Reset();
                _certReq = new Pkcs10CertificationRequest(rawData);
            }
            catch (Exception e)
            {
                throw new CryptographicException("Cannot initialize request from data raw. See internal exception for more details.", e);
            }
        }

        /// <summary>
        /// Returns signature verification result
        /// </summary>
        /// <returns></returns>
        public bool Verify()
        {
            if (!_verify)
            {
                _verify = GetCertRequest().Verify();
            }
            return _verify;
        }

        /// <summary>
        /// Returns certificate request from cashe
        /// </summary>
        /// <returns></returns>
        private Pkcs10CertificationRequest GetCertRequest()
        {
            if (_certReq == null)
            {
                throw new NullReferenceException(nameof(_certReq));
            }
            return _certReq;
        }

        /// <summary>
        /// Returns attribute by id
        /// </summary>
        /// <param name="id">Attribute identifier</param>
        /// <returns></returns>
        public Attribute GetAttribute(Oid id)
        {
            if (id is null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            foreach (var item in Attributes)
            {
                if (item.Type.Value == id.Value)
                {
                    return item;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns attribute by id
        /// </summary>
        /// <param name="id">Attribute identifier</param>
        /// <returns></returns>
        public Attribute GetAttribute(string id)
        {
            if (id is null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            return GetAttribute(new Oid(id));
        }

        /// <summary>
        /// Returns the certificate request in raw data
        /// </summary>
        /// <returns></returns>
        public byte[] Export()
        {
            return _certReq.GetEncoded();
        }

        /// <summary>
        /// Resets cache and all properties on null
        /// </summary>
        public void Reset()
        {
            _attributes = null;
            _certReq = null;
            _extensions = null;
            _hasVersion = false;
            _version = 0;
            _subject = null;
            _publicKey = null;
            _signatureAlgorithm = null;
            _verify = false;
        }

        /// <summary>
        /// Generates the certificate request by params
        /// </summary>
        /// <param name="param">Certificate request params</param>
        public void Generate(CertificateRequestParams param, RSA key)
        {
            //Requested Certificate Name
            X509Name name = string.IsNullOrEmpty(param.CommonName) ? X509Name.GetInstance(new DerSequence()) : new X509Name(param.CommonName);

            string signatureAlgorithm = param.SignatureAlgorithm.ToString();

            var keyPair = DotNetUtilities.GetRsaKeyPair(key);

            //Attributes
            Asn1Set attr = null;

            if (param.Attributes != null)
            {
                attr = new DerSet(param.Attributes.Select(o => Asn1Object.FromByteArray(o.Export())).ToArray());
            }

            //PKCS #10 Certificate Signing Request
            var bcCSR = new Pkcs10CertificationRequest(signatureAlgorithm, name, keyPair.Public, attr, keyPair.Private);
            Import(bcCSR.GetEncoded());
        }
        #endregion
    }
}
