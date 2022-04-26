using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using RankWidthApproximate.Data;
using RankWidthApproximate.LocalSearch;
using RankWidthApproximate.LocalSearch.CutFunction;

namespace RankWidthApproximate.Test
{
    public class PartitionTest
    {
        [Test]
        public void AllEdges([Values("grid5x5.dgf", "d1655.tsp.dgf", "celar07.dgf")] string graphName)
        {
            var graph = DimacsParser.ParseUndirected(graphName);
            var sc    = new SearchContext(graph, new CutRankFunction());

            var edges = new HashSet<(DecompositionNode, DecompositionNode)>();

            var visited = new HashSet<DecompositionNode>();

            var queue = new Queue<DecompositionNode>();
            queue.Enqueue(sc.Decomposition.TreeLeaves[0]);
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                if (visited.Contains(node))
                    continue;
                visited.Add(node);

                foreach (var neighbor in node.Neighbors)
                {
                    if (visited.Contains(neighbor))
                        continue;
                    edges.Add((node, neighbor));
                    queue.Enqueue(neighbor);
                }
            }

            sc.Decomposition.FindEdgePartitions((partition, count, a, b) =>
            {
                Assert.IsTrue(edges.Contains((a, b)) || edges.Contains((b, a)));
                edges.Remove((a, b));
                edges.Remove((b, a));
            });

            Assert.AreEqual(0, edges.Count);
        }

        [Test]
        public void CorrectPartitions([Values("grid5x5.dgf", "celar07.dgf")] string graphName)
        {
            var graph = DimacsParser.ParseUndirected(graphName);
            var sc    = new SearchContext(graph, new CutRankFunction());

            sc.Decomposition.FindEdgePartitions((partition, count, a, b) =>
            {
                var realPartition = new BitArrayEx(sc.Decomposition.TreeLeaves.Length);

                for (int i = 0; i < sc.Decomposition.TreeLeaves.Length; i++)
                {
                    if (a.IsReachable(sc.Decomposition.TreeLeaves[i], b))
                        realPartition[i] = true;
                }

                var invPartition = new BitArrayEx(realPartition).Not();

                Assert.IsTrue(partition == realPartition || partition == invPartition);
            });
        }
    }
}