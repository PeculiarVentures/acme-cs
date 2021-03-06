﻿using System;
using System.Text;

namespace PeculiarVentures.ACME.Helpers
{
    public static class Base64Url
    {
        public static string Encode(string input)
        {

            byte[] bytesToEncode = Encoding.UTF8.GetBytes(input);

            return Encode(bytesToEncode);
        }

        public static string Encode(byte[] input)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (input.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(input));

            var output = Convert.ToBase64String(input);

            output = output.Split('=')[0]; // Remove any trailing '='s
            output = output.Replace('+', '-'); // 62nd char of encoding
            output = output.Replace('/', '_'); // 63rd char of encoding

            return output;
        }

        public static byte[] Decode(byte[] input)
        {
            string stringToDecode = Encoding.UTF8.GetString(input);

            return Decode(stringToDecode);
        }

        public static byte[] Decode(string input)
        {
            if (String.IsNullOrWhiteSpace(input))
                throw new ArgumentException(nameof(input));

            var output = input;

            output = output.Replace('-', '+'); // 62nd char of encoding
            output = output.Replace('_', '/'); // 63rd char of encoding

            switch (output.Length % 4) // Pad with trailing '='s
            {
                case 0:
                    break; // No pad chars in this case
                case 2:
                    output += "==";
                    break; // Two pad chars
                case 3:
                    output += "=";
                    break; // One pad char
                default:
                    throw new FormatException("Illegal base64url string.");
            }

            return Convert.FromBase64String(output); // Standard base64 decoder
        }
    }
}
