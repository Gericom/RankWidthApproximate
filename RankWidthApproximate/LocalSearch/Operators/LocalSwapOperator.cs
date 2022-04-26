using RankWidthApproximate.Data;

namespace RankWidthApproximate.LocalSearch.Operators
{
    public class LocalSwapOperator : ISearchOperator
    {
        private DecompositionNode _centerNode;
        private DecompositionNode _nodeA;
        private DecompositionNode _nodeB;
        private DecompositionNode _neigh2Node;

        public void Perform(SearchContext context)
        {
            DecompositionNode centerNode, nodeA, nodeB;
            do
            {
                centerNode = context.GetRandomInnerNode();

                int a = context.Random.Next(3);
                int b = (a + 1 + context.Random.Next(2)) % 3;

                nodeA = centerNode.Neighbors[a];
                nodeB = centerNode.Neighbors[b];
            } while (nodeA.IsLeaf && nodeB.IsLeaf);

            //ensure nodeA is a non-leaf
            if (nodeB.IsLeaf)
                (nodeA, nodeB) = (nodeB, nodeA);

            int neigh2 = context.Random.Next(nodeB.Neighbors.Count - 1);
            if (nodeB.Neighbors[neigh2] == centerNode)
                neigh2 = nodeB.Neighbors.Count - 1;

            var neigh2Node = nodeB.Neighbors[neigh2];

            var oldPart = context.Decomposition.GetPartition(centerNode, nodeB);

            nodeB.RemoveNeighbor(neigh2Node);
            centerNode.RemoveNeighbor(nodeA);
            nodeB.AddNeighbor(nodeA);
            centerNode.AddNeighbor(neigh2Node);

            var newPart = context.Decomposition.GetPartition(centerNode, nodeB);

            int oldWidth;

            long newScore = context.Score;
            int  oldMin   = context.Graph.VtxCount - oldPart.count;
            if (oldPart.count < oldMin)
                oldMin = oldPart.count;
            if (context.UseThresholdHeuristic && oldMin <= context.Width - context.ThresholdDelta)
                oldWidth = oldMin;
            else
                oldWidth = context.CalculateCutWidth(oldPart.partition, oldPart.count);
            newScore -= (long)oldWidth * oldWidth;

            int newWidth;

            int newMin = context.Graph.VtxCount - newPart.count;
            if (newPart.count < newMin)
                newMin = newPart.count;
            if (context.UseThresholdHeuristic && newMin <= context.Width - context.ThresholdDelta)
                newWidth = newMin;
            else
                newWidth = context.CalculateCutWidth(newPart.partition, newPart.count);
            newScore += (long)newWidth * newWidth;

            if ((oldWidth <= context.OldWidth && newWidth > context.OldWidth) ||
                (oldWidth == context.OldWidth && newWidth < context.OldWidth))
                context.RecalculateScore();
            else
                context.Score = newScore;

            _centerNode = centerNode;
            _nodeA      = nodeA;
            _nodeB      = nodeB;
            _neigh2Node = neigh2Node;
        }

        public void Undo(SearchContext context)
        {
            _centerNode.RemoveNeighbor(_neigh2Node);
            _nodeB.RemoveNeighbor(_nodeA);
            _centerNode.AddNeighbor(_nodeA);
            _nodeB.AddNeighbor(_neigh2Node);
        }
    }
}