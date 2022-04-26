using RankWidthApproximate.Data;
using RankWidthApproximate.Data.Graph;

namespace RankWidthApproximate.LocalSearch.CutFunction
{
    public interface ICutFunction
    {
        /// <summary>
        /// Computes the cut function for the given partition on the given graph
        /// </summary>
        /// <param name="graph">The graph</param>
        /// <param name="mask">The partition of the vertices</param>
        /// <param name="count">The number of ones in the partition</param>
        /// <returns>The value of the cut function for the partition</returns>
        public int Compute(Graph graph, BitArrayEx mask, int count);
    }
}