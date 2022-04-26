namespace RankWidthApproximate.Data.Graph
{
    /// <summary>
    /// Abstract class representing a graph with labeled vertices
    /// </summary>
    public abstract class Graph
    {
        public readonly string[] VtxLabels;

        public int VtxCount { get; }

        public Graph(int vtxCount)
        {
            VtxCount  = vtxCount;
            VtxLabels = new string[vtxCount];
        }

        /// <summary>
        /// Checks if the graph has any edges
        /// </summary>
        /// <returns>True if the graph has any edges, false otherwise</returns>
        public abstract bool CheckHasEdges();
    }
}