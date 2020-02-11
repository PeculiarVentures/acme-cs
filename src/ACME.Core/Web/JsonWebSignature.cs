using System;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using PeculiarVentures.ACME.Helpers;

namespace PeculiarVentures.ACME.Web
{
    /// <summary>
    /// JSON Web Signature (JWS)
    /// </summary>
    /// <see cref="https://www.rfc-editor.org/rfc/rfc7515.html"/>
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class JsonWebSignature
    {
        [JsonProperty("protected")]
        public string Protected { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }

        [JsonProperty("payload")]
        public string Payload { get; set; }

        [JsonIgnore]
        public bool IsPayloadEmpty => string.IsNullOrEmpty(Payload);

        [JsonIgnore]
        public bool IsPayloadEmptyObject =>
            !string.IsNullOrEmpty(Payload)
            && Encoding.UTF8.GetString(Base64Url.Decode(Payload)) == "{}";



        public JsonWebSignatureProtected GetProtected()
        {
            byte[] decodedAsBytes = Base64Url.Decode(Protected ?? throw new ArgumentNullException(nameof(Protected)));
            string decodedAsString = Encoding.UTF8.GetString(decodedAsBytes);

            return JsonConvert.DeserializeObject<JsonWebSignatureProtected>(decodedAsString);
        }



        public void SetProtected(JsonWebSignatureProtected @protected)
        {
            Protected = Base64Url.Encode(JsonConvert.SerializeObject(@protected));
        }



        public object GetPayload(Type type)
        {
            byte[] decodedAsBytes = Base64Url.Decode(Payload ?? throw new ArgumentNullException(nameof(Payload)));
            string decodedAsString = Encoding.UTF8.GetString(decodedAsBytes);

            return JsonConvert.DeserializeObject(decodedAsString, type);
        }


        public T GetPayload<T>()
        {
            return (T)GetPayload(typeof(T));
        }



        public void SetPayload(object payload)
        {
            Payload = Base64Url.Encode(JsonConvert.SerializeObject(payload));
        }



        public byte[] GetSignature()
        {
            return Base64Url.Decode(Signature ?? throw new ArgumentNullException(nameof(Signature)));
        }



        public void SetSignature(byte[] signature)
        {
            Signature = Base64Url.Encode(signature);
        }



        public bool TryGetPayload(Type type, out object payload)
        {
            try
            {
                payload = GetPayload(type);
                return true;
            }
            catch
            {
                payload = null;
                return false;
            }
        }



        public bool TryGetPayload<T>(out T payload)
        {
            try
            {
                payload = (T)GetPayload(typeof(T));
                return true;
            }
            catch
            {
                payload = default;
                return false;
            }
        }



        public bool Verify()
        {
            JsonWebSignatureProtected @protected = GetProtected();

            if (@protected.Key.KeyType == KeyTypesEnum.RSA)
            {
                RSA rsaKey = @protected.Key.GetRsaKey();
                return Verify(rsaKey);
            }

            if (@protected.Key.KeyType == KeyTypesEnum.EC)
            {
                ECDsa ecdsaKey = @protected.Key.GetEcdsaKey();
                return Verify(ecdsaKey);
            }

            throw new ArgumentException($"Unsupported key type: {@protected.Key.KeyType}");
        }



        public bool Verify(AsymmetricAlgorithm key)
        {
            byte[] signature = GetSignature();
            byte[] data = ToByteSign();
            HashAlgorithmName hashAlgorithm = GetHashAlgorithmName();

            if (key is RSA rsaKey)
            {
                RSASignaturePadding signaturePadding = GetRSASignaturePadding();
                return rsaKey.VerifyData(data, signature, hashAlgorithm, signaturePadding);
            }

            if (key is ECDsa ecdsaKey)
            {
                return ecdsaKey.VerifyData(data, signature, hashAlgorithm);
            }

            throw new ArgumentException($"Unsupported key type: {key.GetType()}");
        }



        public bool Verify(SymmetricAlgorithm key)
        {
            throw new NotImplementedException();
        }



        public void Sign(AsymmetricAlgorithm key)
        {
            byte[] data = ToByteSign();
            HashAlgorithmName hashAlgorithm = GetHashAlgorithmName();

            if (key is RSA rsaKey)
            {
                RSASignaturePadding signaturePadding = GetRSASignaturePadding();
                byte[] signedData = rsaKey.SignData(data, hashAlgorithm, signaturePadding);

                Signature = Base64Url.Encode(signedData);
                return;
            }

            if (key is ECDsa ecdsaKey)
            {
                byte[] signedData = ecdsaKey.SignData(data, hashAlgorithm);

                Signature = Base64Url.Encode(signedData);
                return;
            }

            throw new ArgumentException($"Unsupported key type: {key.GetType()}");
        }



        public void Sign(HMAC key)
        {
            byte[] data = ToByteSign();
            byte[] signedData = key.ComputeHash(data);

            Signature = Base64Url.Encode(signedData);
        }



        public void Sign(SymmetricAlgorithm key)
        {
            throw new NotImplementedException();
        }



        private HashAlgorithmName GetHashAlgorithmName()
        {
            JsonWebSignatureProtected @protected = GetProtected();

            switch (@protected.Algorithm)
            {
                case AlgorithmsEnum.RS1:
                    return HashAlgorithmName.SHA1;

                case AlgorithmsEnum.RS256:
                case AlgorithmsEnum.ES256:
                case AlgorithmsEnum.PS256:
                    return HashAlgorithmName.SHA256;

                case AlgorithmsEnum.RS384:
                case AlgorithmsEnum.ES384:
                case AlgorithmsEnum.PS384:
                    return HashAlgorithmName.SHA384;

                case AlgorithmsEnum.RS512:
                case AlgorithmsEnum.ES512:
                case AlgorithmsEnum.PS512:
                    return HashAlgorithmName.SHA512;

                default:
                    throw new CryptographicException($"Unsupported hash algorithm: {@protected.Algorithm}");
            }
        }



        private RSASignaturePadding GetRSASignaturePadding()
        {
            JsonWebSignatureProtected @protected = GetProtected();

            switch (@protected.Algorithm)
            {
                case AlgorithmsEnum.PS256:
                case AlgorithmsEnum.PS384:
                case AlgorithmsEnum.PS512:
                    return RSASignaturePadding.Pss;

                default:
                    return RSASignaturePadding.Pkcs1;
            }
        }



        public string ToStringSign()
        {
            return $"{Protected}.{Payload}";
        }



        public byte[] ToByteSign()
        {
            return Encoding.ASCII.GetBytes(ToStringSign());
        }



        public override string ToString()
        {
            return $"{Protected}.{Payload}.{Signature}";
        }
    }
}
