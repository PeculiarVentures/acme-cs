using Xunit;
using System.Security.Cryptography;
using System;

namespace PeculiarVentures.ACME.Web
{
    public class JsonWebSignatureTests
    {

        [Fact]
        public void SignWithRSA()
        {
            JsonWebSignature jws = new JsonWebSignature();

            var @protected = new JsonWebSignatureProtected
            {
                Algorithm = AlgorithmsEnum.RS256,
                Key = new JsonWebKey
                {
                    Algorithm = AlgorithmsEnum.RS256,
                    KeyType = KeyTypesEnum.RSA,
                }
            };

            jws.SetProtected(@protected);

            AsymmetricAlgorithm key = AsymmetricAlgorithm.Create("RSA");

            jws.Sign(key);

            Assert.True(jws.Verify(key));

            Console.WriteLine("JWS with RSA");
            Console.WriteLine(jws.ToString());
        }

        [Fact]
        public void SignWithEC()
        {
            JsonWebSignature jws = new JsonWebSignature();

            var @protected = new JsonWebSignatureProtected
            {
                Algorithm = AlgorithmsEnum.ES256,
                Key = new JsonWebKey
                {
                    Algorithm = AlgorithmsEnum.ES256,
                    KeyType = KeyTypesEnum.EC,
                }
            };

            jws.SetProtected(@protected);

            AsymmetricAlgorithm key = AsymmetricAlgorithm.Create("ECDsa");

            jws.Sign(key);

            Assert.True(jws.Verify(key));

            Console.WriteLine("JWS with ECDsa");
            Console.WriteLine(jws.ToString());
        }

        [Fact]
        public void VerifyWithRSA()
        {
            JsonWebSignature jws = new JsonWebSignature
            {
                Protected = "eyJhbGciOiJSUzI1NiIsImp3ayI6eyJrdHkiOiJSU0EiLCJhbGciOiJSUzI1NiIsImtleV9vcHMiOlsidmVyaWZ5Il0sImV4dCI6dHJ1ZSwibiI6InNxVUJubG9nM1dFMWh5RUc5QlNSZjhqNEJ6TjY3UHJvekpEUUpSU3NGVWJmVDhiQ2x6ekxtTlo1VTFVaVN3bEQxUUhhVml6VWx4SXZ5Tl9QeHZBTHZzenAwOVhfbnRodXRUQjYyeXRtQVN1NnY3SG95RGVabURKeUdaTzI3bGNoaXJPQ1hNVFIzZ0FyMGlYTEhwaTdfQXN4YTZuWlNsZlhqV2JILWNvd296cjJXX1NKSXExOGNNUi0yQlJ6RW4xQzNUOThnQU1LcWJ3WEJnYzhHWFg1N1ZMcTNvQjlCUkhVTUJKdEFldFdRUW1aTE9qdkk5RXNfQVVfWEVTaUNJaFJWYWh5Sk5RRllrS21NMkowcHhOZTl0WTZMRFdLX211bXVfeFVxbEZxOGlNTWtEUm9jMXdqdVMteDFJWGo5cmpTWGVERXRzY0NkY1UyeHM2Um4tVGVZdyIsImUiOiJBUUFCIn0sInVybCI6Imh0dHBzOi8vYWNtZS50ZXN0LmNvbS9wYXRoIiwibm9uY2UiOiJkR1Z6ZENCdFpYTnpZV2RsIn0",
                Payload = "",
                Signature = "GoZY7BqVtUcrhrMaX1ETzs_NOu6p08sDIl4ZYpFBa-REeUYxuHimJRCIx2Z7p-59UYO9LmRP15VFT7jdku6qIURhJADugd57WQ53WvmburXi3fXGW2Gmx6qfPb9mvPs_-A9U9W0I9DPa4w6Qee408nFO8_XzyYgs5v3hHqSef1n-0lFtO3RiAOFynWf12Y5Bd-rEqCmW3OHw-HTUPz2I8u_uFEDn5ebWQ_yMb2gtxTuKTYP_sbmWJe3Kt3BpVD3FqZrdgyGfhNxqlnhzPMzaV7MldZnZ1z0sodlEZdAXmudgQMoppOGwBw-WRgtf6H4yAqBeKTfZX1-iU01IgKqUKA",
            };

            Assert.True(jws.Verify());
        }

        [Fact]
        public void VerifyWithEC()
        {
            JsonWebSignature jws = new JsonWebSignature
            {
                Protected = "eyJhbGciOiJFUzI1NiIsImp3ayI6eyJrdHkiOiJFQyIsImNydiI6IlAtMjU2Iiwia2V5X29wcyI6WyJ2ZXJpZnkiXSwiZXh0Ijp0cnVlLCJ4IjoibXdhdW9YbWIwcllBTzBKWTJxNlgxV0hiVS1nVm5aY2pjcE5oLVRNVXotMCIsInkiOiJ5VlhySjZZeXd0aUtkVVNPb2hVVDUyVEhsYnQ4QVl0V2lLV1gtNldYYjUwIn0sInVybCI6Imh0dHBzOi8vYWNtZS50ZXN0LmNvbS9wYXRoIiwibm9uY2UiOiJkR1Z6ZENCdFpYTnpZV2RsIn0",
                Payload = "eyJvbmx5UmV0dXJuRXhpc3RpbmciOnRydWV9",
                Signature = "l8B0vPu_ZlfnIU00BKrFZPXRHzQd2qfGKPNCQqkUt7q8vmwdTPzvlFYRaLA2Q25GtP6K9IFUr72UpEPKBc1auw",
            };

            Assert.True(jws.Verify());
        }

        [Fact]
        public void WrongVerifyWithRSA()
        {
            JsonWebSignature jws = new JsonWebSignature
            {
                Protected = "eyJhbGciOiJSUzI1NiIsImp3ayI6eyJrdHkiOiJSU0EiLCJhbGciOiJSUzI1NiIsImtleV9vcHMiOlsidmVyaWZ5Il0sImV4dCI6dHJ1ZSwibiI6InNxVUJubG9nM1dFMWh5RUc5QlNSZjhqNEJ6TjY3UHJvekpEUUpSU3NGVWJmVDhiQ2x6ekxtTlo1VTFVaVN3bEQxUUhhVml6VWx4SXZ5Tl9QeHZBTHZzenAwOVhfbnRodXRUQjYyeXRtQVN1NnY3SG95RGVabURKeUdaTzI3bGNoaXJPQ1hNVFIzZ0FyMGlYTEhwaTdfQXN4YTZuWlNsZlhqV2JILWNvd296cjJXX1NKSXExOGNNUi0yQlJ6RW4xQzNUOThnQU1LcWJ3WEJnYzhHWFg1N1ZMcTNvQjlCUkhVTUJKdEFldFdRUW1aTE9qdkk5RXNfQVVfWEVTaUNJaFJWYWh5Sk5RRllrS21NMkowcHhOZTl0WTZMRFdLX211bXVfeFVxbEZxOGlNTWtEUm9jMXdqdVMteDFJWGo5cmpTWGVERXRzY0NkY1UyeHM2Um4tVGVZdyIsImUiOiJBUUFCIn0sInVybCI6Imh0dHBzOi8vYWNtZS50ZXN0LmNvbS9wYXRoIiwibm9uY2UiOiJkR1Z6ZENCdFpYTnpZV2RsIn0",
                Payload = "",
                Signature = "GoZY7BqVtUcrhrMaX1ETzs_NOu6p08sDIl4ZYpFBa-REeUYxuHimJRCIx2Z7p-59UYO9LmRP15VFT7jdku6qIURhJADugd57WQ53WvmburXi3fXGW2Gmx6qfPb9mvPs_-A9U9W0I9DPa4w6Qee408nFO8_XzyYgs5v3hHqSef1n-0lFtO3RiAOFynWf12Y5Bd-rEqCmW3OHw-HTUPz2I8u_uFEDn5ebWQ_yMb2gtxTuKTYP_sbmWJe3Kt3BpVD3FqZrdgyGfhNxqlnhzPMzaV7MldZnZ1z0sodlEZdAXmudgQMoppOGwBw-WRgtf6H4yAqBeKTfZX1-iU01IgKqUKA",
            };

            AsymmetricAlgorithm key = AsymmetricAlgorithm.Create("RSA");

            Assert.False(jws.Verify(key));
        }

        [Fact]
        public void WrongVerifyWithEC()
        {
            JsonWebSignature jws = new JsonWebSignature
            {
                Protected = "eyJhbGciOiJFUzI1NiIsImp3ayI6eyJrdHkiOiJFQyIsImNydiI6IlAtMjU2Iiwia2V5X29wcyI6WyJ2ZXJpZnkiXSwiZXh0Ijp0cnVlLCJ4IjoibXdhdW9YbWIwcllBTzBKWTJxNlgxV0hiVS1nVm5aY2pjcE5oLVRNVXotMCIsInkiOiJ5VlhySjZZeXd0aUtkVVNPb2hVVDUyVEhsYnQ4QVl0V2lLV1gtNldYYjUwIn0sInVybCI6Imh0dHBzOi8vYWNtZS50ZXN0LmNvbS9wYXRoIiwibm9uY2UiOiJkR1Z6ZENCdFpYTnpZV2RsIn0",
                Payload = "eyJvbmx5UmV0dXJuRXhpc3RpbmciOnRydWV9",
                Signature = "l8B0vPu_ZlfnIU00BKrFZPXRHzQd2qfGKPNCQqkUt7q8vmwdTPzvlFYRaLA2Q25GtP6K9IFUr72UpEPKBc1auw",
            };

            AsymmetricAlgorithm key = AsymmetricAlgorithm.Create("ECDsa");

            Assert.False(jws.Verify(key));
        }

        [Fact]
        public void CorrectParseRSA()
        {
            JsonWebSignature jws = new JsonWebSignature
            {
                Protected = "eyJhbGciOiJSUzI1NiIsImp3ayI6eyJrdHkiOiJSU0EiLCJhbGciOiJSUzI1NiIsImtleV9vcHMiOlsidmVyaWZ5Il0sImV4dCI6dHJ1ZSwibiI6InNxVUJubG9nM1dFMWh5RUc5QlNSZjhqNEJ6TjY3UHJvekpEUUpSU3NGVWJmVDhiQ2x6ekxtTlo1VTFVaVN3bEQxUUhhVml6VWx4SXZ5Tl9QeHZBTHZzenAwOVhfbnRodXRUQjYyeXRtQVN1NnY3SG95RGVabURKeUdaTzI3bGNoaXJPQ1hNVFIzZ0FyMGlYTEhwaTdfQXN4YTZuWlNsZlhqV2JILWNvd296cjJXX1NKSXExOGNNUi0yQlJ6RW4xQzNUOThnQU1LcWJ3WEJnYzhHWFg1N1ZMcTNvQjlCUkhVTUJKdEFldFdRUW1aTE9qdkk5RXNfQVVfWEVTaUNJaFJWYWh5Sk5RRllrS21NMkowcHhOZTl0WTZMRFdLX211bXVfeFVxbEZxOGlNTWtEUm9jMXdqdVMteDFJWGo5cmpTWGVERXRzY0NkY1UyeHM2Um4tVGVZdyIsImUiOiJBUUFCIn0sInVybCI6Imh0dHBzOi8vYWNtZS50ZXN0LmNvbS9wYXRoIiwibm9uY2UiOiJkR1Z6ZENCdFpYTnpZV2RsIn0",
                Payload = "",
                Signature = "GoZY7BqVtUcrhrMaX1ETzs_NOu6p08sDIl4ZYpFBa-REeUYxuHimJRCIx2Z7p-59UYO9LmRP15VFT7jdku6qIURhJADugd57WQ53WvmburXi3fXGW2Gmx6qfPb9mvPs_-A9U9W0I9DPa4w6Qee408nFO8_XzyYgs5v3hHqSef1n-0lFtO3RiAOFynWf12Y5Bd-rEqCmW3OHw-HTUPz2I8u_uFEDn5ebWQ_yMb2gtxTuKTYP_sbmWJe3Kt3BpVD3FqZrdgyGfhNxqlnhzPMzaV7MldZnZ1z0sodlEZdAXmudgQMoppOGwBw-WRgtf6H4yAqBeKTfZX1-iU01IgKqUKA",
            };

            var @protected = jws.GetProtected();

            Assert.Equal(@protected.Algorithm, AlgorithmsEnum.RS256);
            Assert.Equal(@protected.Nonce, "dGVzdCBtZXNzYWdl");
            Assert.Equal(@protected.Url, "https://acme.test.com/path");
            Assert.Equal(@protected.Key.Algorithm, AlgorithmsEnum.RS256);
            Assert.Equal(@protected.Key.KeyType, KeyTypesEnum.RSA);
            Assert.Equal(@protected.Key.Exponent, "AQAB");
            Assert.Equal(@protected.Key.Modulus, "sqUBnlog3WE1hyEG9BSRf8j4BzN67ProzJDQJRSsFUbfT8bClzzLmNZ5U1UiSwlD1QHaVizUlxIvyN_PxvALvszp09X_nthutTB62ytmASu6v7HoyDeZmDJyGZO27lchirOCXMTR3gAr0iXLHpi7_Asxa6nZSlfXjWbH-cowozr2W_SJIq18cMR-2BRzEn1C3T98gAMKqbwXBgc8GXX57VLq3oB9BRHUMBJtAetWQQmZLOjvI9Es_AU_XESiCIhRVahyJNQFYkKmM2J0pxNe9tY6LDWK_mumu_xUqlFq8iMMkDRoc1wjuS-x1IXj9rjSXeDEtscCdcU2xs6Rn-TeYw");
        }

        [Fact]
        public void CorrectParseEC()
        {
            JsonWebSignature jws = new JsonWebSignature
            {
                Protected = "eyJhbGciOiJFUzI1NiIsImp3ayI6eyJrdHkiOiJFQyIsImNydiI6IlAtMjU2Iiwia2V5X29wcyI6WyJ2ZXJpZnkiXSwiZXh0Ijp0cnVlLCJ4IjoibXdhdW9YbWIwcllBTzBKWTJxNlgxV0hiVS1nVm5aY2pjcE5oLVRNVXotMCIsInkiOiJ5VlhySjZZeXd0aUtkVVNPb2hVVDUyVEhsYnQ4QVl0V2lLV1gtNldYYjUwIn0sInVybCI6Imh0dHBzOi8vYWNtZS50ZXN0LmNvbS9wYXRoIiwibm9uY2UiOiJkR1Z6ZENCdFpYTnpZV2RsIn0",
                Payload = "eyJvbmx5UmV0dXJuRXhpc3RpbmciOnRydWV9",
                Signature = "l8B0vPu_ZlfnIU00BKrFZPXRHzQd2qfGKPNCQqkUt7q8vmwdTPzvlFYRaLA2Q25GtP6K9IFUr72UpEPKBc1auw",
            };
            var @protected = jws.GetProtected();

            Assert.Equal(@protected.Algorithm, AlgorithmsEnum.ES256);
            Assert.Equal(@protected.Nonce, "dGVzdCBtZXNzYWdl");
            Assert.Equal(@protected.Url, "https://acme.test.com/path");
            Assert.Equal(@protected.Key.KeyType, KeyTypesEnum.EC);
            Assert.Equal(@protected.Key.EllipticCurve, EllipticCurvesEnum.P256);
            Assert.Equal(@protected.Key.X, "mwauoXmb0rYAO0JY2q6X1WHbU-gVnZcjcpNh-TMUz-0");
            Assert.Equal(@protected.Key.Y, "yVXrJ6YywtiKdUSOohUT52THlbt8AYtWiKWX-6WXb50");
        }
    }
}
