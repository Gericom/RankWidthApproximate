using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace RankWidthApproximate.Data
{
    /// <summary>
    /// A two dimensional matrix of bits
    /// </summary>
    public class BitMatrix
    {
        private readonly BitArrayEx[] _rows;

        public readonly int Rows;
        public readonly int Columns;

        /// <summary>
        /// Constructs an all-zero bit matrix of the given size
        /// </summary>
        /// <param name="rows">The number of rows</param>
        /// <param name="columns">The number of columns</param>
        public BitMatrix(int rows, int columns)
        {
            Rows    = rows;
            Columns = columns;

            Debug.Assert(Rows > 0 && Columns > 0);

            _rows = new BitArrayEx[Rows];
            for (int i = 0; i < Rows; i++)
                _rows[i] = new BitArrayEx(Columns);
        }

        /// <summary>
        /// Constructs a bit matrix from the given one dimensional bool array with the given number of columns per row
        /// </summary>
        /// <param name="matrix">The source matrix</param>
        /// <param name="columns">The number of columns in the source matrix</param>
        public BitMatrix(bool[] matrix, int columns)
        {
            Debug.Assert(columns > 0);

            Columns = columns;
            Rows    = matrix.Length / columns;
            Debug.Assert(Rows > 0 && Rows * Columns == matrix.Length);

            _rows = new BitArrayEx[Rows];
            for (int i = 0; i < Rows; i++)
                _rows[i] = new BitArrayEx(matrix[(i * columns)..((i + 1) * columns)]);
        }

        /// <summary>
        /// Gets or sets the bit at the given position
        /// </summary>
        /// <param name="row">The 0-based row index</param>
        /// <param name="column">The 0-based column index</param>
        /// <returns></returns>
        public bool this[int row, int column]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _rows[row][column];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _rows[row][column] = value;
        }

        /// <summary>
        /// Gets the given row
        /// </summary>
        /// <param name="row">The 0-based row index</param>
        /// <returns>The requested row as a bit array</returns>
        public BitArrayEx this[int row]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _rows[row];
        }
    }
}