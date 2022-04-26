using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RankWidthApproximate.Data;
using RankWidthApproximate.Data.Graph;

namespace RankWidthApproximate.LocalSearch.CutFunction
{
    /// <summary>
    /// Cut function for rank-width using adjacency lists
    /// </summary>
    public class SparseCutRankFunction : ICutFunction
    {
        private class MatrixRow
        {
            private ushort[] _entries;
            private int      _count;

            public int Count => _count;

            public ushort First => _entries[0];

            public MatrixRow(IEnumerable<ushort> entries)
            {
                _entries = entries.ToArray();
                Array.Sort(_entries);
                _count = _entries.Length;
            }

            public MatrixRow(ushort[] entries, BitArrayEx mask, bool isColumn)
            {
                _entries = new ushort[entries.Length];
                int i = 0;
                foreach (ushort entry in entries)
                {
                    if (mask[entry] != isColumn)
                        continue;
                    _entries[i++] = entry;
                }

                _count = i;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool FirstEquals(ushort entry)
            {
                return _count > 0 && _entries[0] == entry;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool FirstLess(ushort entry)
            {
                return _count > 0 && _entries[0] < entry;
            }

            public bool Contains(ushort entry)
            {
                if (_count == 0)
                    return false;

                int left  = 0;
                int right = _count - 1;
                while (left < right)
                {
                    int mid = (left + right) >> 1;
                    if (_entries[mid] < entry)
                        left = mid + 1;
                    else
                        right = mid;
                }

                return _entries[right] == entry;
            }

            [MethodImpl(MethodImplOptions.AggressiveOptimization)]
            public void Xor(MatrixRow b)
            {
                var tmp  = new ushort[_count + b._count];
                int i    = 0;
                int aIdx = 0;
                int bIdx = 0;
                while (aIdx < _count && bIdx < b._count)
                {
                    if (_entries[aIdx] < b._entries[bIdx])
                        tmp[i++] = _entries[aIdx++];
                    else if (b._entries[bIdx] < _entries[aIdx])
                        tmp[i++] = b._entries[bIdx++];
                    else
                    {
                        aIdx++;
                        bIdx++;
                    }
                }

                while (aIdx < _count)
                    tmp[i++] = _entries[aIdx++];

                while (bIdx < b._count)
                    tmp[i++] = b._entries[bIdx++];

                _entries = tmp;
                _count   = i;
            }
        }

        public int Compute(Graph graph, BitArrayEx mask, int count)
        {
            if (graph is not AdjListGraph g)
                throw new Exception();

            bool isColumn = false;
            if (count > (graph.VtxCount >> 1))
            {
                count    = graph.VtxCount - count;
                isColumn = true;
            }

            count    = graph.VtxCount - count;
            isColumn = !isColumn;

            var curMtx = new MatrixRow[count];
            int r      = 0;
            for (int i = 0; i < graph.VtxCount; i++)
            {
                if (mask[i] == isColumn)
                    continue;

                curMtx[r] = new MatrixRow(g.Neighbors[i], mask, isColumn);

                if (curMtx[r].Count > 0)
                    r++;
            }

            count = r;

            var colRows = new MatrixRow[graph.VtxCount];

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
                    while (curMtx[j].FirstLess((ushort)curCol))
                    {
                        curMtx[j].Xor(colRows[curMtx[j].First]);
                    }

                    if (curMtx[j].FirstEquals((ushort)curCol))
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

                colRows[curCol] = curMtx[curRow];

                curRow++;
                curCol++;
            }

            return curRow;
        }
    }
}