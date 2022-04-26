using System.Collections.Generic;

namespace RankWidthApproximate.Data.Graph
{
    /// <summary>
    /// Undirected graph with labeled vertices based on adjacency lists
    /// </summary>
    public class AdjListGraph : Graph
    {
        public readonly ushort[][] Neighbors;

        public AdjListGraph(AdjMtxGraph graph)
            : base(graph.VtxCount)
        {
            graph.VtxLabels.CopyTo(VtxLabels, 0);
            Neighbors = new ushort[graph.VtxCount][];
            for (int i = 0; i < graph.VtxCount; i++)
            {
                var tmp = new List<ushort>();
                for (int j = 0; j < graph.VtxCount; j++)
                {
                    if (!graph.AdjacencyMtx[i, j])
                        continue;
                    tmp.Add((ushort)j);
                }

                tmp.Sort();
                Neighbors[i] = tmp.ToArray();
            }
        }

        public override bool CheckHasEdges()
        {
            for (int i = 0; i < VtxCount; i++)
            {
                if (Neighbors.Length > 0)
                    return true;
            }

            return false;
        }
    }
}