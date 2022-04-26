namespace RankWidthApproximate.Data.Graph
{
    /// <summary>
    /// Directed graph with labeled vertices based on an adjacency matrix in GF(4)
    /// </summary>
    public class GF4DirectedGraph : Graph
    {
        public readonly GF4Matrix AdjacencyMtx;

        public GF4DirectedGraph(int vtxCount)
            : base(vtxCount)
        {
            AdjacencyMtx = new GF4Matrix(vtxCount, vtxCount);
        }

        public override bool CheckHasEdges()
        {
            for (int i = 0; i < VtxCount; i++)
            {
                for (int j = 0; j < VtxCount; j++)
                {
                    if (AdjacencyMtx[i, j] != GF4.Zero)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Makes every directed arc bi-directional
        /// </summary>
        public void MakeUndirected()
        {
            for (int i = 0; i < VtxCount; i++)
            {
                for (int j = 0; j < VtxCount; j++)
                {
                    if (AdjacencyMtx[i, j] == GF4.Zero)
                        continue;

                    AdjacencyMtx[i, j] = GF4.One;
                    AdjacencyMtx[j, i] = GF4.One;
                }
            }
        }

        /// <summary>
        /// Converts this graph to undirected by replacing every arc by an edge
        /// </summary>
        /// <returns>An undirected version of this graph</returns>
        public AdjMtxGraph ToUndirected()
        {
            var result = new AdjMtxGraph(VtxCount);
            VtxLabels.CopyTo(result.VtxLabels, 0);

            for (int i = 0; i < VtxCount; i++)
            {
                for (int j = 0; j < VtxCount; j++)
                {
                    if (AdjacencyMtx[i, j] == GF4.Zero)
                        continue;

                    result.AdjacencyMtx[i, j] = true;
                    result.AdjacencyMtx[j, i] = true;
                }
            }

            return result;
        }
    }
}