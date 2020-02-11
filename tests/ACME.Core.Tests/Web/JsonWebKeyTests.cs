using System;
using System.Security.Cryptography;
using Xunit;

namespace PeculiarVentures.ACME.Web
{
    public class JsonWebKeyTests
    {
        [Fact]
        public void InitWithParametersAndGetRsaKey()
        {
            var jwk = new JsonWebKey
            {
                Modulus = "sqUBnlog3WE1hyEG9BSRf8j4BzN67ProzJDQJRSsFUbfT8bClzzLmNZ5U1UiSwlD1QHaVizUlxIvyN_PxvALvszp09X_nthutTB62ytmASu6v7HoyDeZmDJyGZO27lchirOCXMTR3gAr0iXLHpi7_Asxa6nZSlfXjWbH-cowozr2W_SJIq18cMR-2BRzEn1C3T98gAMKqbwXBgc8GXX57VLq3oB9BRHUMBJtAetWQQmZLOjvI9Es_AU_XESiCIhRVahyJNQFYkKmM2J0pxNe9tY6LDWK_mumu_xUqlFq8iMMkDRoc1wjuS-x1IXj9rjSXeDEtscCdcU2xs6Rn-TeYw",
                Exponent = "AQAB",
            };
            var key = jwk.GetRsaKey();

            Assert.True(key is RSA);
        }

        [Fact]
        public void InitWithParametersAndGetEcdsaKey()
        {
            var jwk = new JsonWebKey
            {
                X = "mwauoXmb0rYAO0JY2q6X1WHbU-gVnZcjcpNh-TMUz-0",
                Y = "yVXrJ6YywtiKdUSOohUT52THlbt8AYtWiKWX-6WXb50",
                EllipticCurve = EllipticCurvesEnum.P256,
            };
            var key = jwk.GetEcdsaKey();

            Assert.True(key is ECDsa);
        }

        [Fact]
        public void InitWithParametersAndGetHmacKey()
        {
            var jwk = new JsonWebKey
            {
                Algorithm = AlgorithmsEnum.HS256,
                KeyValue = "9RBR6ODDPlyBTxQlwdFvKRqqtFuMokffcn9k0TpzVvk",
            };
            var key = jwk.GetHmacKey();

            Assert.True(key is HMAC);
        }

        [Fact]
        public void GetThumbprint()
        {
            var jwk = new JsonWebKey
            {
                Exponent = "AQAB",
                KeyType = KeyTypesEnum.RSA,
                Modulus = "uHbYmn_4HqOwfI_yjqR9Zkf8fUnAQKLdAT60CED1d-UvRlpTkuJZSBtKfQn_fFAC23tk_zMhYR09rIapAh9DPAf_Bdd1APKEDg6xnPquq-iQdYO5KCBO4UDIaHapyjBPqRCRHuHwG-MPKvCBGeEIP1A0Nj3zmJPylP0VSZH5EpEf3ILqZmLFeYfOPbRLgTtJe4L61hE9UTf2JM8zpLkK6Y-sDFSxgHciNCt9RjmhTgc7oHxV06QADx9B8F02odvYPZXeGw_LGgDUncg_gdFPKWoDImDFvKCd5QKdQVbjn_W1LBwgFVaten-JX7JX00NvFDipPjZLNPie9c-qu9Rgqw",
                Algorithm = AlgorithmsEnum.SHA256,
                //KeyValue = "9RBR6ODDPlyBTxQlwdFvKRqqtFuMokffcn9k0TpzVvk",
            };

            var thumbprint = "r/8kxUe4SmBoIAIL4Br3+Edtn3yDnAPo7zlgK3Z4p/I=";

            Assert.Equal(thumbprint, Convert.ToBase64String(jwk.GetThumbprint(AlgorithmsEnum.SHA256)));
        }
    }
}
