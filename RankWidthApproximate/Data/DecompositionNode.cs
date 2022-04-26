using System.Collections.Generic;
using System.Diagnostics;

namespace RankWidthApproximate.Data
{
    /// <summary>
    /// Node of a branch-decomposition tree
    /// </summary>
    public class DecompositionNode
    {
        private readonly List<DecompositionNode> _neighbors = new(3);

        public IReadOnlyList<DecompositionNode> Neighbors => _neighbors;

        public int  GraphVtxId { get; set; }
        public long UniqueId   { get; }

        public int  Degree => _neighbors.Count;
        public bool IsLeaf => GraphVtxId != -1;

        public int PartitionStartId;
        public int PartitionEndId;

        /// <summary>
        /// Constructs a new internal node
        /// </summary>
        /// <param name="uniqueId">A unique id for this node</param>
        public DecompositionNode(long uniqueId)
        {
            GraphVtxId = -1;
            UniqueId   = uniqueId;
        }

        /// <summary>
        /// Constructs a new leaf node
        /// </summary>
        /// <param name="uniqueId">A unique id for this leaf node</param>
        /// <param name="graphVtxId">Graph vertex id this leaf corresponds to</param>
        public DecompositionNode(long uniqueId, int graphVtxId)
        {
            Debug.Assert(graphVtxId >= 0);
            GraphVtxId = graphVtxId;
            UniqueId   = uniqueId;
        }

        /// <summary>
        /// True if this node has space for more neighbors
        /// </summary>
        public bool CanAddNeighbor => (IsLeaf && _neighbors.Count == 0) || _neighbors.Count < 3;

        public override string ToString() => IsLeaf ? "Leaf " + GraphVtxId : "Node";

        /// <summary>
        /// Makes the given node a neighbor if possible
        /// </summary>
        /// <param name="node">The neighbor to add</param>
        /// <returns>True if adding the neighbor was successful</returns>
        public bool AddNeighbor(DecompositionNode node)
        {
            if (!CanAddNeighbor || !node.CanAddNeighbor)
                return false;

            _neighbors.Add(node);
            node._neighbors.Add(this);

            return true;
        }

        /// <summary>
        /// Removes the given neighbor
        /// </summary>
        /// <param name="node">The neighbor to remove</param>
        public void RemoveNeighbor(DecompositionNode node)
        {
            _neighbors.Remove(node);
            node._neighbors.Remove(this);
        }

        /// <summary>
        /// Computes if the given node is reachable from the current node, optionally ignoring one neighbor
        /// </summary>
        /// <param name="dst">The destination node</param>
        /// <param name="ignoreNeighbor">An optional neighbor of this to ignore</param>
        /// <returns>True if dst is reachable</returns>
        public bool IsReachable(DecompositionNode dst, DecompositionNode ignoreNeighbor = null)
        {
            if (dst == this)
                return true;

            var queue = new Queue<(DecompositionNode node, DecompositionNode parent)>();
            foreach (var neighbor in Neighbors)
            {
                if (neighbor == ignoreNeighbor)
                    continue;
                queue.Enqueue((neighbor, this));
            }

            while (queue.Count > 0)
            {
                var (node, parent) = queue.Dequeue();
                if (node == dst)
                    return true;
                for (int i = 0; i < node.Neighbors.Count; i++)
                {
                    var neighbor = node.Neighbors[i];
                    if (neighbor == parent)
                        continue;
                    queue.Enqueue((neighbor, node));
                }
            }

            return false;
        }

        /// <summary>
        /// Finds the path between this and dst and returns the neighbors of this and dst on the path
        /// </summary>
        /// <param name="dst">The destination node</param>
        /// <returns>The neighbors of this and dst on the path between this and dst</returns>
        public (DecompositionNode srcNeigh, DecompositionNode dstNeigh) FindPathDirection(DecompositionNode dst)
        {
            if (dst == this)
                return (null, null);

            var queue = new Queue<(DecompositionNode node, DecompositionNode parent, DecompositionNode srcNeigh)>();
            foreach (var neighbor in Neighbors)
                queue.Enqueue((neighbor, this, neighbor));

            while (queue.Count > 0)
            {
                var (node, parent, srcNeigh) = queue.Dequeue();
                if (node == dst)
                    return (srcNeigh, parent);
                for (int i = 0; i < node.Neighbors.Count; i++)
                {
                    var neighbor = node.Neighbors[i];
                    if (neighbor == parent)
                        continue;
                    queue.Enqueue((neighbor, node, srcNeigh));
                }
            }

            return (null, null);
        }
    }
}