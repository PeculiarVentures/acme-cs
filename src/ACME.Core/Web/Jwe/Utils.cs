using System;
using System.Linq;
using System.Security.Cryptography;

namespace PeculiarVentures.ACME.Web.Jwe
{
    public class Utils
    {
        private static RandomNumberGenerator rng;

        public static byte[] Random(int sizeBits = 128)
        {
            byte[] data = new byte[sizeBits / 8];

            RNG.GetBytes(data);

            return data;
        }

        internal static RandomNumberGenerator RNG
        {
            get
            {
                return rng ?? (rng = RandomNumberGenerator.Create());
            }
        }

        public static byte[] CombineArrays(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }
    }
}
