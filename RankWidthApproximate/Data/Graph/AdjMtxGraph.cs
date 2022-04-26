namespace RankWidthApproximate.Data.Graph
{
    /// <summary>
    /// Undirected graph with labeled vertices based on an adjacency bit matrix
    /// </summary>
    public class AdjMtxGraph : Graph
    {
        public readonly BitMatrix AdjacencyMtx;

        public AdjMtxGraph(int vtxCount)
            : base(vtxCount)
        {
            AdjacencyMtx = new BitMatrix(vtxCount, vtxCount);
        }

        public override bool CheckHasEdges()
        {
            for (int i = 0; i < VtxCount; i++)
            {
                for (int j = 0; j < VtxCount; j++)
                {
                    if (AdjacencyMtx[i, j])
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Converts this graph to directed by introducing a bi-directional arc for every edge
        /// </summary>
        /// <returns>A directed version of this graph</returns>
        public GF4DirectedGraph ToDirected()
        {
            var result = new GF4DirectedGraph(VtxCount);

            for (int i = 0; i < VtxCount; i++)
            {
                for (int j = 0; j < VtxCount; j++)
                {
                    if (!AdjacencyMtx[i, j])
                        continue;

                    result.AdjacencyMtx[i, j] = GF4.One;
                    result.AdjacencyMtx[j, i] = GF4.One;
                }
            }

            return result;
        }
    }
}