namespace PerfBench.Theme6_Memory.MemoryT;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: Memory<T> and MemoryPool<T>  (C# 7.2 / .NET 6+)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  PROBLEM
//  -------
//  Span<T> is fast and stack-only, but that very restriction
//  means it cannot be stored in fields, used in async methods,
//  or passed across await boundaries.  If you need to hand a
//  contiguous memory region to an async pipeline, a background
//  task, or a class-level cache, Span<T> simply won't compile.
//
//  SOLUTION
//  --------
//  Memory<T> is the heap-safe sibling of Span<T>.  It wraps
//  the same backing store (array, string, native buffer) but
//  is a regular struct — not a ref struct — so it can live
//  anywhere.  To access the data you call .Span, which hands
//  you back a Span<T> for the actual read/write work.
//
//  MemoryPool<T> extends the idea: rent a Memory<T> via
//  IMemoryOwner<T>, process it across async boundaries, and
//  return it when done — zero long-lived allocations.
//
//  WHY IT MATTERS
//  ──────────────
//  Modern server workloads are inherently async.  Kestrel's
//  I/O pipeline, gRPC streaming, and SignalR all process
//  buffers that outlive a single stack frame.  Memory<T> and
//  MemoryPool<T> let these pipelines stay allocation-free
//  while remaining fully async-compatible.
//
//  TRY IT
//  ──────
//  1. Pass a Memory<Pixel> to an async method that applies
//     a brightness filter — observe that Span would not compile.
//  2. Benchmark MemoryPool.Rent vs new byte[] in a loop.
//  3. Use Memory<T>.Pin() to pass data to a native library.
//  4. Compare ArrayPool<T> vs MemoryPool<T> — when would you
//     choose each?
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Demonstrates Memory&lt;T&gt;, ReadOnlyMemory&lt;T&gt;, IMemoryOwner&lt;T&gt;,
/// and MemoryPool&lt;T&gt; for async-compatible, zero-allocation buffer processing.
/// </summary>
public static class AsyncMemoryProcessorDemo
{
    // ── 1. Memory<T> Basics ──────────────────────────────────

    /// <summary>
    /// Creates a Memory&lt;T&gt; from an array and demonstrates slicing.
    /// Unlike Span, Memory can be stored in fields and passed to async methods.
    /// </summary>
    private static void DemonstrateMemoryBasics(Pixel[] image, int width)
    {
        // Memory<T> wraps an array (or array segment, or IMemoryOwner)
        Memory<Pixel> imageMemory = image.AsMemory();

        // Slicing creates a view — no copy, no allocation
        Memory<Pixel> firstRow = imageMemory[..width];
        Memory<Pixel> secondRow = imageMemory.Slice(width, width);

        // To access the data, get a Span (the fast path)
        Span<Pixel> span = firstRow.Span;
        var brightness = 0f;
        foreach (ref readonly var pixel in span)
            brightness += pixel.Brightness;
        brightness /= span.Length;

        Console.WriteLine($"  Memory<Pixel> over {image.Length} pixels");
        Console.WriteLine($"  First row slice:  {firstRow.Length} pixels");
        Console.WriteLine($"  Second row slice: {secondRow.Length} pixels");
        Console.WriteLine($"  First row avg brightness: {brightness:F3}");
        Console.WriteLine($"  Memory<T> can be stored in a field — Span<T> cannot.");
    }

    // ── 2. ReadOnlyMemory<T> ─────────────────────────────────

    /// <summary>
    /// ReadOnlyMemory&lt;T&gt; provides an immutable view — callers
    /// can read but not modify the underlying data.
    /// </summary>
    private static float ComputeAverageBrightness(ReadOnlyMemory<Pixel> pixels)
    {
        // .Span on ReadOnlyMemory returns ReadOnlySpan
        ReadOnlySpan<Pixel> span = pixels.Span;
        if (span.IsEmpty) return 0f;

        var total = 0f;
        foreach (ref readonly var p in span)
            total += p.Brightness;
        return total / span.Length;
    }

    // ── 3. Storing Memory<T> in a class field ────────────────

    /// <summary>
    /// Demonstrates the key difference: Memory&lt;T&gt; can be a field,
    /// enabling class-based buffers and caches.
    /// Span&lt;T&gt; cannot be stored in a class — it's a ref struct.
    /// </summary>
    private sealed class ImageBuffer(Pixel[] data)
    {
        // This compiles — Memory<T> is a regular struct
        private readonly Memory<Pixel> buffer = data;

        // This would NOT compile:
        // private Span<Pixel> _span;  // Error CS8345: ref struct in non-ref struct

        public int Length => buffer.Length;

        public Memory<Pixel> GetRow(int row, int width) =>
            buffer.Slice(row * width, width);

        public float AverageBrightness() =>
            ComputeAverageBrightness(buffer);
    }

    // ── 4. IMemoryOwner<T> & MemoryPool<T> ───────────────────

    /// <summary>
    /// MemoryPool&lt;T&gt; returns IMemoryOwner&lt;T&gt; — a disposable
    /// wrapper around a pooled Memory&lt;T&gt;.  Disposing returns
    /// the memory to the pool for reuse.
    /// </summary>
    private static void DemonstrateMemoryPool()
    {
        Console.WriteLine("  Renting from MemoryPool<byte>.Shared:");

        int[] requestSizes = [128, 512, 1024, 4096];
        foreach (var size in requestSizes)
        {
            // Rent returns IMemoryOwner<T> — must be disposed!
            using IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent(size);

            // The Memory property gives you the buffer
            Memory<byte> memory = owner.Memory;

            // Pool may return more than requested (like ArrayPool)
            Console.WriteLine($"    Requested: {size,5}  →  Got: {memory.Length,5}  " +
                              $"(+{memory.Length - size} overhead)");

            // Fill via Span for fast access
            memory.Span.Fill(0xAA);

            // When 'using' scope ends, memory returns to pool
        }
    }

    /// <summary>
    /// Compares MemoryPool vs ArrayPool vs raw allocation.
    /// </summary>
    private static void ComparePoolApproaches(int bufferSize, int iterations)
    {
        // ── Raw allocation ──
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < iterations; i++)
        {
            var buffer = new byte[bufferSize];
            buffer[0] = 1; // prevent dead-code elimination
        }
        var afterRaw = GC.GetAllocatedBytesForCurrentThread();

        // ── ArrayPool ──
        for (var i = 0; i < iterations; i++)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
            buffer[0] = 1;
            ArrayPool<byte>.Shared.Return(buffer);
        }
        var afterArrayPool = GC.GetAllocatedBytesForCurrentThread();
        // ── MemoryPool ──
        for (var i = 0; i < iterations; i++)
        {
            using var owner = MemoryPool<byte>.Shared.Rent(bufferSize);
            owner.Memory.Span[0] = 1;
        }
        var afterMemoryPool = GC.GetAllocatedBytesForCurrentThread();

        Console.WriteLine($"  Buffer size: {bufferSize}, iterations: {iterations:N0}");
        Console.WriteLine($"    new byte[]:    ~{afterRaw - before:N0} bytes allocated");
        Console.WriteLine($"    ArrayPool:     ~{afterArrayPool - afterRaw:N0} bytes allocated");
        Console.WriteLine($"    MemoryPool:    ~{afterMemoryPool - afterArrayPool:N0} bytes allocated");
    }

    // ── 5. Async-compatible usage ────────────────────────────

    /// <summary>
    /// Memory&lt;T&gt; can cross await boundaries — Span&lt;T&gt; cannot.
    /// This is the primary reason Memory&lt;T&gt; exists.
    /// </summary>
    private static async Task<float> ProcessPixelsAsync(Memory<Pixel> pixels)
    {
        // Simulate async I/O (e.g., reading from a socket, file, or database)
        await Task.Yield();

        // After the await, we can still access the Memory
        // (Span<T> would cause a compiler error here)
        var  totalBrightness = 0f;
        Span<Pixel> span = pixels.Span;
        foreach (ref readonly var p in span)
            totalBrightness += p.Brightness;

        // Another await — Memory<T> survives across all of them
        await Task.Yield();

        return totalBrightness / pixels.Length;
    }

    /// <summary>
    /// Async pipeline that reads sensor data using pooled memory.
    /// Shows the production pattern: rent → async process → return.
    /// </summary>
    private static async Task<double> ProcessSensorBatchAsync(
        SensorReading[] source, int batchSize)
    {
        // Rent a buffer that outlives each await
        using IMemoryOwner<double> owner = MemoryPool<double>.Shared.Rent(batchSize);
        Memory<double> buffer = owner.Memory[..batchSize];

        // Fill the buffer (could be an async read from a stream)
        Span<double> span = buffer.Span;
        for (var i = 0; i < batchSize && i < source.Length; i++)
            span[i] = source[i].Value;

        await Task.Yield(); // simulate async boundary

        // Compute average — buffer is still valid after await
        var sum = 0.0;
        ReadOnlySpan<double> readSpan = buffer.Span;
        foreach (var v in readSpan)
            sum += v;

        return sum / batchSize;
    }

    // ── 6. Memory<T>.Pin() ───────────────────────────────────

    /// <summary>
    /// Pin() returns a MemoryHandle that keeps the buffer fixed in
    /// memory — safe for passing to native code via P/Invoke.
    /// </summary>
    private static unsafe void DemonstratePin()
    {
        var data = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF, 0xCA, 0xFE };
        Memory<byte> memory = data;

        // Pin the memory — GC won't move it while pinned
        using var handle = memory.Pin();

        // Get a raw pointer for native interop
        var ptr = (byte*)handle.Pointer;

        Console.WriteLine("  Pinned Memory<byte> for native interop:");
        Console.Write("    Raw bytes: ");
        for (var i = 0; i < data.Length; i++)
            Console.Write($"0x{ptr[i]:X2} ");
        Console.WriteLine();
        Console.WriteLine("    GC cannot move this buffer while the handle is alive.");
        Console.WriteLine("    Dispose the MemoryHandle to unpin.");

        // handle.Dispose() is called by 'using' — unpins the memory
    }

    // ── 7. Span<T> ↔ Memory<T> decision guide ───────────────

    private static void PrintDecisionGuide()
    {
        Console.WriteLine("  Aspect                    Span<T>              Memory<T>");
        Console.WriteLine("  ─────────────────────────────────────────────────────────────");
        Console.WriteLine("  Type category             ref struct            struct");
        Console.WriteLine("  Store in field             No                    Yes");
        Console.WriteLine("  Cross await boundary       No                    Yes");
        Console.WriteLine("  Pass to async method       No                    Yes");
        Console.WriteLine("  Access speed               Direct (fastest)      Via .Span");
        Console.WriteLine("  GC pressure                None                  None*");
        Console.WriteLine("  Pin for interop            N/A (stack-only)      .Pin()");
        Console.WriteLine("  Pool API                   ArrayPool<T>          MemoryPool<T>");
        Console.WriteLine();
        Console.WriteLine("  * Memory<T> itself is allocation-free. The backing array");
        Console.WriteLine("    may allocate unless you use MemoryPool<T>.");
        Console.WriteLine();
        Console.WriteLine("  RULE OF THUMB:");
        Console.WriteLine("  Use Span<T> inside synchronous methods for maximum speed.");
        Console.WriteLine("  Use Memory<T> when the buffer must outlive a stack frame");
        Console.WriteLine("  (fields, async methods, callbacks, queues).");
    }

    public static void Run()
    {
        Console.WriteLine("╔═══ Memory<T> and MemoryPool<T> ═══╗\n");

        // Build a 4x4 pixel image
        const int width = 4;
        const int height = 4;
        var image = new Pixel[width * height];
        for (var i = 0; i < image.Length; i++)
        {
            var  v = (byte)(i * 16);
            image[i] = new Pixel(v, v, v);
        }

        // ── 1. Memory<T> basics ──────────────────────────────
        Console.WriteLine("── Memory<T> Basics ──");
        DemonstrateMemoryBasics(image, width);

        // ── 2. ReadOnlyMemory<T> ─────────────────────────────
        Console.WriteLine("\n── ReadOnlyMemory<T> ──");
        ReadOnlyMemory<Pixel> roMemory = image;
        var avg = ComputeAverageBrightness(roMemory);
        Console.WriteLine($"  ReadOnlyMemory<Pixel>: avg brightness = {avg:F3}");
        Console.WriteLine("  Callers get read-only access — cannot modify the pixels.");

        // ── 3. Storing in a class field ──────────────────────
        Console.WriteLine("\n── Memory<T> in Class Fields ──");
        var buffer = new ImageBuffer(image);
        Memory<Pixel> row1 = buffer.GetRow(1, width);
        Console.WriteLine($"  ImageBuffer: {buffer.Length} pixels total");
        Console.WriteLine($"  Row 1: {row1.Length} pixels (Memory<Pixel> stored in field)");
        Console.WriteLine($"  Avg brightness: {buffer.AverageBrightness():F3}");

        // ── 4. MemoryPool<T> ─────────────────────────────────
        Console.WriteLine("\n── MemoryPool<T> & IMemoryOwner<T> ──");
        DemonstrateMemoryPool();

        // ── 5. Allocation comparison: three-way ──────────────
        Console.WriteLine("\n── Allocation Comparison: new[] vs ArrayPool vs MemoryPool ──");
        ComparePoolApproaches(bufferSize: 1024, iterations: 5_000);

        // ── 6. Async-compatible usage ────────────────────────
        Console.WriteLine("\n── Async-Compatible Usage ──");
        Memory<Pixel> asyncMemory = image;
        var asyncResult = ProcessPixelsAsync(asyncMemory).GetAwaiter().GetResult();
        Console.WriteLine($"  Async pixel processing: avg brightness = {asyncResult:F3}");
        Console.WriteLine("  Memory<T> crossed two await boundaries — Span<T> cannot.");

        var readings = new SensorReading[20];
        for (var i = 0; i < readings.Length; i++)
            readings[i] = SensorReading.Create(i, 20.0 + Random.Shared.NextDouble() * 80.0);

        var sensorAvg = ProcessSensorBatchAsync(readings, 10).GetAwaiter().GetResult();
        Console.WriteLine($"  Async sensor batch (pooled Memory): avg = {sensorAvg:F2}");

        // ── 7. Memory<T>.Pin() ───────────────────────────────
        Console.WriteLine("\n── Memory<T>.Pin() — Native Interop ──");
        DemonstratePin();

        // ── 8. Decision guide ────────────────────────────────
        Console.WriteLine("\n── Span<T> vs Memory<T>: When to Use Which ──");
        PrintDecisionGuide();
    }
}
