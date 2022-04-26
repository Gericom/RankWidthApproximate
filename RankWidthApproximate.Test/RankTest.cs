using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;
using RankWidthApproximate;
using RankWidthApproximate.Data;
using RankWidthApproximate.Data.Graph;
using RankWidthApproximate.LocalSearch.CutFunction;

namespace RankWidthApproximate.Test
{
    public class RankTest
    {
        private static int ComputeMatrixRank(BitMatrix mtx)
        {
            var graph = new AdjMtxGraph(mtx.Columns + mtx.Rows);
            for (int i = 0; i < mtx.Rows; i++)
            {
                for (int j = 0; j < mtx.Columns; j++)
                {
                    graph.AdjacencyMtx[i, mtx.Rows + j] = mtx[i, j];
                    graph.AdjacencyMtx[mtx.Rows + j, i] = mtx[i, j];
                }
            }

            var mask = new BitArrayEx(graph.VtxCount);
            for (int i = mtx.Rows; i < graph.VtxCount; i++)
                mask[i] = true;

            var func = new CutRankFunction();
            return func.Compute(graph, mask, mtx.Columns);
        }

        private static int ComputeMatrixRankSparse(BitMatrix mtx)
        {
            var graph = new AdjMtxGraph(mtx.Columns + mtx.Rows);
            for (int i = 0; i < mtx.Rows; i++)
            {
                for (int j = 0; j < mtx.Columns; j++)
                {
                    graph.AdjacencyMtx[i, mtx.Rows + j] = mtx[i, j];
                    graph.AdjacencyMtx[mtx.Rows + j, i] = mtx[i, j];
                }
            }

            var mask = new BitArrayEx(graph.VtxCount);
            for (int i = mtx.Rows; i < graph.VtxCount; i++)
                mask[i] = true;

            var func = new SparseCutRankFunction();
            return func.Compute(new AdjListGraph(graph), mask, mtx.Columns);
        }

        [Test]
        [TestCase(new[]
        {
            true, true, false,
            false, true, true,
            true, false, true
        }, 3, 2)]
        [TestCase(new[]
        {
            false, true, true, false,
            false, true, false, false
        }, 4, 2)]
        [TestCase(new[]
        {
            true, false, false, false,
            false, true, false, false,
            false, false, true, false,
            false, false, false, true
        }, 4, 4)]
        [TestCase(new[]
        {
            true, false, false, true,
            false, false, false, false,
            false, false, true, false,
            false, true, false, true
        }, 4, 3)]
        public void ComputeRank(bool[] array, int stride, int expected)
        {
            var mtx = new BitMatrix(array, stride);
            Assert.AreEqual(expected, ComputeMatrixRank(mtx));

            var mtx2 = new BitMatrix(mtx.Columns, mtx.Rows);
            for (int i = 0; i < mtx.Rows; i++)
                for (int j = 0; j < mtx.Columns; j++)
                    mtx2[j, i] = mtx[i, j];
            Assert.AreEqual(expected, ComputeMatrixRank(mtx2));
        }

        [Test]
        [TestCase(new[]
        {
            true, true, false,
            false, true, true,
            true, false, true
        }, 3, 2)]
        [TestCase(new[]
        {
            false, true, true, false,
            false, true, false, false
        }, 4, 2)]
        [TestCase(new[]
        {
            true, false, false, false,
            false, true, false, false,
            false, false, true, false,
            false, false, false, true
        }, 4, 4)]
        [TestCase(new[]
        {
            true, false, false, true,
            false, false, false, false,
            false, false, true, false,
            false, true, false, true
        }, 4, 3)]
        public void ComputeRankSparse(bool[] array, int stride, int expected)
        {
            var mtx = new BitMatrix(array, stride);
            Assert.AreEqual(expected, ComputeMatrixRankSparse(mtx));

            var mtx2 = new BitMatrix(mtx.Columns, mtx.Rows);
            for (int i = 0; i < mtx.Rows; i++)
                for (int j = 0; j < mtx.Columns; j++)
                    mtx2[j, i] = mtx[i, j];
            Assert.AreEqual(expected, ComputeMatrixRankSparse(mtx2));
        }
    }
}