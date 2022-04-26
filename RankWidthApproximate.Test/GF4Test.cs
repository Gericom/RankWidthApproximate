using System;
using NUnit.Framework;
using RankWidthApproximate.Data;

namespace RankWidthApproximate.Test
{
    public class GF4Test
    {
        private static readonly int[,] sGF4MulTab =
        {
            { GF4.Zero, GF4.Zero, GF4.Zero, GF4.Zero },
            { GF4.Zero, GF4.One, GF4.Alpha, GF4.AlphaSquare },
            { GF4.Zero, GF4.Alpha, GF4.AlphaSquare, GF4.One },
            { GF4.Zero, GF4.AlphaSquare, GF4.One, GF4.Alpha }
        };

        private static readonly int[,] sGF4DivTab =
        {
            { -1, GF4.Zero, GF4.Zero, GF4.Zero },
            { -1, GF4.One, GF4.AlphaSquare, GF4.Alpha },
            { -1, GF4.Alpha, GF4.One, GF4.AlphaSquare },
            { -1, GF4.AlphaSquare, GF4.Alpha, GF4.One }
        };

        [Test]
        public void AddArrayShort()
        {
            var rand = new Random(1);

            var baseArray = new GF4Array(96);
            for (int i = 0; i < baseArray.Length; i++)
                baseArray[i] = rand.Next(4);

            var baseArray2 = new GF4Array(96);
            for (int i = 0; i < baseArray2.Length; i++)
                baseArray2[i] = rand.Next(4);

            var result = new GF4Array(baseArray).Add(baseArray2);
            for (int i = 0; i < baseArray.Length; i++)
                Assert.AreEqual(baseArray[i] ^ baseArray2[i], result[i]);
        }

        [Test]
        public void AddArrayLong()
        {
            var rand = new Random(1);

            var baseArray = new GF4Array(8192 + 8);
            for (int i = 0; i < baseArray.Length; i++)
                baseArray[i] = rand.Next(4);

            var baseArray2 = new GF4Array(8192 + 8);
            for (int i = 0; i < baseArray2.Length; i++)
                baseArray2[i] = rand.Next(4);

            var result = new GF4Array(baseArray).Add(baseArray2);
            for (int i = 0; i < baseArray.Length; i++)
                Assert.AreEqual(baseArray[i] ^ baseArray2[i], result[i]);
        }

        [Test]
        public void Multiply(
            [Values(GF4.Zero, GF4.One, GF4.Alpha, GF4.AlphaSquare)]
            int x,
            [Values(GF4.Zero, GF4.One, GF4.Alpha, GF4.AlphaSquare)]
            int y)
        {
            Assert.AreEqual(sGF4MulTab[x, y], GF4.Multiply(x, y));
        }

        [TestCase(GF4.Zero)]
        [TestCase(GF4.One)]
        [TestCase(GF4.Alpha)]
        [TestCase(GF4.AlphaSquare)]
        public void MulArrayShort(int factor)
        {
            var baseArray = new GF4Array(4);
            baseArray[0] = GF4.Zero;
            baseArray[1] = GF4.One;
            baseArray[2] = GF4.Alpha;
            baseArray[3] = GF4.AlphaSquare;
            var mul = new GF4Array(baseArray).Multiply(factor);
            for (int i = 0; i < baseArray.Length; i++)
                Assert.AreEqual(sGF4MulTab[baseArray[i], factor], mul[i]);
        }

        [TestCase(GF4.Zero)]
        [TestCase(GF4.One)]
        [TestCase(GF4.Alpha)]
        [TestCase(GF4.AlphaSquare)]
        public void MulArrayLong(int factor)
        {
            var rand = new Random(1);

            var baseArray = new GF4Array(8192 + 8);
            for (int i = 0; i < baseArray.Length; i++)
                baseArray[i] = rand.Next(4);

            var mul = new GF4Array(baseArray).Multiply(factor);
            for (int i = 0; i < baseArray.Length; i++)
                Assert.AreEqual(sGF4MulTab[baseArray[i], factor], mul[i]);
        }

        [TestCase(GF4.Zero)]
        [TestCase(GF4.One)]
        [TestCase(GF4.Alpha)]
        [TestCase(GF4.AlphaSquare)]
        public void MulAddArrayShort(int factor)
        {
            var rand = new Random(1);

            var baseArray = new GF4Array(96);
            for (int i = 0; i < baseArray.Length; i++)
                baseArray[i] = rand.Next(4);

            var baseArray2 = new GF4Array(96);
            for (int i = 0; i < baseArray2.Length; i++)
                baseArray2[i] = rand.Next(4);

            var mul = new GF4Array(baseArray).MultiplyAdd(baseArray2, factor);
            for (int i = 0; i < baseArray.Length; i++)
                Assert.AreEqual(baseArray[i] ^ sGF4MulTab[baseArray2[i], factor], mul[i]);
        }

        [TestCase(GF4.Zero)]
        [TestCase(GF4.One)]
        [TestCase(GF4.Alpha)]
        [TestCase(GF4.AlphaSquare)]
        public void MulAddArrayLong(int factor)
        {
            var rand = new Random(1);

            var baseArray = new GF4Array(8192 + 8);
            for (int i = 0; i < baseArray.Length; i++)
                baseArray[i] = rand.Next(4);

            var baseArray2 = new GF4Array(8192 + 8);
            for (int i = 0; i < baseArray2.Length; i++)
                baseArray2[i] = rand.Next(4);

            var mul = new GF4Array(baseArray).MultiplyAdd(baseArray2, factor);
            for (int i = 0; i < baseArray.Length; i++)
                Assert.AreEqual(baseArray[i] ^ sGF4MulTab[baseArray2[i], factor], mul[i]);
        }

        [Test]
        public void Inverse([Values(GF4.One, GF4.Alpha, GF4.AlphaSquare)] int x)
        {
            Assert.AreEqual(GF4.One, GF4.Multiply(x, GF4.Inverse(x)));
        }

        [Test]
        public void Divide(
            [Values(GF4.Zero, GF4.One, GF4.Alpha, GF4.AlphaSquare)]
            int num,
            [Values(GF4.One, GF4.Alpha, GF4.AlphaSquare)]
            int den)
        {
            Assert.AreEqual(sGF4DivTab[num, den], GF4.Divide(num, den));
        }
    }
}