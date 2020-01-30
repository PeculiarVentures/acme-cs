using System;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;

namespace PeculiarVentures.ACME.Cryptography
{
    /// <summary>
    /// ChallengePassword attribute may be included in a PKCS#10 CertificateRequest to specify a password by which an entity may request certificate revocation
    /// </summary>
    /// <see cref="http://javadoc.iaik.tugraz.at/iaik_jce/current/iaik/pkcs/pkcs9/ChallengePassword.html"/>
    public class X509ChallengePasswordAttribute : Attribute
    {
        #region Constructors
        public X509ChallengePasswordAttribute(byte[] rawData) : base(rawData) => Import(rawData);

        public X509ChallengePasswordAttribute(string password) : base()
        {
            Password = password ?? throw new ArgumentNullException(nameof(password));
            var attr = new AttributePkcs(new DerObjectIdentifier(OidAttributes.СhallengePassword), new DerSet(new DerUtf8String(Password)));
            Import(attr.GetDerEncoded());
        }
        #endregion

        #region Properties
        /// <summary>
        /// The challenge password as ASN.1 string
        /// </summary>
        public string Password
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
            Password = null;
        }

        /// <summary>
        /// Sets attribute params from raw data
        /// </summary>
        protected override void OnImport()
        {
            var rawData = Values[0].RawData;
            var value = Asn1Object.FromByteArray(rawData);
            if(value is DerStringBase)
            {
                Password = ((DerStringBase)value).GetString();
            } else
            {
                throw new ArgumentException("The value is not in the correct format");
            }
        }
        #endregion

    }

}