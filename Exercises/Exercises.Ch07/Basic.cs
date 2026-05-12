// Chapter 7 — Memory & Allocation Control — BASIC
// ----------------------------------------------------------------
// Exercise: Take the ProcessSensorBatchHeap method from PerfBench
//   (allocates a double[] every call) and rewrite it as a method
//   that uses stackalloc double[pixels.Length] for buffers up to
//   256 elements and ArrayPool<double>.Shared for larger ones.
//   Benchmark both versions and compare allocations.
//
// Hint: The ternary stackalloc ? : ArrayPool.Rent() pattern is
//   exactly the shape you need.
// ----------------------------------------------------------------
//
// Note on BenchmarkDotNet: this file uses a simple Stopwatch +
// GC.GetAllocatedBytesForCurrentThread() comparison to avoid an
// extra NuGet dependency. To run the full benchmark, add the
// BenchmarkDotNet package and decorate the methods with
// [Benchmark] / [MemoryDiagnoser]. The accompanying appendix
// commentary shows what those attributes look like.
// ----------------------------------------------------------------

using System.Buffers;
using System.Diagnostics;

namespace KeepUpCs.Exercises.Ch07;

public readonly record struct SensorReading(int Id, double Value);

public static class SensorProcessing
{
    private const int StackThreshold = 256;

    // ── BEFORE (heap allocation every call) ─────────────────────
    public static double ProcessSensorBatchHeap(ReadOnlySpan<SensorReading> readings)
    {
        var buffer = new double[readings.Length]; // heap every call!
        for (int i = 0; i < readings.Length; i++)
            buffer[i] = readings[i].Value;

        double sum = 0;
        foreach (var v in buffer) sum += v;
        return sum / buffer.Length;
    }

    // ── AFTER (stackalloc OR ArrayPool, never a new heap array) ─
    public static double ProcessSensorBatchOptimal(ReadOnlySpan<SensorReading> readings)
    {
        int len = readings.Length;

        // Pick the strategy based on size — stackalloc for small,
        // ArrayPool for large. Cast pooled array to Span via slicing
        // so the rest of the method works against a single Span<double>.
        double[]? pooled = null;
        Span<double> buffer = len <= StackThreshold
            ? stackalloc double[len]
            : (pooled = ArrayPool<double>.Shared.Rent(len)).AsSpan(0, len);

        try
        {
            for (int i = 0; i < len; i++)
                buffer[i] = readings[i].Value;

            double sum = 0;
            for (int i = 0; i < len; i++)
                sum += buffer[i];

            return sum / len;
        }
        finally
        {
            if (pooled is not null)
                ArrayPool<double>.Shared.Return(pooled);
        }
    }
}

public static class BasicDemo
{
    public static void Run()
    {
        Console.WriteLine("Ch07 Basic — stackalloc/ArrayPool rewrite of ProcessSensorBatchHeap");
        Console.WriteLine(new string('─', 60));

        var rng = new Random(42);
        var smallBatch = new SensorReading[100];
        var largeBatch = new SensorReading[10_000];
        for (int i = 0; i < smallBatch.Length; i++)
            smallBatch[i] = new SensorReading(i, rng.NextDouble() * 100);
        for (int i = 0; i < largeBatch.Length; i++)
            largeBatch[i] = new SensorReading(i, rng.NextDouble() * 100);

        const int iter = 5_000;

        // Heap path
        long beforeHeap = GC.GetAllocatedBytesForCurrentThread();
        var swHeap = Stopwatch.StartNew();
        for (int i = 0; i < iter; i++) _ = SensorProcessing.ProcessSensorBatchHeap(smallBatch);
        swHeap.Stop();
        long heapBytes = GC.GetAllocatedBytesForCurrentThread() - beforeHeap;

        // Optimal path
        long beforeOpt = GC.GetAllocatedBytesForCurrentThread();
        var swOpt = Stopwatch.StartNew();
        for (int i = 0; i < iter; i++) _ = SensorProcessing.ProcessSensorBatchOptimal(smallBatch);
        swOpt.Stop();
        long optBytes = GC.GetAllocatedBytesForCurrentThread() - beforeOpt;

        Console.WriteLine($"  small batch ({smallBatch.Length} sensors × {iter:N0} iterations):");
        Console.WriteLine($"    heap:    {heapBytes,15:N0} bytes  in {swHeap.ElapsedMilliseconds,5} ms");
        Console.WriteLine($"    optimal: {optBytes,15:N0} bytes  in {swOpt.ElapsedMilliseconds,5} ms");

        // Large batch — exercises the ArrayPool path
        long beforeLarge = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 200; i++) _ = SensorProcessing.ProcessSensorBatchOptimal(largeBatch);
        long largeBytes = GC.GetAllocatedBytesForCurrentThread() - beforeLarge;

        Console.WriteLine($"  large batch ({largeBatch.Length} sensors × 200 iterations, ArrayPool path):");
        Console.WriteLine($"    optimal: {largeBytes,15:N0} bytes allocated");
    }
}
