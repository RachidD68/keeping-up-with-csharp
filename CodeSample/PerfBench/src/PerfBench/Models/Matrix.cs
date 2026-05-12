namespace PerfBench.Models;

// ╔══════════════════════════════════════════════════════════╗
// ║  Matrix — a simple matrix type for demonstrating        ║
// ║  Span<T>, Memory<T>, and ArrayPool<T> usage.            ║
// ╚══════════════════════════════════════════════════════════╝

/// <summary>
/// A 2D matrix backed by a flat array — demonstrates
/// array pooling, span slicing, and memory management.
/// </summary>
public sealed class Matrix : IDisposable
{
    private double[] _data;
    private readonly bool _pooled;
    private bool _disposed;

    public int Rows { get; }
    public int Cols { get; }

    /// <summary>
    /// Creates a matrix with pooled backing array.
    /// The array is rented from ArrayPool and returned on Dispose.
    /// </summary>
    public Matrix(int rows, int cols, bool usePool = true)
    {
        Rows = rows;
        Cols = cols;
        _pooled = usePool;

        _data = usePool
            ? ArrayPool<double>.Shared.Rent(rows * cols)
            : new double[rows * cols];

        // Clear the rented array (may contain stale data)
        AsSpan().Clear();
    }

    /// <summary>Gets/sets an element by row and column.</summary>
    public ref double this[int row, int col] =>
        ref _data[row * Cols + col];

    /// <summary>Gets a span over the entire backing array.</summary>
    public Span<double> AsSpan() =>
        _data.AsSpan(0, Rows * Cols);

    /// <summary>Gets a span for a specific row.</summary>
    public Span<double> GetRow(int row) =>
        _data.AsSpan(row * Cols, Cols);

    /// <summary>Fills the matrix with a value.</summary>
    public void Fill(double value) =>
        AsSpan().Fill(value);

    /// <summary>Calculates the sum of all elements using Span.</summary>
    public double Sum()
    {
        var span = AsSpan();
        double sum = 0;
        foreach (var v in span) sum += v;
        return sum;
    }

    /// <summary>Returns the array to the pool.</summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_pooled)
            ArrayPool<double>.Shared.Return(_data);
        _data = [];
    }

    public override string ToString() =>
        $"Matrix({Rows}x{Cols}, sum={Sum():F2})";
}
