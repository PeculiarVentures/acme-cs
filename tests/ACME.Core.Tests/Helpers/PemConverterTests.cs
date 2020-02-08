using System;
using System.Text;
using Xunit;

namespace PeculiarVentures.ACME.Helpers
{
    public class PemConverterTests
    {
        private const string PEM =  "-----BEGIN CERTIFICATE-----\n" +
                                    "TWFuIGlzIGRpc3Rpbmd1aXNoZWQsIG5vdCBvbmx5IGJ5IGhpcyByZWFzb24sIGJ1\n" +
                                    "dCBieSB0aGlzIHNpbmd1bGFyIHBhc3Npb24gZnJvbSBvdGhlciBhbmltYWxzLCB3\n" +
                                    "aGljaCBpcyBhIGx1c3Qgb2YgdGhlIG1pbmQsIHRoYXQgYnkgYSBwZXJzZXZlcmFu\n" +
                                    "Y2Ugb2YgZGVsaWdodCBpbiB0aGUgY29udGludWVkIGFuZCBpbmRlZmF0aWdhYmxl\n" +
                                    "IGdlbmVyYXRpb24gb2Yga25vd2xlZGdlLCBleGNlZWRzIHRoZSBzaG9ydCB2ZWhl\n" +
                                    "bWVuY2Ugb2YgYW55IGNhcm5hbCBwbGVhc3VyZS4=\n" +
                                    "-----END CERTIFICATE-----\n" +
                                    "-----BEGIN CERTIFICATE-----\n" +
                                    "TWFuIGlzIGRpc3Rpbmd1aXNoZWQsIG5vdCBvbmx5IGJ5IGhpcyByZWFzb24sIGJ1\n" +
                                    "dCBieSB0aGlzIHNpbmd1bGFyIHBhc3Npb24gZnJvbSBvdGhlciBhbmltYWxzLCB3\n" +
                                    "aGljaCBpcyBhIGx1c3Qgb2YgdGhlIG1pbmQsIHRoYXQgYnkgYSBwZXJzZXZlcmFu\n" +
                                    "Y2Ugb2YgZGVsaWdodCBpbiB0aGUgY29udGludWVkIGFuZCBpbmRlZmF0aWdhYmxl\n" +
                                    "IGdlbmVyYXRpb24gb2Yga25vd2xlZGdlLCBleGNlZWRzIHRoZSBzaG9ydCB2ZWhl\n" +
                                    "bWVuY2Ugb2YgYW55IGNhcm5hbCBwbGVhc3VyZS4=\n" +
                                    "-----END CERTIFICATE-----\n" +
                                    "-----BEGIN CERTIFICATE-----\n" +
                                    "TWFuIGlzIGRpc3Rpbmd1aXNoZWQsIG5vdCBvbmx5IGJ5IGhpcyByZWFzb24sIGJ1\n" +
                                    "dCBieSB0aGlzIHNpbmd1bGFyIHBhc3Npb24gZnJvbSBvdGhlciBhbmltYWxzLCB3\n" +
                                    "aGljaCBpcyBhIGx1c3Qgb2YgdGhlIG1pbmQsIHRoYXQgYnkgYSBwZXJzZXZlcmFu\n" +
                                    "Y2Ugb2YgZGVsaWdodCBpbiB0aGUgY29udGludWVkIGFuZCBpbmRlZmF0aWdhYmxl\n" +
                                    "IGdlbmVyYXRpb24gb2Yga25vd2xlZGdlLCBleGNlZWRzIHRoZSBzaG9ydCB2ZWhl\n" +
                                    "bWVuY2Ugb2YgYW55IGNhcm5hbCBwbGVhc3VyZS4=\n" +
                                    "-----END CERTIFICATE-----";
        private const string DATA = "TWFuIGlzIGRpc3Rpbmd1aXNoZWQsIG5vdCBvbmx5IGJ5IGhpcyByZWFzb24sIGJ1dCBieSB0aGlzIHNpbmd1bGFyIHBhc3Npb24gZnJvbSBvdGhlciBhbmltYWxzLCB3aGljaCBpcyBhIGx1c3Qgb2YgdGhlIG1pbmQsIHRoYXQgYnkgYSBwZXJzZXZlcmFuY2Ugb2YgZGVsaWdodCBpbiB0aGUgY29udGludWVkIGFuZCBpbmRlZmF0aWdhYmxlIGdlbmVyYXRpb24gb2Yga25vd2xlZGdlLCBleGNlZWRzIHRoZSBzaG9ydCB2ZWhlbWVuY2Ugb2YgYW55IGNhcm5hbCBwbGVhc3VyZS4=";

        [Fact]
        public void Encode_Array()
        {
            var rawData = Convert.FromBase64String(DATA);
            var rawDataArray = new byte[][]
            {
                rawData,
                rawData,
                rawData
            };
            var res = PemConverter.Encode(rawDataArray, "certificate");
            Assert.Equal(PEM, res);
        }

        [Fact]
        public void Encode()
        {
            var rawData = Convert.FromBase64String("TWFuIGlzIGRpc3Rpbmd1aXNoZWQsIG5vdCBvbmx5IGJ5IGhpcyByZWFzb24sIGJ1");
            var expected = "-----BEGIN CERTIFICATE-----\n" +
                           "TWFuIGlzIGRpc3Rpbmd1aXNoZWQsIG5vdCBvbmx5IGJ5IGhpcyByZWFzb24sIGJ1\n" +
                           "-----END CERTIFICATE-----";
            var res = PemConverter.Encode(rawData, "certificate");
            Assert.Equal(expected, res);
        }

        [Fact]
        public void Decode()
        {
            var res = PemConverter.Decode(PEM);
            Assert.Equal(DATA, Convert.ToBase64String(res[0]));
        }
    }
}
