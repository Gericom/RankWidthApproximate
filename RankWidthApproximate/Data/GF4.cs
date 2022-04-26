using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace RankWidthApproximate.Data
{
    /// <summary>
    /// Static class implementing basic math operations on GF(4) numbers
    /// </summary>
    public static class GF4
    {
        public const int Zero        = 0;
        public const int One         = 1;
        public const int Alpha       = 2;
        public const int AlphaSquare = 3;

        /// <summary>
        /// Adds two numbers in GF(4)
        /// </summary>
        public static int Add(int x, int y)
        {
            Debug.Assert(x >= 0 && x <= 3);
            Debug.Assert(y >= 0 && y <= 3);
            return x ^ y;
        }

        /// <summary>
        /// Multiplies two numbers in GF(4)
        /// </summary>
        public static int Multiply(int x, int y)
        {
            Debug.Assert(x >= 0 && x <= 3);
            Debug.Assert(y >= 0 && y <= 3);
            int c2 = (y << 1) ^ (y >= 2 ? 7 : 0);
            return ((x & 1) * y) ^ (((x >> 1) & 1) * c2);
        }

        private static readonly int[] sInvTable = { -1, One, AlphaSquare, Alpha };

        /// <summary>
        /// Computes the multiplicative inverse in GF(4)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Inverse(int x)
        {
            Debug.Assert(x >= 1 && x <= 3);
            return sInvTable[x];
        }

        /// <summary>
        /// Divides two numbers in GF(4)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Divide(int num, int den)
        {
            Debug.Assert(num >= 0 && num <= 3);
            Debug.Assert(den >= 1 && den <= 3);
            return Multiply(num, sInvTable[den]);
        }
    }
}