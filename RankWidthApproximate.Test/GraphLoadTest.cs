using System;
using System.Linq;
using NUnit.Framework;
using RankWidthApproximate.Data;

namespace RankWidthApproximate.Test
{
    public class GraphLoadTest
    {
        [Test]
        public void GridGraph()
        {
            var graph = DimacsParser.ParseUndirected("grid5x5.dgf");
            Assert.AreEqual(25, graph.VtxCount);
            var mapping = Enumerable.Range(0, 25).Select(i => Array.IndexOf(graph.VtxLabels, "" + i)).ToArray();
            for (int i = 0; i < 25; i++)
            {
                if (i % 5 != 0)
                    Assert.IsTrue(graph.AdjacencyMtx[mapping[i], mapping[i - 1]]);
                if (i % 5 != 4)
                    Assert.IsTrue(graph.AdjacencyMtx[mapping[i], mapping[i + 1]]);

                if (i / 5 != 0)
                    Assert.IsTrue(graph.AdjacencyMtx[mapping[i], mapping[i - 5]]);
                if (i / 5 != 4)
                    Assert.IsTrue(graph.AdjacencyMtx[mapping[i], mapping[i + 5]]);
            }
        }
    }
}