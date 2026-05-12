namespace PerfBench.Theme6_Memory;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: ArrayPool<T>  (.NET Core / .NET 6+)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  PROBLEM
//  -------
//  High-throughput code that allocates and discards arrays in
//  a tight loop generates enormous GC pressure.  Processing
//  10,000 sensor batches?  That's 10,000 new double[] arrays
//  that survive for microseconds before becoming garbage.
//
//  SOLUTION
//  --------
//  ArrayPool<T>.Shared maintains a thread-safe pool of arrays.
//  Rent() returns a reusable array (possibly larger than
//  requested), and Return() gives it back for the next caller.
//  No GC, no fragmentation — the same memory is recycled.
//
//  WHY IT MATTERS
//  ──────────────
//  In server workloads, ArrayPool can reduce Gen 0 collections
//  by 10x or more.  The Matrix model already uses it internally,
//  and patterns like MemoryPool<T> build on the same concept.
//  The key discipline: always Return what you Rent, ideally
//  in a try/finally.
//
//  TRY IT
//  ──────
//  1. Remove the Return() call and watch allocation count grow.
//  2. Create an ArrayPool<T>.Create(maxArrayLength, maxPerBucket)
//     — compare behavior with Shared.
//  3. Use the AllocationTracker to log rented vs requested sizes.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Demonstrates ArrayPool&lt;T&gt; for reusing array buffers and reducing GC pressure.
/// </summary>
public static class PooledProcessingDemo
{
    // ── Before: new array every time ─────────────────────────

    /// <summary>
    /// Old-style: each call allocates a fresh array, processes it,
    /// and abandons it to the GC.
    /// </summary>
    private static double ProcessSensorBatchHeap( ReadOnlySpan<SensorReading> readings)
    {
        // Heap allocation every call!
        var buffer = new double[readings.Length];

        for (int i = 0; i < readings.Length; i++)
            buffer[i] = readings[i].Value;

        double sum = 0;
        foreach (var v in buffer)
            sum += v;

        return sum / buffer.Length;
    }

    // ── After: ArrayPool recycles the buffer ─────────────────

    /// <summary>
    /// Pool-backed: Rent an array, use it, Return it.
    /// The same physical memory is reused across calls.
    /// </summary>
    private static double ProcessSensorBatchPooled(ReadOnlySpan<SensorReading> readings)
    {
        // Rent may return an array LARGER than requested.
        // Always use readings.Length, not buffer.Length.
        double[] buffer = ArrayPool<double>.Shared.Rent(readings.Length);

        try
        {
            for (int i = 0; i < readings.Length; i++)
                buffer[i] = readings[i].Value;

            double sum = 0;
            for (int i = 0; i < readings.Length; i++)
                sum += buffer[i];

            return sum / readings.Length;
        }
        finally
        {
            // ALWAYS return rented arrays — preferably in finally.
            // clearArray: true zeros the buffer if it held sensitive data.
            ArrayPool<double>.Shared.Return(buffer, clearArray: false);
        }
    }

    /// <summary>
    /// Demonstrates the Matrix model's built-in pool usage,
    /// tracked by AllocationTracker.
    /// </summary>
    private static void DemonstrateMatrixPooling(AllocationTracker tracker)
    {
        Console.WriteLine("  Creating 3 matrices with pooled backing arrays...");

        // Matrix uses ArrayPool internally (see Models/Matrix.cs)
        using var m1 = new Matrix(100, 100, usePool: true);
        tracker.Track("Matrix 100x100", 100 * 100 * sizeof(double),100 * 100 * sizeof(double), pooled: true);

        using var m2 = new Matrix(50, 200, usePool: true);
        tracker.Track("Matrix 50x200", 50 * 200 * sizeof(double), 50 * 200 * sizeof(double), pooled: true);

        using var m3 = new Matrix(256, 256, usePool: true);
        tracker.Track("Matrix 256x256", 256 * 256 * sizeof(double), 256 * 256 * sizeof(double), pooled: true);

        // Fill and compute
        m1.Fill(1.0);
        m2.Fill(2.0);
        m3.Fill(0.5);

        Console.WriteLine($"    {m1}");
        Console.WriteLine($"    {m2}");
        Console.WriteLine($"    {m3}");

        // Matrices return their arrays to the pool on Dispose
        Console.WriteLine("  Disposing returns arrays to pool for reuse.");
    }

    /// <summary>
    /// Shows the rent/return lifecycle and the over-allocation behavior.
    /// ArrayPool rounds up to power-of-two bucket sizes.
    /// </summary>
    private static void DemonstratePoolBehavior(AllocationTracker tracker)
    {
        int[] requestSizes = [100, 200, 300, 500, 1000, 1500];

        foreach (int requested in requestSizes)
        {
            byte[] rented = ArrayPool<byte>.Shared.Rent(requested);
            int actual = rented.Length;

            tracker.Track($"byte[{requested}]", requested, actual, pooled: true);

            Console.WriteLine($"    Requested: {requested,5}  →  Got: {actual,5}  " +
                              $"(+{actual - requested} overhead, " +
                              $"{(double)requested / actual:P0} utilized)");

            ArrayPool<byte>.Shared.Return(rented);
        }
    }

    /// <summary>
    /// Compares heap allocation vs pool allocation in a loop.
    /// </summary>
    private static (long heapBytes, long poolBytes) CompareAllocationPressure( SensorReading[] readings, int iterations)
    {
        // Heap path
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < iterations; i++)
            _ = ProcessSensorBatchHeap(readings);
        long afterHeap = GC.GetAllocatedBytesForCurrentThread();

        // Pool path
        for (int i = 0; i < iterations; i++)
            _ = ProcessSensorBatchPooled(readings);
        long afterPool = GC.GetAllocatedBytesForCurrentThread();

        return (afterHeap - before, afterPool - afterHeap);
    }

    /// <summary>
    /// Demonstrates creating a custom-configured pool.
    /// </summary>
    private static void DemonstrateCustomPool()
    {
        // Create a pool with specific limits:
        //   maxArrayLength: largest single array the pool will cache
        //   maxArraysPerBucket: how many arrays of each size to keep
        var customPool = ArrayPool<double>.Create(maxArrayLength: 1024, maxArraysPerBucket: 4);

        Console.WriteLine("  Custom pool (max 1024 elements, 4 per bucket):");

        // Rent and return several arrays
        var arrays = new double[6][];
        for (int i = 0; i < arrays.Length; i++)
        {
            arrays[i] = customPool.Rent(256);
            Console.WriteLine($"    Rented [{i}]: length = {arrays[i].Length}");
        }

        // Return all
        for (int i = 0; i < arrays.Length; i++)
            customPool.Return(arrays[i]);

        Console.WriteLine("  All returned — pool holds up to 4 of this size.");

        // Rent again — should get recycled arrays
        var reused = customPool.Rent(256);
        Console.WriteLine($"  Re-rented: length = {reused.Length} (recycled from pool)");
        customPool.Return(reused);
    }

    public static void Run()
    {
        Console.WriteLine("\u2554\u2550\u2550\u2550 ArrayPool<T> \u2550\u2550\u2550\u2557\n");

        // Build test sensor data
        var readings = new SensorReading[100];
        for (int i = 0; i < readings.Length; i++)
        {
            readings[i] = SensorReading.Create(
                sensorId: i % 10,
                value: 20.0 + Random.Shared.NextDouble() * 15.0,
                status: SensorStatus.Normal);
        }

        // ── 1. Before vs After: single batch ─────────────────
        Console.WriteLine("\u2500\u2500 Single Batch: Heap vs Pool \u2500\u2500");
        double heapAvg = ProcessSensorBatchHeap(readings);
        double poolAvg = ProcessSensorBatchPooled(readings);
        Console.WriteLine($"  Heap-allocated:  avg = {heapAvg:F4}");
        Console.WriteLine($"  Pool-allocated:  avg = {poolAvg:F4}");
        Console.WriteLine($"  Results match:   {Math.Abs(heapAvg - poolAvg) < 0.0001}");

        // ── 2. Allocation pressure comparison ─────────────────
        Console.WriteLine("\n\u2500\u2500 GC Pressure: 5,000 Iterations \u2500\u2500");
        const int iterations = 5_000;
        var (heapBytes, poolBytes) = CompareAllocationPressure(readings, iterations);
        Console.WriteLine($"  Heap path:  ~{heapBytes:N0} bytes allocated");
        Console.WriteLine($"  Pool path:  ~{poolBytes:N0} bytes allocated");
        Console.WriteLine($"  Reduction:  ~{(heapBytes > 0 ? (1.0 - (double)poolBytes / heapBytes) * 100 : 0):F1}%");

        // ── 3. Pool over-allocation (bucket sizes) ────────────
        Console.WriteLine("\n\u2500\u2500 Pool Bucket Sizes \u2500\u2500");
        Console.WriteLine("  ArrayPool rounds up to power-of-two buckets:");
        var tracker = new AllocationTracker();
        DemonstratePoolBehavior(tracker);

        // ── 4. Matrix with built-in pooling ───────────────────
        Console.WriteLine("\n\u2500\u2500 Matrix Model (Built-in ArrayPool) \u2500\u2500");
        DemonstrateMatrixPooling(tracker);

        // ── 5. Allocation tracker summary ─────────────────────
        Console.WriteLine("\n\u2500\u2500 AllocationTracker Summary \u2500\u2500");
        tracker.PrintSummary();
        Console.WriteLine("  Regions tracked:");
        foreach (var region in tracker.Regions)
        {
            Console.WriteLine($"    {region}");
        }

        // ── 6. Custom pool configuration ──────────────────────
        Console.WriteLine("\n\u2500\u2500 Custom ArrayPool \u2500\u2500");
        DemonstrateCustomPool();

        // ── 7. The golden rule ────────────────────────────────
        Console.WriteLine("\n\u2500\u2500 Golden Rule \u2500\u2500");
        Console.WriteLine("  ALWAYS return rented arrays, ideally in try/finally.");
        Console.WriteLine("  Forgetting to return causes pool exhaustion and");
        Console.WriteLine("  falls back to new allocations — defeating the purpose.");
    }
}
