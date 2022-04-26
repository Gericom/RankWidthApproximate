using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RankWidthApproximate.Data;
using RankWidthApproximate.Data.Graph;
using RankWidthApproximate.LocalSearch.CutFunction;

namespace RankWidthApproximate.LocalSearch
{
    public class SearchContext
    {
        public Decomposition Decomposition { get; }

        public Graph Graph { get; }

        private readonly ICutFunction _cutFunction;

        public long       OldScore { get; private set; }
        public int        OldWidth { get; private set; }
        public BitArrayEx OldWorst { get; private set; }

        public long       Score { get; set; }
        public int        Width { get; set; }
        public BitArrayEx Worst { get; set; }

        public long   BestScore              { get; set; }
        public int    BestWidth              { get; set; }
        public string BestDecomposition      { get; set; }
        public long   BestDecompositionScore { get; set; }

        public Random Random { get; }

        public bool UseThresholdHeuristic { get; }
        public int  ThresholdDelta        { get; }

#if CACHE_HISTOGRAM
        public readonly int[] CacheHistogram = new int[20];
#endif

        private readonly LruCache<BitArrayEx, int> _cutWidthCache = new(16 * 1024);

        public SearchContext(Graph graph, ICutFunction cutFunction, int thresholdDelta = -1, int seed = -1,
            int initialSolutionSeed = -1)
        {
            UseThresholdHeuristic = thresholdDelta >= 0;
            ThresholdDelta        = thresholdDelta;
            Graph                 = graph;
            _cutFunction          = cutFunction;
            Decomposition         = new Decomposition(graph);
            Random                = seed != -1 ? new Random(seed) : new Random();

            MakeInitialSolution(initialSolutionSeed);
            //faster initial calculation in case the heuristic is used by guessing the current width to be an upper bound
            Width = (int)Math.Ceiling(graph.VtxCount / 3.0);
            RecalculateScore();

            BestWidth = Width;
            BestScore = Score;
        }

        /// <summary>
        /// Constructs an initial solution such that its width is at most ceil(n/3) with n the number of vertices in the graph
        /// </summary>
        /// <param name="initialSolutionSeed"></param>
        private void MakeInitialSolution(int initialSolutionSeed)
        {
            var rand = initialSolutionSeed == -1 ? Random : new Random(initialSolutionSeed);

            var leaves = new List<DecompositionNode>(Decomposition.TreeLeaves);

            var vtx = new DecompositionNode(Decomposition.GetUniqueId());
            Decomposition.TreeNodes.Add(vtx);
            for (int i = 0; i < Graph.VtxCount / 3; i++)
            {
                if (vtx.Degree == 2)
                {
                    var newVtx = new DecompositionNode(Decomposition.GetUniqueId());
                    Decomposition.TreeNodes.Add(newVtx);
                    newVtx.AddNeighbor(vtx);
                    vtx = newVtx;
                }

                int leaf = rand.Next(leaves.Count);
                vtx.AddNeighbor(leaves[leaf]);
                leaves.RemoveAt(leaf);
            }

            var center = vtx;
            if (center.Degree == 2)
            {
                center = new DecompositionNode(Decomposition.GetUniqueId());
                Decomposition.TreeNodes.Add(center);
                center.AddNeighbor(vtx);
            }

            vtx = new DecompositionNode(Decomposition.GetUniqueId());
            Decomposition.TreeNodes.Add(vtx);
            for (int i = Graph.VtxCount / 3; i < 2 * Graph.VtxCount / 3; i++)
            {
                if (vtx.Degree == 2)
                {
                    var newVtx = new DecompositionNode(Decomposition.GetUniqueId());
                    Decomposition.TreeNodes.Add(newVtx);
                    newVtx.AddNeighbor(vtx);
                    vtx = newVtx;
                }

                int leaf = rand.Next(leaves.Count);
                vtx.AddNeighbor(leaves[leaf]);
                leaves.RemoveAt(leaf);
            }

            center.AddNeighbor(vtx);

            vtx = new DecompositionNode(Decomposition.GetUniqueId());
            Decomposition.TreeNodes.Add(vtx);
            for (int i = 2 * Graph.VtxCount / 3; i < Graph.VtxCount; i++)
            {
                if (vtx.Degree == 2)
                {
                    var newVtx = new DecompositionNode(Decomposition.GetUniqueId());
                    Decomposition.TreeNodes.Add(newVtx);
                    newVtx.AddNeighbor(vtx);
                    vtx = newVtx;
                }

                int leaf = rand.Next(leaves.Count);
                vtx.AddNeighbor(leaves[leaf]);
                leaves.RemoveAt(leaf);
            }

            center.AddNeighbor(vtx);
        }

        public void RecalculateScore()
        {
            int computationThreshold;
            int miss          = 0;
            int thresholdSkip = 0;
            int hit           = 0;
            int lookups       = 0;
            int oldWidth;
            do
            {
                var partitions = new List<(BitArrayEx, int)>();

                computationThreshold = UseThresholdHeuristic ? Width - ThresholdDelta : 1;
                if (computationThreshold < 1)
                    computationThreshold = 1;

                long       score     = 0;
                int        width     = computationThreshold;
                BitArrayEx worst     = null;
                int        edgeCount = 0;

                Decomposition.FindEdgePartitions((part, count, nodeA, nodeB) =>
                {
                    if (count > 0)
                        edgeCount++;

                    int minCount = Graph.VtxCount - count;
                    if (count < minCount)
                        minCount = count;

                    if (minCount <= computationThreshold)
                    {
                        thresholdSkip++;
                        score += (long)minCount * minCount;
                        return;
                    }

                    if (part is null)
                        return;

                    lookups++;
                    int rank;
                    if (_cutWidthCache.TryGet(part, out rank))
                    {
                        hit++;
                        score += (long)rank * rank;
                        if (rank >= width)
                        {
                            width = rank;
                            worst = new BitArrayEx(part);
                        }
                    }
                    else
                    {
                        miss++;
                        partitions.Add((new BitArrayEx(part), count));
                    }
                }, computationThreshold);

                object lockObj = new();
                Parallel.ForEach(partitions,
                    () => (score: (long)0, width: width, worst: (BitArrayEx)null),
                    (input, loopState, idk, local) =>
                    {
                        int rank = _cutFunction.Compute(Graph, input.Item1, input.Item2);
                        lock (lockObj)
                            _cutWidthCache.Add(input.Item1, rank);

                        local.score += (long)rank * rank;
                        if (rank > local.width)
                        {
                            local.width = rank;
                            local.worst = input.Item1;
                        }

                        return local;
                    }, local =>
                    {
                        lock (lockObj)
                        {
                            score += local.score;
                            if (local.width >= width)
                            {
                                width = local.width;
                                worst = local.worst;
                            }
                        }
                    });

                score += (long)width * width * Graph.VtxCount;

                oldWidth = Width;

                Score = score;
                Width = width;
                Worst = worst;
                // Console.WriteLine($"Cache hit: {hit * 100 / (float)edgeCount}%");
            } while (UseThresholdHeuristic && ((Width > 1 && computationThreshold == Width) || Width < oldWidth));

            // Console.WriteLine($"tres/hit/miss: {thresholdSkip}/{hit}/{miss}");
#if CACHE_HISTOGRAM
            int category = (int)Math.Round(hit * 20f / lookups);
            if (category == 20)
                category = 19;
            CacheHistogram[category]++;
#endif
        }

        /// <summary>
        /// Computes the width of the cut represented by the given partition, making use of the cache
        /// </summary>
        /// <param name="partition">The partition representing the cut</param>
        /// <param name="count">The number of ones in the partition</param>
        /// <returns>The width belonging to the given partition</returns>
        public int CalculateCutWidth(BitArrayEx partition, int count)
        {
            int width;
            if (_cutWidthCache.TryGet(partition, out width))
                return width;

            width = _cutFunction.Compute(Graph, partition, count);
            _cutWidthCache.Add(new BitArrayEx(partition), width);

            return width;
        }

        /// <summary>
        /// Store the current score
        /// </summary>
        public void StoreScore()
        {
            OldScore = Score;
            OldWidth = Width;
            OldWorst = Worst;
        }

        /// <summary>
        /// Restore the previously stored score
        /// </summary>
        public void RestoreScore()
        {
            Score = OldScore;
            Width = OldWidth;
            Worst = OldWorst;
        }

        public DecompositionNode GetRandomInnerNode(DecompositionNode forbidden = null)
        {
            int rand;
            if (forbidden != null)
            {
                rand = Random.Next(Decomposition.TreeNodes.Count - 1);
                if (Decomposition.TreeNodes[rand] == forbidden)
                    rand = Decomposition.TreeNodes.Count - 1;
            }
            else
                rand = Random.Next(Decomposition.TreeNodes.Count);

            return Decomposition.TreeNodes[rand];
        }

        public DecompositionNode GetRandomLeafNode(DecompositionNode forbidden = null)
        {
            int rand;
            if (forbidden != null)
            {
                rand = Random.Next(Decomposition.TreeLeaves.Length - 1);
                if (Decomposition.TreeLeaves[rand] == forbidden)
                    rand = Decomposition.TreeLeaves.Length - 1;
            }
            else
                rand = Random.Next(Decomposition.TreeLeaves.Length);

            return Decomposition.TreeLeaves[rand];
        }

        public DecompositionNode GetRandomNode(DecompositionNode forbidden = null)
        {
            int rand = Random.Next(Decomposition.TreeLeaves.Length + Decomposition.TreeNodes.Count);
            if (rand < Decomposition.TreeLeaves.Length)
            {
                if (Decomposition.TreeLeaves[rand] == forbidden)
                {
                    rand++;
                    if (rand >= Decomposition.TreeLeaves.Length)
                        rand = 0;
                }

                return Decomposition.TreeLeaves[rand];
            }

            rand -= Decomposition.TreeLeaves.Length;

            if (Decomposition.TreeNodes[rand] == forbidden)
            {
                rand++;
                if (rand >= Decomposition.TreeNodes.Count)
                    rand = 0;
            }

            return Decomposition.TreeNodes[rand];
        }
    }
}