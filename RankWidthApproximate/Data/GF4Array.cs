using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

namespace RankWidthApproximate.Data
{
    /// <summary>
    /// Class representing an array of numbers in GF(4).
    /// The multiplication is based on https://github.com/moepinet/libmoepgf/blob/master/src/gf4.c
    /// </summary>
    public class GF4Array : ICloneable
    {
        private readonly ulong[] _data;

        public int Length { get; }

        public GF4Array(int length)
        {
            Debug.Assert(length > 0);
            Length = length;
            _data  = new ulong[(length + 31) >> 5];
        }

        public GF4Array(GF4Array src)
        {
            Debug.Assert(src != null);
            Length = src.Length;
            _data  = GC.AllocateUninitializedArray<ulong>(src._data.Length);
            Buffer.BlockCopy(src._data, 0, _data, 0, src._data.Length * sizeof(ulong));
        }

        public int this[int idx]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Debug.Assert(idx >= 0 && idx < Length);
                return (int)((_data[idx >> 5] >> ((idx & 31) << 1)) & 3);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                Debug.Assert(value >= 0 && value <= 3);
                _data[idx >> 5] &= ~(3UL << ((idx & 31) << 1));
                _data[idx >> 5] |= (ulong)value << ((idx & 31) << 1);
            }
        }

        /// <summary>
        /// Adds GF(4) array b to this array
        /// </summary>
        /// <param name="b">The array to add</param>
        /// <returns>This array</returns>
        public unsafe GF4Array Add(GF4Array b)
        {
            Debug.Assert(b != null);
            Debug.Assert(Length == b.Length);
            int count = _data.Length;
            switch (count)
            {
                case 3:
                    _data[2] ^= b._data[2];
                    goto case 2;
                case 2:
                    _data[1] ^= b._data[1];
                    goto case 1;
                case 1:
                    _data[0] ^= b._data[0];
                    return this;
                case 0:
                    return this;
            }

            uint i = 0;
            if (Avx2.IsSupported)
            {
                fixed (ulong* leftPtr = _data)
                fixed (ulong* rightPtr = b._data)
                {
                    for (; i < (uint)count - 3u; i += 4u)
                    {
                        var leftVec  = Avx.LoadVector256(leftPtr + i);
                        var rightVec = Avx.LoadVector256(rightPtr + i);
                        Avx.Store(leftPtr + i, Avx2.Xor(leftVec, rightVec));
                    }
                }
            }
            else if (Sse2.IsSupported)
            {
                fixed (ulong* leftPtr = _data)
                fixed (ulong* rightPtr = b._data)
                {
                    for (; i < (uint)count - 1u; i += 2u)
                    {
                        var leftVec  = Sse2.LoadVector128(leftPtr + i);
                        var rightVec = Sse2.LoadVector128(rightPtr + i);
                        Sse2.Store(leftPtr + i, Sse2.Xor(leftVec, rightVec));
                    }
                }
            }
            else if (AdvSimd.IsSupported)
            {
                fixed (ulong* leftPtr = _data)
                fixed (ulong* rightPtr = b._data)
                {
                    for (; i < (uint)count - 1u; i += 2u)
                    {
                        var leftVec  = AdvSimd.LoadVector128(leftPtr + i);
                        var rightVec = AdvSimd.LoadVector128(rightPtr + i);
                        AdvSimd.Store(leftPtr + i, AdvSimd.Xor(leftVec, rightVec));
                    }
                }
            }

            for (; i < (uint)count; i++)
                _data[i] ^= b._data[i];

            return this;
        }

        /// <summary>
        /// Multiplies this array by the given factor
        /// </summary>
        /// <param name="factor">The factor to multiply by</param>
        /// <returns>This array</returns>
        public unsafe GF4Array Multiply(int factor)
        {
            Debug.Assert(factor >= 0 && factor <= 3);

            if (factor == 0)
            {
                //multiply by zero always yields zero
                Array.Clear(_data, 0, _data.Length);
                return this;
            }

            if (factor == 1) //multiply by one keeps everything the same
                return this;

            //Multiplication is based on the Russian peasant multiplication method
            //Since there are only two bits, only two iterations are needed at most
            //and this can be merged into a single formula

            //Let x be the variable side of the multiplication, and let c be a constant
            //Then to calculate y = x * c in GF(4) we can do
            //c2 = (c << 1) ^ (c >= 2 ? 7 : 0)
            //y = ((x & 1) * c) ^ (((x >> 1) & 1) * c2)

            uint mulFactor = (uint)((factor << 1) ^ 7); //calculate c2, note that we already know factor >= 2

            int   count = _data.Length;
            ulong tmpA, tmpB;
            switch (count)
            {
                case 3:
                    tmpA     = (_data[2] & 0x5555555555555555UL) * (uint)factor;
                    tmpB     = ((_data[2] >> 1) & 0x5555555555555555UL) * mulFactor;
                    _data[2] = tmpA ^ tmpB;
                    goto case 2;
                case 2:
                    tmpA     = (_data[1] & 0x5555555555555555UL) * (uint)factor;
                    tmpB     = ((_data[1] >> 1) & 0x5555555555555555UL) * mulFactor;
                    _data[1] = tmpA ^ tmpB;
                    goto case 1;
                case 1:
                    tmpA     = (_data[0] & 0x5555555555555555UL) * (uint)factor;
                    tmpB     = ((_data[0] >> 1) & 0x5555555555555555UL) * mulFactor;
                    _data[0] = tmpA ^ tmpB;
                    return this;
                case 0:
                    return this;
            }

            uint i = 0;
            if (Avx2.IsSupported)
            {
                var maskVec = Vector256.Create((ushort)0x5555);
                var mulVecA = Vector256.Create((ushort)factor);
                var mulVecB = Vector256.Create((ushort)mulFactor);
                fixed (ulong* ptr = _data)
                {
                    for (; i < (uint)count - 3u; i += 4u)
                    {
                        var vec     = Avx.LoadVector256(ptr + i).AsUInt16();
                        var vecTmpA = Avx2.MultiplyLow(Avx2.And(vec, maskVec), mulVecA);
                        var vecTmpB = Avx2.MultiplyLow(Avx2.And(Avx2.ShiftRightLogical(vec, 1), maskVec), mulVecB);
                        Avx.Store(ptr + i, Avx2.Xor(vecTmpA, vecTmpB).AsUInt64());
                    }
                }
            }
            else if (Sse2.IsSupported)
            {
                var maskVec = Vector128.Create((ushort)0x5555);
                var mulVecA = Vector128.Create((ushort)factor);
                var mulVecB = Vector128.Create((ushort)mulFactor);
                fixed (ulong* ptr = _data)
                {
                    for (; i < (uint)count - 1u; i += 2u)
                    {
                        var vec     = Sse2.LoadVector128(ptr + i).AsUInt16();
                        var vecTmpA = Sse2.MultiplyLow(Sse2.And(vec, maskVec), mulVecA);
                        var vecTmpB = Sse2.MultiplyLow(Sse2.And(Sse2.ShiftRightLogical(vec, 1), maskVec), mulVecB);
                        Sse2.Store(ptr + i, Sse2.Xor(vecTmpA, vecTmpB).AsUInt64());
                    }
                }
            }
            else if (AdvSimd.IsSupported)
            {
                var maskVec = Vector128.Create((byte)0x55);
                var mulVecA = Vector128.Create((byte)factor);
                var mulVecB = Vector128.Create((byte)mulFactor);
                fixed (ulong* ptr = _data)
                {
                    for (; i < (uint)count - 1u; i += 2u)
                    {
                        var vec     = AdvSimd.LoadVector128(ptr + i).AsByte();
                        var vecTmpA = AdvSimd.Multiply(AdvSimd.And(vec, maskVec), mulVecA);
                        var vecTmpB = AdvSimd.Multiply(AdvSimd.And(AdvSimd.ShiftRightLogical(vec, 1), maskVec),
                            mulVecB);
                        AdvSimd.Store(ptr + i, AdvSimd.Xor(vecTmpA, vecTmpB).AsUInt64());
                    }
                }
            }

            for (; i < (uint)count; i++)
            {
                tmpA     = (_data[i] & 0x5555555555555555UL) * (uint)factor;
                tmpB     = ((_data[i] >> 1) & 0x5555555555555555UL) * mulFactor;
                _data[i] = tmpA ^ tmpB;
            }

            return this;
        }

        /// <summary>
        /// Adds GF(4) array b, multiplied by factor to this array
        /// </summary>
        /// <param name="b">The array to multiply and add</param>
        /// <param name="factor">The factor to multiply b by</param>
        /// <returns>This array</returns>
        public unsafe GF4Array MultiplyAdd(GF4Array b, int factor)
        {
            Debug.Assert(b != null);
            Debug.Assert(Length == b.Length);
            Debug.Assert(factor >= 0 && factor <= 3);

            if (factor == 0)
                return this;

            if (factor == 1)
                return Add(b);

            uint mulFactor = (uint)((factor << 1) ^ 7);

            int   count = _data.Length;
            ulong tmpA, tmpB;
            switch (count)
            {
                case 3:
                    tmpA     =  (b._data[2] & 0x5555555555555555UL) * (uint)factor;
                    tmpB     =  ((b._data[2] >> 1) & 0x5555555555555555UL) * mulFactor;
                    _data[2] ^= tmpA ^ tmpB;
                    goto case 2;
                case 2:
                    tmpA     =  (b._data[1] & 0x5555555555555555UL) * (uint)factor;
                    tmpB     =  ((b._data[1] >> 1) & 0x5555555555555555UL) * mulFactor;
                    _data[1] ^= tmpA ^ tmpB;
                    goto case 1;
                case 1:
                    tmpA     =  (b._data[0] & 0x5555555555555555UL) * (uint)factor;
                    tmpB     =  ((b._data[0] >> 1) & 0x5555555555555555UL) * mulFactor;
                    _data[0] ^= tmpA ^ tmpB;
                    return this;
                case 0:
                    return this;
            }

            uint i = 0;
            if (Avx2.IsSupported)
            {
                var maskVec = Vector256.Create((ushort)0x5555);
                var mulVecA = Vector256.Create((ushort)factor);
                var mulVecB = Vector256.Create((ushort)mulFactor);
                fixed (ulong* leftPtr = _data)
                fixed (ulong* rightPtr = b._data)
                {
                    for (; i < (uint)count - 3u; i += 4u)
                    {
                        var vecB    = Avx.LoadVector256(rightPtr + i).AsUInt16();
                        var vecTmpA = Avx2.MultiplyLow(Avx2.And(vecB, maskVec), mulVecA);
                        var vecTmpB = Avx2.MultiplyLow(Avx2.And(Avx2.ShiftRightLogical(vecB, 1), maskVec), mulVecB);
                        var vecA    = Avx.LoadVector256(leftPtr + i).AsUInt16();
                        Avx.Store(leftPtr + i, Avx2.Xor(vecA, Avx2.Xor(vecTmpA, vecTmpB)).AsUInt64());
                    }
                }
            }
            else if (Sse2.IsSupported)
            {
                var maskVec = Vector128.Create((ushort)0x5555);
                var mulVecA = Vector128.Create((ushort)factor);
                var mulVecB = Vector128.Create((ushort)mulFactor);
                fixed (ulong* leftPtr = _data)
                fixed (ulong* rightPtr = b._data)
                {
                    for (; i < (uint)count - 1u; i += 2u)
                    {
                        var vecB    = Sse2.LoadVector128(rightPtr + i).AsUInt16();
                        var vecTmpA = Sse2.MultiplyLow(Sse2.And(vecB, maskVec), mulVecA);
                        var vecTmpB = Sse2.MultiplyLow(Sse2.And(Sse2.ShiftRightLogical(vecB, 1), maskVec), mulVecB);
                        var vecA    = Sse2.LoadVector128(leftPtr + i).AsUInt16();
                        Sse2.Store(leftPtr + i, Sse2.Xor(vecA, Sse2.Xor(vecTmpA, vecTmpB)).AsUInt64());
                    }
                }
            }
            else if (AdvSimd.IsSupported)
            {
                var maskVec = Vector128.Create((byte)0x55);
                var mulVecA = Vector128.Create((byte)factor);
                var mulVecB = Vector128.Create((byte)mulFactor);
                fixed (ulong* leftPtr = _data)
                fixed (ulong* rightPtr = b._data)
                {
                    for (; i < (uint)count - 1u; i += 2u)
                    {
                        var vecB    = AdvSimd.LoadVector128(rightPtr + i).AsByte();
                        var vecTmpA = AdvSimd.Multiply(AdvSimd.And(vecB, maskVec), mulVecA);
                        var vecTmpB = AdvSimd.Multiply(AdvSimd.And(AdvSimd.ShiftRightLogical(vecB, 1), maskVec),
                            mulVecB);
                        var vecA = AdvSimd.LoadVector128(leftPtr + i).AsByte();
                        AdvSimd.Store(leftPtr + i, AdvSimd.Xor(vecA, AdvSimd.Xor(vecTmpA, vecTmpB)).AsUInt64());
                    }
                }
            }

            for (; i < (uint)count; i++)
            {
                tmpA     =  (b._data[i] & 0x5555555555555555UL) * (uint)factor;
                tmpB     =  ((b._data[i] >> 1) & 0x5555555555555555UL) * mulFactor;
                _data[i] ^= tmpA ^ tmpB;
            }

            return this;
        }

        public void Clear()
        {
            Array.Clear(_data, 0, _data.Length);
        }

        public object Clone() => new GF4Array(this);

        public void Set(GF4Array src)
        {
            Debug.Assert(Length == src.Length);
            Buffer.BlockCopy(src._data, 0, _data, 0, src._data.Length * sizeof(ulong));
        }
    }
}