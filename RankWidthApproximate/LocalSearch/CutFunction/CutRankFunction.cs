using System;
using System.Diagnostics;
using RankWidthApproximate.Data;
using RankWidthApproximate.Data.Graph;

namespace RankWidthApproximate.LocalSearch.CutFunction
{
    /// <summary>
    /// Cut function for rank-width using an adjacency matrix
    /// </summary>
    public class CutRankFunction : ICutFunction
    {
        public int Compute(Graph graph, BitArrayEx mask, int count)
        {
            if (graph is not AdjMtxGraph g)
                throw new Exception();

            bool isColumn = false;
            if (count > (graph.VtxCount >> 1))
            {
                count    = graph.VtxCount - count;
                isColumn = true;
            }

            var curMtx         = new BitArrayEx[count];
            var curMtxModified = new bool[count];
            int r              = 0;
            for (int i = 0; i < graph.VtxCount; i++)
            {
                if (mask[i] != isColumn)
                    curMtx[r++] = g.AdjacencyMtx[i];
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
                    if (curMtx[j][curCol])
                    {
                        (curMtx[j], curMtx[curRow])                 = (curMtx[curRow], curMtx[j]);
                        (curMtxModified[j], curMtxModified[curRow]) = (curMtxModified[curRow], curMtxModified[j]);
                        success                                     = true;
                        break;
                    }
                }

                if (!success)
                {
                    //all-zero column
                    curCol++;
                    continue;
                }

                //clear all elements below the current
                for (int j = curRow + 1; j < count; j++)
                {
                    if (curMtx[j][curCol])
                    {
                        if (!curMtxModified[j])
                        {
                            curMtx[j]         = BitArrayEx.Xor(curMtx[j], curMtx[curRow]);
                            curMtxModified[j] = true;
                        }
                        else
                            curMtx[j].Xor(curMtx[curRow], curCol);
                    }
                }

                curRow++;
                curCol++;
            }

            return curRow;
        }
    }
}