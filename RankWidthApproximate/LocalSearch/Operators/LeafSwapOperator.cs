using RankWidthApproximate.Data;

namespace RankWidthApproximate.LocalSearch.Operators
{
    /// <summary>
    /// Operator that swaps two random leaf nodes
    /// </summary>
    public class LeafSwapOperator : ISearchOperator
    {
        private DecompositionNode _leafANode;
        private DecompositionNode _leafBNode;

        public void Perform(SearchContext context)
        {
            var leafA = context.GetRandomLeafNode();
            var leafB = context.GetRandomLeafNode(leafA);

            (leafA.GraphVtxId, leafB.GraphVtxId) = (leafB.GraphVtxId, leafA.GraphVtxId);

            context.RecalculateScore();

            _leafANode = leafA;
            _leafBNode = leafB;
        }

        public void Undo(SearchContext context)
        {
            (_leafANode.GraphVtxId, _leafBNode.GraphVtxId) = (_leafBNode.GraphVtxId, _leafANode.GraphVtxId);
        }
    }
}