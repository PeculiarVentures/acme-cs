using System;
using System.Linq;
using System.Security.Cryptography;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;

namespace PeculiarVentures.ACME.Cryptography
{
    /// <summary>
    /// This class is the basic implementation for Attribute Values. Any class which implements some specific Attribute Value must be derived from this class.
    /// </summary>
    public class Attribute : IAsnEncodableData
    {
        #region Cache
        internal AttributePkcs _attribute;
        private AsnEncodedData[] _values;
        private Oid _type;
        #endregion

        #region Constructors
        public Attribute(byte[] rawData) => Import(rawData);
        public Attribute() => Reset();
        #endregion

        #region Properties
        /// <summary>
        /// Attribute identifier
        /// </summary>
        public Oid Type
        {
            get
            {
                if (_type == null)
                {
                    _type = new Oid(GetAttribute().AttrType.Id);
                }
                return _type;
            }
        }

        /// <summary>
        /// Array of attribute values
        /// </summary>
        public AsnEncodedData[] Values
        {
            get
            {
                if (_values == null)
                {
                    _values = GetAttribute().AttrValues.ToArray().Select(o => new AsnEncodedData(o.GetEncoded())).ToArray();
                }
                return _values;
            }
        }

        /// <summary>
        /// Attribute in the format raw data
        /// </summary>
        public byte[] RawData { get => Export(); }
        #endregion

        #region Methods
        /// <summary>
        /// Sets attribute properties from raw data
        /// </summary>
        /// <param name="rawData">Asn1Encodable</param>
        public virtual void Import(byte[] rawData)
        {
            #region Check arguments
            if (rawData is null)
            {
                throw new ArgumentNullException(nameof(rawData));
            }
            #endregion
            Reset();
            _attribute = AttributePkcs.GetInstance(Asn1Object.FromByteArray(rawData));
            OnImport();
        }

        protected virtual void OnImport() { }

        /// <summary>
        /// Returns the attribute in the format raw data
        /// </summary>
        /// <returns></returns>
        public byte[] Export()
        {
            return GetAttribute().GetEncoded();
        }

        /// <summary>
        /// Returns the attribute from cache
        /// </summary>
        /// <returns></returns>
        private AttributePkcs GetAttribute()
        {
            return _attribute ?? throw new NullReferenceException(nameof(_attribute));
        }

        /// <summary>
        /// Resets cache and properties on null
        /// </summary>
        public void Reset()
        {
            OnReset();
            _attribute = null;
            _values = null;
            _type = null;
        }

        protected virtual void OnReset() { }

        /// <summary>
        /// Creates sequence
        /// </summary>
        /// <param name="name">Name param</param>
        /// <param name="value">Value param</param>
        /// <returns></returns>
        protected DerSequence CreateSequence(string name, string value)
        {
            #region Check params
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(nameof(value));
            }
            #endregion

            var seq = new DerSequence(new DerBmpString(name), new DerBmpString(value));
            return seq;
        }
        #endregion
    }
}
