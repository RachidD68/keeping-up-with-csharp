namespace TypeForge.Theme4_TypeSystem;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: Inline Arrays  (C# 12)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  PROBLEM
//  -------
//  Fixed-size buffers were only available in unsafe code.
//  For high-performance scenarios (networking, parsing),
//  you needed unsafe pointers to have a struct with an
//  inline array of known size.
//
//  SOLUTION
//  --------
//  C# 12 inline arrays use the [InlineArray(N)] attribute
//  to create a fixed-size buffer as a regular struct.
//  They are indexable, span-compatible, and safe.
//
//  WHY IT MATTERS
//  ──────────────
//  Zero-allocation fixed buffers without unsafe code.
//  Used extensively in the runtime: interpolated string
//  handlers, collection builders, and network buffers.
//
//  TRY IT
//  ──────
//  1. Create an InlineArray(16) for a UUID buffer.
//  2. Use an inline array as a scratch buffer in a method.
//  3. Compare performance with a regular array allocation.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Demonstrates inline arrays for fixed-size, stack-allocated buffers.
/// </summary>
public static class FixedBufferPluginDemo
{
    // ── Inline array declarations ────────────────────────────

    /// <summary>
    /// A fixed-size buffer of 4 doubles — perfect for RGBA or quaternion.
    /// [InlineArray(4)] makes this a struct with 4 contiguous doubles.
    /// </summary>
    [InlineArray(4)]
    private struct Buffer4
    {
        private double _element0; // Only the first element is declared
        // The compiler generates _element1, _element2, _element3
    }

    /// <summary>
    /// A fixed buffer of 8 ints — for small lookup tables or indices.
    /// </summary>
    [InlineArray(8)]
    private struct IntBuffer8
    {
        private int _element0;
    }

    /// <summary>
    /// A char buffer for small string operations without allocation.
    /// </summary>
    [InlineArray(32)]
    private struct CharBuffer32
    {
        private char _element0;
    }

    // ── Helper methods using inline arrays ───────────────────

    /// <summary>
    /// Calculates statistics using a stack-allocated buffer.
    /// No heap allocation needed for the working buffer.
    /// </summary>
    private static (double Min, double Max, double Avg) CalculateStats(
        ReadOnlySpan<double> values)
    {
        if (values.IsEmpty) return (0, 0, 0);

        double min = values[0], max = values[0], sum = values[0];
        for (int i = 1; i < values.Length; i++)
        {
            if (values[i] < min) min = values[i];
            if (values[i] > max) max = values[i];
            sum += values[i];
        }
        return (min, max, sum / values.Length);
    }

    /// <summary>
    /// Builds a short type name into a fixed buffer.
    /// </summary>
    private static string FormatTypeName(string name)
    {
        var buffer = new CharBuffer32();
        Span<char> span = buffer;

        int written = 0;
        foreach (var c in name)
        {
            if (written >= span.Length - 1) break;
            span[written++] = c;
        }

        return new string(span[..written]);
    }

    public static void Run()
    {
        Console.WriteLine("╔═══ Inline Arrays ═══╗\n");

        // ── 1. Basic inline array usage ──────────────────────
        Console.WriteLine("── Basic Inline Array ──");
        var buffer = new Buffer4();

        // Index directly like an array
        buffer[0] = 1.0;
        buffer[1] = 2.5;
        buffer[2] = 3.7;
        buffer[3] = 4.2;

        Console.WriteLine("  Buffer4 values:");
        // Convert to span for iteration
        ReadOnlySpan<double> span = buffer;
        for (int i = 0; i < span.Length; i++)
            Console.WriteLine($"    [{i}] = {span[i]}");

        // ── 2. Span compatibility ────────────────────────────
        Console.WriteLine("\n── Span Compatibility ──");
        var stats = CalculateStats(buffer);
        Console.WriteLine($"  Min: {stats.Min}, Max: {stats.Max}, Avg: {stats.Avg:F2}");

        // ── 3. Int buffer for indices ────────────────────────
        Console.WriteLine("\n── IntBuffer8 for Indices ──");
        var indices = new IntBuffer8();
        for (int i = 0; i < 8; i++)
            indices[i] = i * 10;

        Span<int> intSpan = indices;
        Console.Write("  Indices: ");
        foreach (var idx in intSpan)
            Console.Write($"{idx} ");
        Console.WriteLine();

        // Slice the span
        var firstHalf = intSpan[..4];
        Console.Write("  First 4: ");
        foreach (var idx in firstHalf)
            Console.Write($"{idx} ");
        Console.WriteLine();

        // ── 4. Char buffer for string operations ─────────────
        Console.WriteLine("\n── CharBuffer32 for String Ops ──");
        Console.WriteLine($"  Formatted: '{FormatTypeName("Dictionary")}'");
        Console.WriteLine($"  Formatted: '{FormatTypeName("IReadOnlyCollection")}'");

        // ── 5. Initialize with collection expression ─────────
        Console.WriteLine("\n── Collection Expression Init ──");
        var initialized = new Buffer4();
        initialized[0] = 10.0;
        initialized[1] = 20.0;
        initialized[2] = 30.0;
        initialized[3] = 40.0;
        ReadOnlySpan<double> initSpan = initialized;
        Console.Write("  Values: ");
        foreach (var v in initSpan)
            Console.Write($"{v} ");
        Console.WriteLine();

        // ── 6. Before vs After ───────────────────────────────
        Console.WriteLine("\n── Before vs After ──");
        Console.WriteLine("  Before C# 12 (unsafe fixed buffer):");
        Console.WriteLine("    unsafe struct OldBuffer { fixed double data[4]; }");
        Console.WriteLine("  After C# 12 (inline array):");
        Console.WriteLine("    [InlineArray(4)] struct Buffer4 { double _e0; }");
        Console.WriteLine();
        Console.WriteLine("  Key differences:");
        Console.WriteLine("    ✓ Safe — no unsafe keyword needed");
        Console.WriteLine("    ✓ Indexable — buffer[i] works");
        Console.WriteLine("    ✓ Span-compatible — implicit conversion");
        Console.WriteLine("    ✓ Stack-allocated — no heap allocation");
        Console.WriteLine("    ✓ Works with collection expressions");
    }
}
