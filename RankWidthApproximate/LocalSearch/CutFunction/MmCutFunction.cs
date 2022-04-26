using System;
using System.Collections.Generic;
using RankWidthApproximate.Data;
using RankWidthApproximate.Data.Graph;

namespace RankWidthApproximate.LocalSearch.CutFunction
{
    /// <summary>
    /// Cut function for maximum matching-width
    /// </summary>
    public class MmCutFunction : ICutFunction
    {
        private bool Bfs(AdjListGraph graph, BitArrayEx mask, int[] pair, int[] dist)
        {
            var queue = new Queue<int>();
            for (int i = 0; i < graph.VtxCount; i++)
            {
                if (mask[i] != false) //if not in U
                    continue;
                if (pair[i] == graph.VtxCount) //if NIL
                {
                    dist[i] = 0;
                    queue.Enqueue(i);
                }
                else
                    dist[i] = int.MaxValue;
            }

            dist[graph.VtxCount] = int.MaxValue;

            while (queue.Count > 0)
            {
                int u = queue.Dequeue();
                if (dist[u] >= dist[graph.VtxCount])
                    continue;
                bool group = mask[u];
                foreach (ushort v in graph.Neighbors[u])
                {
                    if (mask[v] != !group) //if not in the right group
                        continue;
                    if (dist[pair[v]] != int.MaxValue)
                        continue;

                    dist[pair[v]] = dist[u] + 1;
                    queue.Enqueue(pair[v]);
                }
            }

            return dist[graph.VtxCount] != int.MaxValue;
        }

        private bool Dfs(AdjListGraph graph, BitArrayEx mask, int[] pair, int[] dist, int u)
        {
            if (u == graph.VtxCount) //if NIL
                return true;

            bool group = mask[u];
            foreach (ushort v in graph.Neighbors[u])
            {
                if (mask[v] != !group) //if not in the right group
                    continue;
                if (dist[pair[v]] != dist[u] + 1)
                    continue;

                if (Dfs(graph, mask, pair, dist, pair[v]))
                {
                    pair[v] = u;
                    pair[u] = v;
                    return true;
                }
            }

            dist[u] = int.MaxValue;
            return false;
        }

        public int Compute(Graph graph, BitArrayEx mask, int count)
        {
            if (graph is not AdjListGraph g)
                throw new Exception();

            //Hopcroft–Karp
            //Based on the pseudo-code at: https://en.wikipedia.org/wiki/Hopcroft%E2%80%93Karp_algorithm

            //mask[i] == false => U
            //mask[i] == true => V

            var pair = new int[graph.VtxCount + 1];
            Array.Fill(pair, graph.VtxCount);
            var dist = new int[graph.VtxCount + 1];

            int matching = 0;
            while (Bfs(g, mask, pair, dist))
            {
                for (int i = 0; i < graph.VtxCount; i++)
                {
                    if (mask[i] != false) //if not in U
                        continue;
                    if (pair[i] != graph.VtxCount) //if not NIL
                        continue;
                    if (Dfs(g, mask, pair, dist, i))
                        matching++;
                }
            }

            return matching;
        }
    }
}