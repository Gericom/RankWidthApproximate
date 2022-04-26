using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace RankWidthApproximate.Data
{
    /// <summary>
    /// A two dimensional matrix of GF4 values
    /// </summary>
    public class GF4Matrix
    {
        private readonly GF4Array[] _rows;

        public readonly int Rows;
        public readonly int Columns;

        /// <summary>
        /// Constructs a new GF4Matrix of the given dimensions
        /// </summary>
        /// <param name="rows">The number of rows</param>
        /// <param name="columns">The number of columns</param>
        public GF4Matrix(int rows, int columns)
        {
            Rows    = rows;
            Columns = columns;

            Debug.Assert(Rows > 0 && Columns > 0);

            _rows = new GF4Array[Rows];
            for (int i = 0; i < Rows; i++)
                _rows[i] = new GF4Array(Columns);
        }

        public int this[int row, int column]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _rows[row][column];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _rows[row][column] = value;
        }

        public GF4Array this[int row]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _rows[row];
        }
    }
}