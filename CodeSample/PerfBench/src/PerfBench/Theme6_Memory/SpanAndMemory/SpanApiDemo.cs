namespace PerfBench.Theme6_Memory;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: Span<T> API — Properties & Methods
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  Span<T> is not just a slice — it provides a rich API for
//  manipulating contiguous memory in-place, without allocating.
//
//  This demo covers every key property and method:
//  ● Length, IsEmpty, Item[int]
//  ● Slice(start), Slice(start, length)
//  ● Fill(value), Clear()
//  ● CopyTo(destination), TryCopyTo(destination)
//  ● ToArray()
//  ● Reverse() — in-place, zero-allocation
//
//  TRY IT
//  ──────
//  1. Call Fill with a Pixel value, then Reverse the span.
//  2. Try CopyTo with a destination that is too small — observe
//     the ArgumentException.
//  3. Use TryCopyTo to safely handle size mismatches.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Demonstrates the full Span&lt;T&gt; API: properties and methods
/// for in-place, zero-allocation buffer manipulation.
/// </summary>
public static class SpanApiDemo
{
    public static void Run()
    {
        Console.WriteLine("╔═══ Span<T> API — Properties & Methods ═══╗\n");

        int[] numbers = [1, 2, 3, 4, 5, 6, 7, 8];
        Span<int> span = numbers;

        // ── 1. Properties ────────────────────────────────────
        Console.WriteLine("── Properties ──");
        Console.WriteLine($"  Length:    {span.Length}");
        Console.WriteLine($"  IsEmpty:  {span.IsEmpty}");
        Console.WriteLine($"  span[0]:  {span[0]}");
        Console.WriteLine($"  span[^1]: {span[^1]}  (last element)");

        Span<int> empty = Span<int>.Empty;
        Console.WriteLine($"  Span<int>.Empty.IsEmpty: {empty.IsEmpty}");

        // ── 2. Slice ─────────────────────────────────────────
        Console.WriteLine("\n── Slice ──");
        Span<int> fromIndex2 = span.Slice(2);
        Console.Write("  Slice(2):    ");
        PrintSpan(fromIndex2);

        Span<int> middle = span.Slice(2, 3);
        Console.Write("  Slice(2, 3): ");
        PrintSpan(middle);

        // Range syntax equivalent
        Span<int> rangeSlice = span[2..5];
        Console.Write("  span[2..5]:  ");
        PrintSpan(rangeSlice);

        // ── 3. Fill ──────────────────────────────────────────
        Console.WriteLine("\n── Fill ──");
        Span<int> fillTarget = stackalloc int[5];
        Console.Write("  Before Fill: ");
        PrintSpan(fillTarget);

        fillTarget.Fill(42);
        Console.Write("  After Fill(42): ");
        PrintSpan(fillTarget);

        // ── 4. Clear ─────────────────────────────────────────
        Console.WriteLine("\n── Clear ──");
        Span<int> clearTarget = stackalloc int[] { 10, 20, 30, 40, 50 };
        Console.Write("  Before Clear: ");
        PrintSpan(clearTarget);

        clearTarget.Clear();
        Console.Write("  After Clear:  ");
        PrintSpan(clearTarget);

        // ── 5. CopyTo ────────────────────────────────────────
        Console.WriteLine("\n── CopyTo ──");
        Span<int> source = stackalloc int[] { 100, 200, 300 };
        Span<int> dest = stackalloc int[5];
        dest.Fill(0);

        source.CopyTo(dest);
        Console.Write("  Source:      ");
        PrintSpan(source);
        Console.Write("  Dest (5):    ");
        PrintSpan(dest);

        // CopyTo with too-small destination throws
        Span<int> tooSmall = stackalloc int[2];
        try
        {
            source.CopyTo(tooSmall);
        }
        catch (ArgumentException e)
        {
            Console.WriteLine($"  CopyTo(tooSmall) threw: {e.Message}");
        }

        // ── 6. TryCopyTo ─────────────────────────────────────
        Console.WriteLine("\n── TryCopyTo ──");
        Span<int> safeTarget = stackalloc int[2];
        bool success = source.TryCopyTo(safeTarget);
        Console.WriteLine($"  TryCopyTo(2-element dest): {success}  (safe, no exception)");

        Span<int> goodTarget = stackalloc int[3];
        success = source.TryCopyTo(goodTarget);
        Console.Write($"  TryCopyTo(3-element dest): {success} → ");
        PrintSpan(goodTarget);

        // ── 7. Reverse ───────────────────────────────────────
        Console.WriteLine("\n── Reverse ──");
        Span<byte> bytes = stackalloc byte[] { 0xDE, 0xAD, 0xBE, 0xEF };
        Console.Write("  Before Reverse: ");
        PrintHexSpan(bytes);

        bytes.Reverse();
        Console.Write("  After Reverse:  ");
        PrintHexSpan(bytes);
        Console.WriteLine("  In-place, zero allocation — no new array created.");

        // ── 8. ToArray ───────────────────────────────────────
        Console.WriteLine("\n── ToArray ──");
        Span<int> spanForArray = stackalloc int[] { 5, 10, 15 };
        int[] array = spanForArray.ToArray();
        Console.WriteLine($"  ToArray() returns int[{array.Length}]: [{string.Join(", ", array)}]");
        Console.WriteLine("  Note: ToArray allocates a new array — use only when you must.");

        // ── 9. SequenceEqual ─────────────────────────────────
        Console.WriteLine("\n── SequenceEqual ──");
        Span<int> a = stackalloc int[] { 1, 2, 3 };
        Span<int> b = stackalloc int[] { 1, 2, 3 };
        Span<int> c = stackalloc int[] { 1, 2, 4 };
        Console.WriteLine($"  [1,2,3] == [1,2,3]: {a.SequenceEqual(b)}");
        Console.WriteLine($"  [1,2,3] == [1,2,4]: {a.SequenceEqual(c)}");

        // ── 10. Pixel example ────────────────────────────────
        Console.WriteLine("\n── Pixel Manipulation with Span API ──");
        Span<Pixel> pixels = stackalloc Pixel[4];
        pixels.Fill(Pixel.Black);
        Console.Write("  After Fill(Black): ");
        foreach (var p in pixels) Console.Write($"{p} ");
        Console.WriteLine();

        pixels[0] = Pixel.Red;
        pixels[^1] = Pixel.Blue;
        Console.Write("  After set [0]=Red, [^1]=Blue: ");
        foreach (var p in pixels) Console.Write($"{p} ");
        Console.WriteLine();

        pixels.Reverse();
        Console.Write("  After Reverse: ");
        foreach (var p in pixels) Console.Write($"{p} ");
        Console.WriteLine();
    }

    private static void PrintSpan(ReadOnlySpan<int> span)
    {
        Console.Write("[");
        for (int i = 0; i < span.Length; i++)
        {
            if (i > 0) Console.Write(", ");
            Console.Write(span[i]);
        }
        Console.WriteLine("]");
    }

    private static void PrintHexSpan(ReadOnlySpan<byte> span)
    {
        Console.Write("[");
        for (int i = 0; i < span.Length; i++)
        {
            if (i > 0) Console.Write(", ");
            Console.Write($"0x{span[i]:X2}");
        }
        Console.WriteLine("]");
    }
}
