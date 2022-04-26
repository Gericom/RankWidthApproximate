using NUnit.Framework;
using RankWidthApproximate.Data;
using RankWidthApproximate.Data.Graph;
using RankWidthApproximate.LocalSearch.CutFunction;

namespace RankWidthApproximate.Test
{
    public class MmTest
    {
        [Test]
        public void MaximumMatchingTest1()
        {
            var func  = new MmCutFunction();
            var graph = new AdjMtxGraph(8);
            graph.AdjacencyMtx[0, 5] = true;
            graph.AdjacencyMtx[5, 0] = true;
            graph.AdjacencyMtx[0, 6] = true;
            graph.AdjacencyMtx[6, 0] = true;
            graph.AdjacencyMtx[1, 4] = true;
            graph.AdjacencyMtx[4, 1] = true;
            graph.AdjacencyMtx[2, 5] = true;
            graph.AdjacencyMtx[5, 2] = true;
            graph.AdjacencyMtx[3, 5] = true;
            graph.AdjacencyMtx[5, 3] = true;
            graph.AdjacencyMtx[3, 7] = true;
            graph.AdjacencyMtx[7, 3] = true;
            var sparseGraph = new AdjListGraph(graph);
            var mask        = new BitArrayEx(8);
            mask[4] = true;
            mask[5] = true;
            mask[6] = true;
            mask[7] = true;
            Assert.AreEqual(4, func.Compute(sparseGraph, mask, 4));
        }

        [Test]
        public void MaximumMatchingTest2()
        {
            var func  = new MmCutFunction();
            var graph = new AdjMtxGraph(8);
            graph.AdjacencyMtx[0, 4] = true;
            graph.AdjacencyMtx[4, 0] = true;
            graph.AdjacencyMtx[1, 5] = true;
            graph.AdjacencyMtx[5, 1] = true;
            graph.AdjacencyMtx[0, 6] = true;
            graph.AdjacencyMtx[6, 0] = true;
            graph.AdjacencyMtx[0, 7] = true;
            graph.AdjacencyMtx[7, 0] = true;
            graph.AdjacencyMtx[1, 7] = true;
            graph.AdjacencyMtx[7, 1] = true;
            var sparseGraph = new AdjListGraph(graph);
            var mask        = new BitArrayEx(8);
            mask[4] = true;
            mask[5] = true;
            mask[6] = true;
            mask[7] = true;
            Assert.AreEqual(2, func.Compute(sparseGraph, mask, 4));
        }

        [Test]
        public void MaximumMatchingTest3()
        {
            var func  = new MmCutFunction();
            var graph = new AdjMtxGraph(6);
            graph.AdjacencyMtx[0, 2] = true;
            graph.AdjacencyMtx[2, 0] = true;
            graph.AdjacencyMtx[0, 3] = true;
            graph.AdjacencyMtx[3, 0] = true;
            graph.AdjacencyMtx[0, 5] = true;
            graph.AdjacencyMtx[5, 0] = true;
            graph.AdjacencyMtx[1, 2] = true;
            graph.AdjacencyMtx[2, 1] = true;
            graph.AdjacencyMtx[1, 3] = true;
            graph.AdjacencyMtx[3, 1] = true;
            graph.AdjacencyMtx[1, 4] = true;
            graph.AdjacencyMtx[4, 1] = true;
            graph.AdjacencyMtx[2, 4] = true;
            graph.AdjacencyMtx[4, 2] = true;
            graph.AdjacencyMtx[2, 5] = true;
            graph.AdjacencyMtx[5, 2] = true;
            var sparseGraph = new AdjListGraph(graph);
            var mask        = new BitArrayEx(6);
            mask[2] = true;
            mask[3] = true;
            mask[4] = true;
            mask[5] = true;
            Assert.AreEqual(2, func.Compute(sparseGraph, mask, 4));
            mask    = new BitArrayEx(6);
            mask[3] = true;
            mask[4] = true;
            mask[5] = true;
            Assert.AreEqual(3, func.Compute(sparseGraph, mask, 3));
        }
    }
}