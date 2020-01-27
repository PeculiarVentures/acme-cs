using System;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PeculiarVentures.ACME.Web.Helpers;

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
        public AlgorithmsEnum Algorithm { get; set; }

        [JsonProperty("kty")]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonRequired]
        public KeyTypesEnum KeyType { get; set; }

        [JsonProperty("e")]
        public string Exponent { get; set; }

        [JsonProperty("n")]
        public string Modulus { get; set; }

        [JsonProperty("y")]
        public string Y { get; set; }

        [JsonProperty("x")]
        public string X { get; set; }

        [JsonProperty("crv")]
        [JsonConverter(typeof(StringEnumConverter))]
        public EllipticCurvesEnum EllipticCurve { get; set; }

        [JsonProperty("k")]
        public string KeyValue { get; set; }



        public JsonWebKey()
        {

        }



        public JsonWebKey(AsymmetricAlgorithm key)
        {
            if (key is RSA rsaKey)
            {
                SetRsaKey(rsaKey);
                return;
            }

            if (key is ECDsa ecdsaKey)
            {
                SetEcdsaKey(ecdsaKey);
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



        public RSA GetRsaKey()
        {
            var @params = new RSAParameters
            {
                Exponent = Base64Url.Decode(Exponent ?? throw new ArgumentNullException(nameof(Exponent))),
                Modulus = Base64Url.Decode(Modulus ?? throw new ArgumentNullException(nameof(Modulus))),
            };

            return RSA.Create(@params);
        }



        public void SetRsaKey(RSA key)
        {
            var @params = key.ExportParameters(false);


            KeyType = KeyTypesEnum.RSA;
            Exponent = Base64Url.Encode(@params.Exponent);
            Modulus = Base64Url.Encode(@params.Modulus);
        }



        public ECDsa GetEcdsaKey()
        {
            ECCurve curve = EllipticCurve switch
            {
                EllipticCurvesEnum.P256 => ECCurve.NamedCurves.nistP256,
                EllipticCurvesEnum.P384 => ECCurve.NamedCurves.nistP384,
                EllipticCurvesEnum.P512 => ECCurve.NamedCurves.nistP521,
                _ => throw new ArgumentException($"Could not create ECCurve based on EllipticCurve: {EllipticCurve}"),
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

            return ECDsa.Create(@params);
        }



        public void SetEcdsaKey(ECDsa key)
        {
            var @params = key.ExportParameters(false);

            KeyType = KeyTypesEnum.EC;
            X = Base64Url.Encode(@params.Q.X);
            Y = Base64Url.Encode(@params.Q.Y);
        }



        public HMAC GetHmacKey()
        {
            var hashName = Algorithm switch
            {
                AlgorithmsEnum.HS256 => "HMACSHA256",
                AlgorithmsEnum.HS384 => "HMACSHA384",
                AlgorithmsEnum.HS512 => "HMACSHA512",
                _ => throw new CryptographicException($"Unsupported hash algorithm '{Algorithm}'"),
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
    }
}
