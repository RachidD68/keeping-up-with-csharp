namespace PerfBench.Theme6_Memory.SpanAndMemory;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: Span<T>  (C# 7.2)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  PROBLEM
//  -------
//  Arrays allocate on the heap, and slicing them with
//  Array.Copy creates yet another heap allocation.  Parsing
//  a substring?  That's a new string.  Processing a subrange
//  of a byte buffer?  That's a new byte[].  Each allocation
//  feeds the GC pressure monster.
//
//  SOLUTION
//  --------
//  Span<T> provides a zero-copy, bounds-checked view over
//  contiguous memory — arrays, stackalloc, or native buffers.
//  It's a ref struct, so it lives only on the stack, making
//  it extremely cheap.
//
//  For the heap-safe sibling that works with async methods
//  and class fields, see Memory<T> in MemoryT/AsyncMemoryProcessor.cs.
//
//  WHY IT MATTERS
//  ──────────────
//  Zero-copy slicing eliminates millions of small allocations
//  in data-processing pipelines.  The .NET runtime itself was
//  rewritten around Span<T> — Kestrel, System.Text.Json, and
//  the UTF-8 stack all rely on it for throughput.
//
//  TRY IT
//  ──────
//  1. Benchmark Span slicing vs Array.Copy for 10,000 iterations.
//  2. Use MemoryMarshal.Cast to reinterpret Span<Pixel> as
//     Span<byte> — observe the zero-copy cast.
//  3. Build a zero-alloc CSV parser using ReadOnlySpan<char>.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Demonstrates Span&lt;T&gt; for zero-copy buffer processing.
/// For Memory&lt;T&gt; (heap-safe, async-compatible), see AsyncMemoryProcessor.
/// </summary>
public static class BufferProcessorDemo
{
    // ── Before: Array.Copy creates heap allocations ──────────

    /// <summary>
    /// Old-style: extracting a row from a pixel buffer requires
    /// allocating a brand-new array and copying data into it.
    /// </summary>
    private static Pixel[] ExtractRowCopy(Pixel[] image, int width, int rowIndex)
    {
        var row = new Pixel[width]; // heap allocation!
        Array.Copy(image, rowIndex * width, row, 0, width);
        return row;
    }

    /// <summary>
    /// Old-style: extracting a sub-buffer from a byte array.
    /// </summary>
    private static byte[] ExtractSliceCopy(byte[] buffer, int offset, int length)
    {
        var slice = new byte[length]; // another heap allocation!
        Array.Copy(buffer, offset, slice, 0, length);
        return slice;
    }

    // ── After: Span<T> provides zero-copy views ─────────────

    /// <summary>
    /// Returns a Span view over a single row — no allocation,
    /// no copy.  The span points directly into the original array.
    /// </summary>
    private static Span<Pixel> ExtractRowSpan(Pixel[] image, int width, int rowIndex)
        => image.AsSpan(rowIndex * width, width);

    /// <summary>
    /// Processes a row of pixels using Span — calculates average
    /// brightness without allocating anything.
    /// </summary>
    private static float AverageRowBrightness(ReadOnlySpan<Pixel> row)
    {
        if (row.IsEmpty) return 0f;

        var total = 0f;
        foreach (ref readonly var pixel in row)
        {
            total += pixel.Brightness;
        }
        return total / row.Length;
    }

    /// <summary>
    /// Parses sensor data from a text span — no substring allocations.
    /// Format: "SensorId:Value" (e.g., "42:98.6")
    /// </summary>
    private static (int id, double value) ParseSensorData(ReadOnlySpan<char> text)
    {
        var colonIndex = text.IndexOf(':');
        if (colonIndex < 0)
            throw new FormatException("Expected 'Id:Value' format");

        // Slice without allocating new strings
        var idPart = text[..colonIndex];
        var valuePart = text[(colonIndex + 1)..];

        var id = int.Parse(idPart);
        var value = double.Parse(valuePart);
        return (id, value);
    }

    /// <summary>
    /// Demonstrates stackalloc + Span combo: allocate a temp buffer
    /// on the stack, fill it, and process it — zero heap allocations.
    /// </summary>
    private static float ProcessWithStackalloc()
    {
        // stackalloc is only safe because Span bounds-checks access.
        Span<Pixel> tempRow = stackalloc Pixel[8];

        // Fill with a gradient
        for (var i = 0; i < tempRow.Length; i++)
        {
            var intensity = (byte)(i * 32);
            tempRow[i] = new Pixel(intensity, intensity, intensity);
        }

        return AverageRowBrightness(tempRow);
    }

    /// <summary>
    /// Shows how Span slicing chains without allocation.
    /// </summary>
    private static void DemonstrateChainedSlicing(ReadOnlySpan<byte> buffer)
    {
        // Each slice is a view — no copies, no allocations.
        var header = buffer[..16];
        var payload = buffer[16..^4];
        var checksum = buffer[^4..];

        Console.WriteLine($"    Header:   {header.Length} bytes (offset 0)");
        Console.WriteLine($"    Payload:  {payload.Length} bytes (offset 16)");
        Console.WriteLine($"    Checksum: {checksum.Length} bytes (last 4)");
        Console.WriteLine($"    Total:    {buffer.Length} bytes — zero allocations");
    }

    public static void Run()
    {
        Console.WriteLine("\u2554\u2550\u2550\u2550 Span<T> \u2550\u2550\u2550\u2557\n");

        // Build a 4x4 pixel image (16 pixels in a flat array)
        const int width = 4;
        const int height = 4;
        var image = new Pixel[width * height];
        for (var i = 0; i < image.Length; i++)
        {
            var v = (byte)(i * 16);
            image[i] = new Pixel(v, v, v);
        }

        // ── 1. Before vs After: Row Extraction ────────────────
        Console.WriteLine("\u2500\u2500 Row Extraction: Copy vs Span \u2500\u2500");

        Pixel[] copiedRow = ExtractRowCopy(image, width, 1);
        Console.WriteLine($"  Array.Copy: new Pixel[{copiedRow.Length}] allocated on heap");

        Span<Pixel> spanRow = ExtractRowSpan(image, width, 1);
        Console.WriteLine($"  Span slice: {spanRow.Length} pixels, zero allocation");

        // Prove they see the same data
        var same = true;
        for (var i = 0; i < width; i++)
            if (copiedRow[i] != spanRow[i]) same = false;
        Console.WriteLine($"  Same data?  {same}");

        // ── 2. Processing rows without allocation ─────────────
        Console.WriteLine("\n\u2500\u2500 Zero-Copy Row Processing \u2500\u2500");
        for (var row = 0; row < height; row++)
        {
            ReadOnlySpan<Pixel> rowSpan = image.AsSpan(row * width, width);
            var brightness = AverageRowBrightness(rowSpan);
            Console.WriteLine($"  Row {row}: avg brightness = {brightness:F3}");
        }

        // ── 3. Parsing without substring allocations ──────────
        Console.WriteLine("\n\u2500\u2500 Zero-Alloc Parsing with ReadOnlySpan<char> \u2500\u2500");
        var rawData = "42:98.6";
        // Pass the string as a ReadOnlySpan<char> — no substring needed
        var (sensorId, sensorValue) = ParseSensorData(rawData.AsSpan());
        Console.WriteLine($"  Parsed: SensorId={sensorId}, Value={sensorValue}");

        // Parse from a slice of a larger string
        var csv = "ignore,7:23.5,alsoignore";
        var start = csv.IndexOf(',') + 1;
        var end = csv.IndexOf(',', start);
        var (id2, val2) = ParseSensorData(csv.AsSpan(start, end - start));
        Console.WriteLine($"  Parsed from CSV slice: SensorId={id2}, Value={val2}");

        // ── 4. stackalloc + Span combo ────────────────────────
        Console.WriteLine("\n\u2500\u2500 stackalloc + Span \u2500\u2500");
        var stackBrightness = ProcessWithStackalloc();
        Console.WriteLine($"  Stack-allocated gradient brightness: {stackBrightness:F3}");
        Console.WriteLine("  Entire operation: zero heap allocations");

        // ── 5. Chained slicing ────────────────────────────────
        Console.WriteLine("\n\u2500\u2500 Chained Slicing (zero-copy) \u2500\u2500");
        var packet = new byte[64];
        Random.Shared.NextBytes(packet);
        DemonstrateChainedSlicing(packet);

        // ── 6. Span vs Array performance comparison ───────────
        Console.WriteLine("\n\u2500\u2500 Allocation Comparison \u2500\u2500");
        const int iterations = 10_000;
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < iterations; i++)
        {
            var slice = ExtractSliceCopy(packet, 16, 32);
            _ = slice[0]; // prevent dead-code elimination
        }
        var afterCopy = GC.GetAllocatedBytesForCurrentThread();

        for (var i = 0; i < iterations; i++)
        {
            Span<byte> slice = packet.AsSpan(16, 32);
            _ = slice[0]; // prevent dead-code elimination
        }
        var afterSpan = GC.GetAllocatedBytesForCurrentThread();

        Console.WriteLine($"  Array.Copy x{iterations:N0}: ~{afterCopy - before:N0} bytes allocated");
        Console.WriteLine($"  Span.Slice x{iterations:N0}: ~{afterSpan - afterCopy:N0} bytes allocated");
        Console.WriteLine($"  Span eliminated {(afterCopy - before):N0} bytes of allocation");
    }
}
