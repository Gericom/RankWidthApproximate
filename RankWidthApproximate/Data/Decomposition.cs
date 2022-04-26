using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using RankWidthApproximate.LocalSearch;

namespace RankWidthApproximate.Data
{
    /// <summary>
    /// Class representing a branch-decomposition with graph vertices as ground-set
    /// </summary>
    public class Decomposition
    {
        private long _uniqueId = 0;

        public Graph.Graph             Graph      { get; }
        public DecompositionNode[]     TreeLeaves { get; }
        public List<DecompositionNode> TreeNodes  { get; } = new();

        public Decomposition(Graph.Graph graph)
        {
            Graph      = graph;
            TreeLeaves = new DecompositionNode[Graph.VtxCount];
            for (int i = 0; i < Graph.VtxCount; i++)
                TreeLeaves[i] = new DecompositionNode(GetUniqueId(), i);
        }

        /// <summary>
        /// Compute a unique node id
        /// </summary>
        /// <returns>A unique node id</returns>
        public long GetUniqueId() => _uniqueId++;

        /// <summary>
        /// Constructs the name of a node to be used in a dot graph
        /// </summary>
        /// <param name="node">The node to construct a name for</param>
        /// <returns>A name for the given node</returns>
        private string GetNodeName(DecompositionNode node)
        {
            if (node.IsLeaf)
                return $"leaf_{Graph.VtxLabels[node.GraphVtxId]}";
            return $"node_{node.UniqueId}";
        }

        /// <summary>
        /// Converts the decomposition to dot graph format
        /// </summary>
        /// <param name="context">The search context</param>
        /// <returns>A string representing this decomposition in dot graph format</returns>
        public string ToDotGraph(SearchContext context)
        {
            var builder = new StringBuilder();
            builder.AppendLine("graph decomposition {");
            FindEdgePartitions((part, count, nodeA, nodeB) =>
            {
                int width = context.CalculateCutWidth(part, count);
                builder.AppendLine($"{GetNodeName(nodeA)} -- {GetNodeName(nodeB)} [label={width}]");
            });
            builder.AppendLine("}");
            return builder.ToString();
        }

        /// <summary>
        /// Finds the partitions for all edges in the decomposition.
        /// For edges for which the smallest partition side is at most the given threshold
        /// only the size is reported to save time.
        /// </summary>
        /// <param name="func">Callback function called for every decomposition edge</param>
        /// <param name="threshold">Only size is reported for partitions with smallest side at most this threshold</param>
        public void FindEdgePartitions(Action<BitArrayEx, int, DecompositionNode, DecompositionNode> func,
            int threshold = -1)
        {
            int curId  = 0;
            var vtxIds = new int[TreeLeaves.Length];
            var stack  = new Stack<(DecompositionNode node, DecompositionNode parent, bool childrenDone)>();
            stack.Push((TreeLeaves[0], null, false));
            var array        = new BitArrayEx(TreeLeaves.Length);
            int arrayStartId = -1;
            int arrayEndId   = -1;
            while (stack.Count > 0)
            {
                var (node, parent, childrenDone) = stack.Pop();
                if (childrenDone)
                {
                    node.PartitionEndId = curId - 1;

                    for (int j = 0; j < node.Neighbors.Count; j++)
                    {
                        var neighbor = node.Neighbors[j];
                        if (neighbor == parent)
                            continue;

                        int count    = neighbor.PartitionEndId - neighbor.PartitionStartId + 1;
                        int minCount = TreeLeaves.Length - count;
                        if (count < minCount)
                            minCount = count;

                        if (minCount <= threshold)
                        {
                            func(null, count, node, neighbor);
                            continue;
                        }

                        if (neighbor.PartitionStartId <= arrayStartId && neighbor.PartitionEndId >= arrayEndId)
                        {
                            for (int i = neighbor.PartitionStartId; i < arrayStartId; i++)
                                array[vtxIds[i]] = true;
                            for (int i = arrayEndId + 1; i <= neighbor.PartitionEndId; i++)
                                array[vtxIds[i]] = true;
                        }
                        else
                        {
                            array.SetAll(false);

                            for (int i = neighbor.PartitionStartId; i <= neighbor.PartitionEndId; i++)
                                array[vtxIds[i]] = true;
                        }

                        arrayStartId = neighbor.PartitionStartId;
                        arrayEndId   = neighbor.PartitionEndId;

                        func(array, count, node, neighbor);
                    }
                }
                else
                {
                    node.PartitionStartId = curId;

                    if (node.IsLeaf)
                        vtxIds[curId++] = node.GraphVtxId;

                    stack.Push((node, parent, true));
                    for (int i = 0; i < node.Neighbors.Count; i++)
                    {
                        var neighbor = node.Neighbors[i];
                        if (neighbor != parent)
                            stack.Push((neighbor, node, false));
                    }
                }
            }
        }

        /// <summary>
        /// Computes the partition for the edge between nodeA and nodeB, which are assumed to be neighbors
        /// </summary>
        /// <param name="nodeA">One side of the edge</param>
        /// <param name="nodeB">Other side of the edge</param>
        /// <returns>The partition of the edge and the number of ones in the bit vector</returns>
        public (BitArrayEx partition, int count) GetPartition(DecompositionNode nodeA, DecompositionNode nodeB)
        {
            Debug.Assert(nodeA.Neighbors.Contains(nodeB));
            var result = new BitArrayEx(TreeLeaves.Length);
            int count  = 0;

            var queue = new Queue<(DecompositionNode node, DecompositionNode parent)>();
            queue.Enqueue((nodeB, nodeA));

            while (queue.Count > 0)
            {
                var (node, parent) = queue.Dequeue();

                if (node.IsLeaf)
                {
                    result[node.GraphVtxId] = true;
                    count++;
                    continue;
                }

                for (int i = 0; i < node.Neighbors.Count; i++)
                {
                    var neighbor = node.Neighbors[i];
                    if (neighbor == parent)
                        continue;
                    queue.Enqueue((neighbor, node));
                }
            }

            return (result, count);
        }
    }
}