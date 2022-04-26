using System;
using System.Diagnostics;
using RankWidthApproximate.Data;
using RankWidthApproximate.Data.Graph;

namespace RankWidthApproximate.LocalSearch.CutFunction
{
    /// <summary>
    /// Cut function for F4-rank-width
    /// </summary>
    public class F4CutRankFunction : ICutFunction
    {
        public int Compute(Graph graph, BitArrayEx mask, int count)
        {
            if (graph is not GF4DirectedGraph g)
                throw new Exception();

            bool isColumn = false;
            if (count > (graph.VtxCount >> 1))
            {
                count    = graph.VtxCount - count;
                isColumn = true;
            }

            var curMtx = new GF4Array[count];
            int r      = 0;
            for (int i = 0; i < graph.VtxCount; i++)
            {
                if (mask[i] != isColumn)
                    curMtx[r++] = new GF4Array(g.AdjacencyMtx[i]);
            }

            Debug.Assert(r == count);

            int curRow = 0;
            int curCol = 0;
            while (curRow < count && curCol < graph.VtxCount)
            {
                if (mask[curCol] != isColumn)
                {
                    curCol++;
                    continue;
                }

                bool success = false;
                for (int j = curRow; j < count; j++)
                {
                    if (curMtx[j][curCol] != GF4.Zero)
                    {
                        (curMtx[j], curMtx[curRow]) = (curMtx[curRow], curMtx[j]);
                        success                     = true;
                        break;
                    }
                }

                if (!success)
                {
                    //all-zero column
                    curCol++;
                    continue;
                }

                int pivot = curMtx[curRow][curCol];

                //clear all elements below the current
                for (int j = curRow + 1; j < count; j++)
                {
                    int entry = curMtx[j][curCol];
                    if (entry != GF4.Zero)
                    {
                        curMtx[j].MultiplyAdd(curMtx[curRow], GF4.Divide(entry, pivot));
                        Debug.Assert(curMtx[j][curCol] == GF4.Zero);
                    }
                }

                curRow++;
                curCol++;
            }

            return curRow;
        }
    }
}