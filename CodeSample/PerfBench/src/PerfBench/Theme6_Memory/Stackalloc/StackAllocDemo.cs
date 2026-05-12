namespace PerfBench.Theme6_Memory;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: stackalloc in Safe Contexts  (C# 7.2+, C# 8)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  PROBLEM
//  -------
//  Small temporary arrays — a scratch buffer for 16 pixels,
//  a working set for 64 sensor readings — still allocate on
//  the heap.  Each allocation feeds GC pressure, and the
//  objects themselves are so short-lived they rarely survive
//  Gen 0.  It's pure waste.
//
//  SOLUTION
//  --------
//  `stackalloc` allocates memory directly on the stack frame.
//  Combined with Span<T>, it's fully bounds-checked and safe
//  — no `unsafe` keyword required.  C# 8 extended stackalloc
//  to work inside nested expressions (ternary, initializers).
//
//  WHY IT MATTERS
//  ──────────────
//  Stack allocation is essentially free — the stack pointer
//  moves once, and the memory is reclaimed when the method
//  returns.  No GC, no fragmentation, no Gen 0 promotion.
//  For hot loops processing small buffers, this can eliminate
//  millions of allocations per second.
//
//  TRY IT
//  ──────
//  1. Increase the stackalloc size to 4096 — observe the
//     conditional fallback to ArrayPool.
//  2. Benchmark stackalloc vs new[] for 100-element buffers.
//  3. Try stackalloc inside a foreach loop — what happens
//     when iterations are deep?
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Demonstrates stackalloc for zero-allocation temporary buffers.
/// </summary>
public static class StackAllocDemoRunner
{
    /// <summary>
    /// Reasonable stack threshold — avoid blowing the stack on
    /// large requests.  Anything above this rents from ArrayPool.
    /// </summary>
    private const int stackAllocThreshold = 256;

    // ── Before: small temporary arrays hit the heap ──────────

    /// <summary>
    /// Old-style: even a tiny scratch buffer is heap-allocated.
    /// </summary>
    private static float ComputeAverageBrightnessHeap(ReadOnlySpan<Pixel> pixels)
    {
        // Every call allocates a new float[] on the heap
        var scratch = new float[pixels.Length]; // heap allocation!

        for (int i = 0; i < pixels.Length; i++)
            scratch[i] = pixels[i].Brightness;

        float sum = 0f;
        foreach (var b in scratch)
            sum += b;

        return sum / scratch.Length;
    }

    // ── After: stackalloc eliminates the allocation ──────────

    /// <summary>
    /// Stack-allocated scratch buffer — zero heap allocations.
    /// The Span provides full bounds checking, so this is safe
    /// without the `unsafe` keyword.
    /// </summary>
    private static float ComputeAverageBrightnessStack(ReadOnlySpan<Pixel> pixels)
    {
        // stackalloc on the stack frame — freed when method returns
        Span<float> scratch = stackalloc float[pixels.Length];

        for (int i = 0; i < pixels.Length; i++)
            scratch[i] = pixels[i].Brightness;

        float sum = 0f;
        foreach (var b in scratch)
            sum += b;

        return sum / scratch.Length;
    }

    /// <summary>
    /// The production pattern: stackalloc for small sizes, ArrayPool
    /// for large.  This prevents stack overflow on large inputs while
    /// avoiding heap allocation for typical small buffers.
    /// </summary>
    private static float ComputeAverageBrightnessSafe(ReadOnlySpan<Pixel> pixels)
    {
        float[]? rented = null;

        // C# 8: stackalloc in ternary expressions
        Span<float> scratch = pixels.Length <= stackAllocThreshold
            ? stackalloc float[pixels.Length]
            : (rented = ArrayPool<float>.Shared.Rent(pixels.Length));

        try
        {
            for (int i = 0; i < pixels.Length; i++)
                scratch[i] = pixels[i].Brightness;

            float sum = 0f;
            // Only iterate over the valid portion (rented arrays may be larger)
            foreach (var b in scratch[..pixels.Length])
                sum += b;

            return sum / pixels.Length;
        }
        finally
        {
            if (rented is not null)
                ArrayPool<float>.Shared.Return(rented);
        }
    }

    /// <summary>
    /// Demonstrates stackalloc with Pixel structs for small image
    /// processing — e.g., building a gradient tile.
    /// </summary>
    private static Span<Pixel> BuildGradientTile(Span<Pixel> output, Pixel from, Pixel to)
    {
        for (int i = 0; i < output.Length; i++)
        {
            float t = (float)i / (output.Length - 1);
            output[i] = Pixel.Lerp(from, to, t);
        }
        return output;
    }

    /// <summary>
    /// Demonstrates stackalloc for building a small lookup table
    /// of PacketHeader sizes — entirely on the stack.
    /// </summary>
    private static void BuildPacketLookup()
    {
        // stackalloc in an initializer expression (C# 8)
        Span<int> packetSizes = stackalloc int[]
        {
            PacketHeader.SizeInBytes,
            PacketHeader.SizeInBytes + 64,   // data packet
            PacketHeader.SizeInBytes + 4,    // ack packet
            PacketHeader.SizeInBytes + 0     // heartbeat
        };

        Console.WriteLine("  Packet size lookup (stack-allocated):");
        string[] names = ["Header Only", "Data", "Ack", "Heartbeat"];
        for (int i = 0; i < packetSizes.Length; i++)
        {
            Console.WriteLine($"    {names[i],-15} : {packetSizes[i],4} bytes");
        }
    }

    /// <summary>
    /// Shows stackalloc with byte spans for binary protocol building.
    /// </summary>
    private static void BuildPacketOnStack()
    {
        Span<byte> packet = stackalloc byte[PacketHeader.SizeInBytes + 8];

        // Write header fields directly into the stack buffer
        packet[0] = 1;                          // version
        packet[1] = PacketTypes.Data;           // type
        BitConverter.TryWriteBytes(packet[2..], (ushort)8);  // payload length
        BitConverter.TryWriteBytes(packet[4..], 42u);        // sequence number

        // Write a small payload
        ReadOnlySpan<byte> payload = [0xDE, 0xAD, 0xBE, 0xEF, 0xCA, 0xFE, 0xBA, 0xBE];
        payload.CopyTo(packet[PacketHeader.SizeInBytes..]);

        Console.WriteLine($"  Stack-built packet: {packet.Length} bytes total");
        Console.WriteLine($"    Version:  {packet[0]}");
        Console.WriteLine($"    Type:     0x{packet[1]:X2}");
        Console.WriteLine($"    Payload:  {BitConverter.ToString(packet[PacketHeader.SizeInBytes..].ToArray())}");
    }

    public static void Run()
    {
        Console.WriteLine("\u2554\u2550\u2550\u2550 stackalloc in Safe Contexts \u2550\u2550\u2550\u2557\n");

        // Build a small pixel strip
        var pixels = new Pixel[16];
        for (int i = 0; i < pixels.Length; i++)
        {
            byte v = (byte)(i * 16);
            pixels[i] = new Pixel(v, v, v);
        }

        // ── 1. Before vs After: scratch buffer ────────────────
        Console.WriteLine("\u2500\u2500 Scratch Buffer: Heap vs Stack \u2500\u2500");
        float heapResult = ComputeAverageBrightnessHeap(pixels);
        float stackResult = ComputeAverageBrightnessStack(pixels);
        Console.WriteLine($"  Heap-allocated:  avg brightness = {heapResult:F4}");
        Console.WriteLine($"  Stack-allocated: avg brightness = {stackResult:F4}");
        Console.WriteLine($"  Results match:   {Math.Abs(heapResult - stackResult) < 0.0001f}");

        // ── 2. Allocation comparison ──────────────────────────
        Console.WriteLine("\n\u2500\u2500 Allocation Impact \u2500\u2500");
        const int iterations = 10_000;

        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < iterations; i++)
            _ = ComputeAverageBrightnessHeap(pixels);
        long afterHeap = GC.GetAllocatedBytesForCurrentThread();

        for (int i = 0; i < iterations; i++)
            _ = ComputeAverageBrightnessStack(pixels);
        long afterStack = GC.GetAllocatedBytesForCurrentThread();

        Console.WriteLine($"  Heap path x{iterations:N0}:  ~{afterHeap - before:N0} bytes allocated");
        Console.WriteLine($"  Stack path x{iterations:N0}: ~{afterStack - afterHeap:N0} bytes allocated");

        // ── 3. Conditional: stackalloc vs ArrayPool ───────────
        Console.WriteLine("\n\u2500\u2500 Conditional: Small=Stack, Large=Pool \u2500\u2500");

        float smallResult = ComputeAverageBrightnessSafe(pixels.AsSpan(0, 8));
        Console.WriteLine($"  8 pixels (below threshold):  stackalloc path, avg = {smallResult:F4}");

        var largePixels = new Pixel[512];
        for (int i = 0; i < largePixels.Length; i++)
            largePixels[i] = new Pixel((byte)(i % 256), 128, 64);

        float largeResult = ComputeAverageBrightnessSafe(largePixels);
        Console.WriteLine($"  512 pixels (above threshold): ArrayPool path, avg = {largeResult:F4}");
        Console.WriteLine($"  Threshold: {stackAllocThreshold} elements");

        // ── 4. stackalloc Pixel[] for image processing ────────
        Console.WriteLine("\n\u2500\u2500 stackalloc Pixel[] — Gradient Tile \u2500\u2500");
        Span<Pixel> tile = stackalloc Pixel[8];
        BuildGradientTile(tile, Pixel.Black, Pixel.White);

        Console.WriteLine("  8-pixel gradient (Black to White):");
        for (int i = 0; i < tile.Length; i++)
        {
            Console.WriteLine($"    [{i}] {tile[i]}  brightness={tile[i].Brightness:F3}");
        }

        // ── 5. stackalloc initializer syntax ──────────────────
        Console.WriteLine("\n\u2500\u2500 stackalloc Initializer (C# 8) \u2500\u2500");
        BuildPacketLookup();

        // ── 6. Building binary data on the stack ──────────────
        Console.WriteLine("\n\u2500\u2500 Binary Packet on Stack \u2500\u2500");
        BuildPacketOnStack();

        // ── 7. stackalloc in nested expressions ───────────────
        Console.WriteLine("\n\u2500\u2500 stackalloc in Expressions (C# 8) \u2500\u2500");
        int count = 4;
        // stackalloc directly in a method argument (C# 8 feature)
        ReadOnlySpan<Pixel> gradient = BuildGradientTile(
            stackalloc Pixel[count],
            Pixel.Red,
            Pixel.Blue);

        Console.WriteLine($"  Built {gradient.Length}-pixel Red->Blue gradient inline:");
        foreach (var p in gradient)
            Console.WriteLine($"    {p}");
    }
}
