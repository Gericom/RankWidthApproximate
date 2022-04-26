using System.Linq;
using RankWidthApproximate.Data;

namespace RankWidthApproximate.LocalSearch.Operators
{
    public class MoveOperator : ISearchOperator
    {
        private DecompositionNode _nodeA;
        private DecompositionNode _nodeB;
        private DecompositionNode _nodeANeigh;
        private DecompositionNode _nodeANeighX;
        private DecompositionNode _nodeANeighY;
        private DecompositionNode _nodeBNeigh;

        public void Perform(SearchContext context)
        {
            while (true)
            {
                var nodeA = context.GetRandomNode();
                var nodeB = context.GetRandomNode(nodeA);

                if (nodeA.Neighbors.Contains(nodeB))
                    continue;

                var (nodeANeigh, nodeBNeigh) = nodeA.FindPathDirection(nodeB);

                if (nodeANeigh == null || nodeBNeigh == null)
                    continue;

                if (nodeANeigh.Neighbors.Count != 3)
                    continue;

                if (nodeANeigh == nodeBNeigh)
                    continue;

                nodeANeigh.RemoveNeighbor(nodeA);

                var nodeANeighX = nodeANeigh.Neighbors[0];
                var nodeANeighY = nodeANeigh.Neighbors[1];

                nodeANeigh.RemoveNeighbor(nodeANeighX);
                nodeANeigh.RemoveNeighbor(nodeANeighY);
                nodeANeighX.AddNeighbor(nodeANeighY);

                nodeBNeigh.RemoveNeighbor(nodeB);

                nodeANeigh.AddNeighbor(nodeBNeigh);
                nodeANeigh.AddNeighbor(nodeB);
                nodeANeigh.AddNeighbor(nodeA);

                context.RecalculateScore();

                _nodeA       = nodeA;
                _nodeB       = nodeB;
                _nodeANeigh  = nodeANeigh;
                _nodeANeighX = nodeANeighX;
                _nodeANeighY = nodeANeighY;
                _nodeBNeigh  = nodeBNeigh;

                return;
            }
        }

        public void Undo(SearchContext context)
        {
            _nodeANeigh.RemoveNeighbor(_nodeA);
            _nodeANeigh.RemoveNeighbor(_nodeB);
            _nodeANeigh.RemoveNeighbor(_nodeBNeigh);

            _nodeBNeigh.AddNeighbor(_nodeB);

            _nodeANeighX.RemoveNeighbor(_nodeANeighY);

            _nodeANeigh.AddNeighbor(_nodeANeighY);
            _nodeANeigh.AddNeighbor(_nodeANeighX);
            _nodeANeigh.AddNeighbor(_nodeA);
        }
    }
}