using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using PeculiarVentures.ACME.Helpers;
using PeculiarVentures.ACME.Web.Jwa;
using PeculiarVentures.ACME.Web.Jwe;
using PeculiarVentures.ACME.Web.Jwe.Json;
using PeculiarVentures.ACME.Web.Jwe.Crypto;

namespace PeculiarVentures.ACME.Web
{
    /// <summary>
    /// Json Web Encrypton (JWE)
    /// </summary>
    /// <see cref="https://tools.ietf.org/html/rfc7516#section-7.2.1"/>
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class JsonWebEncyption
    {
        private static Dictionary<Encryptions, IJweAlgorithm> _encAlgorithms = new Dictionary<Encryptions, IJweAlgorithm>
            {
                { Encryptions.A128CBC_HS256, new AesCbcHmacEncryption("HMACSHA256", 256) },
                { Encryptions.A192CBC_HS384, new AesCbcHmacEncryption("HMACSHA384", 384) },
                { Encryptions.A256CBC_HS512, new AesCbcHmacEncryption("HMACSHA512", 512) },

                { Encryptions.A128GCM, new AesGcmEncryption(128) },
                { Encryptions.A192GCM, new AesGcmEncryption(192) },
                { Encryptions.A256GCM, new AesGcmEncryption(256) },
            };

        private static Dictionary<Algorithms, IKeyManagement> _keyAlgorithms = new Dictionary<Algorithms, IKeyManagement>
            {
                { Algorithms.RSA_OAEP, new RsaKeyManagement(RSAEncryptionPadding.OaepSHA1) },
                { Algorithms.RSA_OAEP_256, new RsaKeyManagement(RSAEncryptionPadding.OaepSHA256) },
                { Algorithms.RSA1_5, new RsaKeyManagement(RSAEncryptionPadding.Pkcs1) },
                { Algorithms.A128KW, new AesKeyWrapManagement(128) },
                { Algorithms.A192KW, new AesKeyWrapManagement(192) },
                { Algorithms.A256KW, new AesKeyWrapManagement(256) },
            };

        private string _message { get; }

        private JsonHeader _protectedHeader { get; }

        private string _aad { get; }

        private RecipientCollection _recipients { get; set; } = new RecipientCollection();

        private JsonGeneral _result { get; set; }

        public JsonWebEncyption()
        {
        }

        public JsonWebEncyption(string message, JsonHeader protectedHeader, string aad = null)
        {
            _message = Base64Url.Encode(message);
            _protectedHeader = protectedHeader;
            _aad = aad != null ? Base64Url.Encode(aad) : null;
        }

        public void Recipient(AsymmetricAlgorithm key, JsonHeader header)
        {
            _recipients.Add(new Recipient
            {
                EncryptionKey = key,
                Header = header,
            });
        }

        public void Recipient(KeyedHashAlgorithm key, JsonHeader header)
        {
            _recipients.Add(new Recipient
            {
                EncryptionKey = key,
                Header = header,
            });
        }

        public void Encrypt()
        {
            var enc = _encAlgorithms[(Encryptions)_protectedHeader.EncryptionAlgorithm];

            if (enc == null)
            {
                throw new Exception("Unsupported JWE encryption requested");
            }

            var recipients = new JsonRecipientCollection();
            var cek = Utils.Random(enc.KeySize);
            var iv = Utils.Random(128);

            foreach (var recipient in _recipients)
            {
                var alg = _keyAlgorithms[(Algorithms)recipient.Header.Algorithm];

                if (alg == null)
                {
                    throw new Exception("Unsupported JWA algorithm requested");
                }

                var encryptedCek = alg.WrapKey(cek, recipient.EncryptionKey);

                recipients.Add(new JsonRecipient
                {
                    Header = recipient.Header,
                    EncryptedKey = Base64Url.Encode(encryptedCek),
                });
            }

            var encParts = enc.Encrypt(Base64Url.Decode(_message), cek, iv, _aad != null ? Base64Url.Decode(_aad) : new byte[0]);

            _result = new JsonGeneral
            {
                ProtectedHeader = Base64Url.Encode(JsonConvert.SerializeObject(_protectedHeader)),
                Recipients = recipients,
                AdditionalAuthenticatedData = _aad,
                InitializationVector = Base64Url.Encode(iv),
                CipherText = Base64Url.Encode(encParts[0]),
                AuthenticationTag = Base64Url.Encode(encParts[1]),
            };
        }

        public void Encrypt(AsymmetricAlgorithm key)
        {
            Encrypt((object)key);
        }

        public void Encrypt(KeyedHashAlgorithm key)
        {
            Encrypt((object)key);
        }

        private void Encrypt(object key)
        {
            var alg = _keyAlgorithms[(Algorithms)_protectedHeader.Algorithm];
            var enc = _encAlgorithms[(Encryptions)_protectedHeader.EncryptionAlgorithm];

            if (alg == null)
            {
                throw new Exception("Unsupported JWA algorithm requested");
            }

            if (enc == null)
            {
                throw new Exception("Unsupported JWE encryption requested");
            }

            var cek = Utils.Random(enc.KeySize);
            var iv = Utils.Random(128);
            var encryptedCek = alg.WrapKey(cek, key);
            var encParts = enc.Encrypt(Base64Url.Decode(_message), cek, iv, _aad != null ? Base64Url.Decode(_aad) : new byte[0]);

            _result = new JsonGeneral
            {
                ProtectedHeader = Base64Url.Encode(JsonConvert.SerializeObject(_protectedHeader)),
                EncryptedKey = Base64Url.Encode(encryptedCek),
                AdditionalAuthenticatedData = _aad,
                InitializationVector = Base64Url.Encode(iv),
                CipherText = Base64Url.Encode(encParts[0]),
                AuthenticationTag = Base64Url.Encode(encParts[1]),
            };
        }

        public static byte[] Decrypt(string jwe, object key)
        {
            return Decrypt(Import(jwe), key);
        }

        public static byte[] Decrypt(byte[] jwe, object key)
        {
            return Decrypt(Import(jwe), key);
        }

        public static byte[] Decrypt(JsonGeneral jwe, object key)
        {
            var protectedHeader = JsonConvert.DeserializeObject<JsonHeader>(Encoding.UTF8.GetString(Base64Url.Decode(jwe.ProtectedHeader)));
            var enc = _encAlgorithms[(Encryptions)protectedHeader.EncryptionAlgorithm];

            if (enc == null)
            {
                throw new Exception("Unsupported JWE encryption requested");
            }

            byte[] cek = null;

            if (jwe.EncryptedKey != null)
            {
                var alg = _keyAlgorithms[(Algorithms)protectedHeader.Algorithm];

                if (alg == null)
                {
                    throw new Exception("Unsupported JWA algorithm requested");
                }

                var encryptedKey = Base64Url.Decode(jwe.EncryptedKey);
                cek = alg.UnwrapKey(encryptedKey, key);
            }
            else if (jwe.Recipients != null)
            {
                foreach (var recipient in jwe.Recipients)
                {
                    var alg = _keyAlgorithms[(Algorithms)recipient.Header.Algorithm];

                    if (alg == null)
                    {
                        throw new Exception("Unsupported JWA algorithm requested");
                    }

                    var encryptedKey = Base64Url.Decode(recipient.EncryptedKey);

                    try
                    {
                        cek = alg.UnwrapKey(encryptedKey, key);

                        break;
                    }
                    catch (Exception) { }
                }
            }

            var iv = Base64Url.Decode(jwe.InitializationVector);
            var aad = jwe.AdditionalAuthenticatedData != null ? Base64Url.Decode(jwe.AdditionalAuthenticatedData) : new byte[0];
            var cipherText = Base64Url.Decode(jwe.CipherText);
            var tag = Base64Url.Decode(jwe.AuthenticationTag);

            return enc.Decrypt(cipherText, cek, iv, aad, tag);
        }

        public static JsonGeneral Import(byte[] jwe)
        {
            return Import(Encoding.UTF8.GetString(jwe));
        }

        public static JsonGeneral Import(string jwe)
        {
            JsonGeneral result;

            try
            {
                result = JsonConvert.DeserializeObject<JsonGeneral>(jwe);
            }
            catch (Exception)
            {
                string[] parts = jwe.Split('.');

                result = new JsonGeneral
                {
                    ProtectedHeader = parts[0],
                    EncryptedKey = parts[1],
                    InitializationVector = parts[2],
                    CipherText = parts[3],
                    AuthenticationTag = parts[4],
                };
            }

            if (result == null)
            {
                throw new Exception("Wrong JWE format");
            }

            return result;
        }

        public byte[] Export(ExportType type)
        {
            if (_result == null)
            {
                throw new Exception("JWE doesn't encrypted");
            }

            switch (type)
            {
                case ExportType.General:
                    return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_result));

                case ExportType.Compact:
                    if (_result.Recipients != null || _result.AdditionalAuthenticatedData != null)
                    {
                        throw new Exception("JWE Compact Serialization doesn't support multiple recipients, JWE unprotected headers or AAD");
                    }

                    return Encoding.UTF8.GetBytes($"{_result.ProtectedHeader}.{_result.EncryptedKey}.{_result.InitializationVector}.{_result.CipherText}.{_result.AuthenticationTag}");

                default:
                    throw new Exception($"Unsupported {nameof(ExportType)}: {type}");
            }
        }
    }
}
