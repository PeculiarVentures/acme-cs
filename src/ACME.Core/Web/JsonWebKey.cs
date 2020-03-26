using System;
using System.Collections;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PeculiarVentures.ACME.Helpers;
using PeculiarVentures.ACME.Protocol;

namespace PeculiarVentures.ACME.Web
{
    /// <summary>
    /// JSON Web Key (JWK)
    /// </summary>
    /// <see cref="https://www.rfc-editor.org/rfc/rfc7517.html"/>
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class JsonWebKey
    {
        [JsonProperty("alg")]
        [JsonConverter(typeof(StringEnumConverter))]
        public AlgorithmsEnum? Algorithm { get; set; }

        [JsonProperty("kty")]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonRequired]
        public KeyTypesEnum? KeyType { get; set; }

        [JsonProperty("e")]
        public string Exponent { get; set; }

        [JsonProperty("n")]
        public string Modulus { get; set; }

        [JsonProperty("d")]
        public string D { get; set; }

        [JsonProperty("dp")]
        public string DP { get; set; }

        [JsonProperty("dq")]
        public string DQ { get; set; }

        [JsonProperty("p")]
        public string P { get; set; }

        [JsonProperty("q")]
        public string Q { get; set; }

        [JsonProperty("qi")]
        public string QI { get; set; }

        [JsonProperty("y")]
        public string Y { get; set; }

        [JsonProperty("x")]
        public string X { get; set; }

        [JsonProperty("crv")]
        [JsonConverter(typeof(StringEnumConverter))]
        public EllipticCurvesEnum? EllipticCurve { get; set; }

        [JsonProperty("k")]
        public string KeyValue { get; set; }



        public JsonWebKey()
        {

        }

        /// <summary>
        /// Gets thumbprint of JWK
        /// </summary>
        /// <param name="alg">Default SHA256</param>
        /// <returns></returns>
        public byte[] GetThumbprint(AlgorithmsEnum alg = AlgorithmsEnum.SHA256)
        {
            var listKeys = new SortedList();
            if (KeyType != null)
            {
                listKeys.Add("kty", KeyType.Value.ToString());
            }
            if (Exponent != null)
            {
                listKeys.Add("e", Exponent);
            }
            if (Modulus != null)
            {
                listKeys.Add("n", Modulus);
            }
            if (Y != null)
            {
                listKeys.Add("y", Y);
            }
            if (X != null)
            {
                listKeys.Add("x", X);
            }
            if (EllipticCurve != null)
            {
                listKeys.Add("crv", EllipticCurve.Value.ToString());
            }
            if (KeyValue != null)
            {
                listKeys.Add("k", KeyValue);
            }
            var json = JsonConvert.SerializeObject(listKeys);

            byte[] thumbprint;
            switch (alg)
            {
                case AlgorithmsEnum.SHA256:
                    SHA256 mySHA256 = SHA256.Create();
                    thumbprint = mySHA256.ComputeHash(Encoding.UTF8.GetBytes(json));
                    break;
                case AlgorithmsEnum.SHA1:
                    SHA1 mySHA1 = SHA1.Create();
                    thumbprint = mySHA1.ComputeHash(Encoding.UTF8.GetBytes(json));
                    break;
                default:
                    throw new ArgumentException($"Unsupported algorithm: {nameof(alg)}");
            }
            return thumbprint;
        }

        public JsonWebKey(AsymmetricAlgorithm key)
        {
            if (key is RSA rsaKey)
            {
                SetRsaKey(rsaKey, false);
                return;
            }

            if (key is ECDsa ecdsaKey)
            {
                SetEcdsaKey(ecdsaKey, false);
                return;
            }

            throw new ArgumentException($"Unsupported key type: {key.GetType()}");
        }

        public JsonWebKey(AsymmetricAlgorithm key, bool exportPrivate)
        {
            if (key is RSA rsaKey)
            {
                SetRsaKey(rsaKey, exportPrivate);
                return;
            }

            if (key is ECDsa ecdsaKey)
            {
                SetEcdsaKey(ecdsaKey, exportPrivate);
                return;
            }

            throw new ArgumentException($"Unsupported key type: {key.GetType()}");
        }


        public JsonWebKey(KeyedHashAlgorithm key)
        {
            if (key is HMAC hmacKey)
            {
                SetHmacKey(hmacKey);
                return;
            }

            throw new ArgumentException($"Unsupported key type: {key.GetType()}");
        }

        public AsymmetricAlgorithm GetAsymmetricAlgorithm()
        {
            if (KeyType == KeyTypesEnum.EC)
            {
                return GetEcdsaKey();
            }
            else if (KeyType == KeyTypesEnum.RSA)
            {
                return GetRsaKey();
            }
            throw new Exception("Unsupported key type");
        }

        public RSA GetRsaKey()
        {
            var @params = new RSAParameters
            {
                // public params
                Exponent = Base64Url.Decode(Exponent ?? throw new ArgumentNullException(nameof(Exponent))),
                Modulus = Base64Url.Decode(Modulus ?? throw new ArgumentNullException(nameof(Modulus))),
            };

            if (D != null)
            {
                // private params
                @params.D = Base64Url.Decode(D) ?? throw new ArgumentNullException(nameof(D));
                @params.DP = Base64Url.Decode(DP) ?? throw new ArgumentNullException(nameof(DP));
                @params.DQ = Base64Url.Decode(DQ) ?? throw new ArgumentNullException(nameof(DQ));
                @params.Q = Base64Url.Decode(Q) ?? throw new ArgumentNullException(nameof(Q));
                @params.P = Base64Url.Decode(P) ?? throw new ArgumentNullException(nameof(P));
                @params.InverseQ = Base64Url.Decode(QI) ?? throw new ArgumentNullException(nameof(QI));
            }

            var key = RSA.Create();
            key.ImportParameters(@params);

            return key;
        }



        public void SetRsaKey(RSA key, bool includePrivateParameters)
        {
            var @params = key.ExportParameters(includePrivateParameters);

            KeyType = KeyTypesEnum.RSA;
            Exponent = Base64Url.Encode(@params.Exponent);
            Modulus = Base64Url.Encode(@params.Modulus);

            if (includePrivateParameters)
            {
                D = Base64Url.Encode(@params.D);
                DP = Base64Url.Encode(@params.DP);
                DQ = Base64Url.Encode(@params.DQ);
                P = Base64Url.Encode(@params.P);
                Q = Base64Url.Encode(@params.Q);
                QI = Base64Url.Encode(@params.InverseQ);
            }
        }



        public ECDsa GetEcdsaKey()
        {
            ECCurve curve;
            switch (EllipticCurve)
            {
                case EllipticCurvesEnum.P256:
                    curve = ECCurve.NamedCurves.nistP256;
                    break;
                case EllipticCurvesEnum.P384:
                    curve = ECCurve.NamedCurves.nistP384;
                    break;
                case EllipticCurvesEnum.P512:
                    curve = ECCurve.NamedCurves.nistP521;
                    break;
                default:
                    throw new ArgumentException($"Could not create ECCurve based on EllipticCurve: {EllipticCurve}");
            };
            var @params = new ECParameters
            {
                Curve = curve,
                Q = new ECPoint
                {
                    X = Base64Url.Decode(X ?? throw new ArgumentNullException(nameof(X))),
                    Y = Base64Url.Decode(Y ?? throw new ArgumentNullException(nameof(Y))),
                },
            };

            if (D != null)
            {
                @params.D = Base64Url.Decode(D ?? throw new ArgumentNullException(nameof(D)));
            }

            return ECDsa.Create(@params);
        }

        public AsymmetricAlgorithm GetPublicKey()
        {
            if (KeyType == KeyTypesEnum.EC)
            {
                return GetEcdsaKey();
            }
            else if (KeyType == KeyTypesEnum.RSA)
            {
                return GetRsaKey();
            }
            throw new AcmeException(ErrorType.BadPublicKey);
        }

        public void SetEcdsaKey(ECDsa key, bool includePrivateParameters)
        {
            var @params = key.ExportParameters(includePrivateParameters);

            KeyType = KeyTypesEnum.EC;
            X = Base64Url.Encode(@params.Q.X);
            Y = Base64Url.Encode(@params.Q.Y);

            // TODO Set Curve
            // EllipticCurve = ?

            if (includePrivateParameters)
            {
                D = Base64Url.Encode(@params.D);
            }
        }

        public HMAC GetHmacKey()
        {
            string hashName;
            switch (Algorithm)
            {
                case AlgorithmsEnum.HS256:
                    hashName = "HMACSHA256";
                    break;
                case AlgorithmsEnum.HS384:
                    hashName = "HMACSHA384";
                    break;
                case AlgorithmsEnum.HS512:
                    hashName = "HMACSHA512";
                    break;
                default:
                    throw new CryptographicException($"Unsupported hash algorithm '{Algorithm}'");
            };
            HMAC hmac = HMAC.Create(hashName);
            hmac.Key = Base64Url.Decode(KeyValue);

            return hmac;
        }



        public void SetHmacKey(HMAC key)
        {
            KeyType = KeyTypesEnum.OctetSequence;
            KeyValue = Base64Url.Encode(key.Key);
        }

        public override bool Equals(object obj)
        {
            if (obj is JsonWebKey jwk)
            {
                return KeyType == jwk.KeyType
                    && EllipticCurve == jwk.EllipticCurve
                    && (KeyValue == null || KeyValue.SequenceEqual(jwk.KeyValue))
                    && (Modulus == null || Modulus.SequenceEqual(jwk.Modulus))
                    && (Exponent == null || Exponent.SequenceEqual(jwk.Exponent))
                    && (X == null || X.SequenceEqual(jwk.X))
                    && (Y == null || Y.SequenceEqual(jwk.Y));
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
