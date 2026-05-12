namespace PerfBench.Theme6_Memory;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: Creating Span<T> — 9 Construction Patterns
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  Span<T> can be created from many sources. Understanding
//  each construction pattern matters because each represents
//  a different memory origin (heap array, stack, native heap,
//  pointer) and has different lifetime and safety implications.
//
//  This demo covers all 9 ways to create a Span<T>:
//  1. new Span<T>(T[] array)
//  2. new Span<T>(T[] array, int start, int length)
//  3. new Span<T>(T* pointer, int length)       — unsafe
//  4. new Span<T>((T*)void*, int length)         — unsafe
//  5. Marshal.AllocHGlobal + Span from native heap
//  6. Implicit conversion: Span<T> span = array
//  7. stackalloc assigned to Span<T>
//  8. span.Slice(start, length)
//  9. array.AsSpan(start, length)
//
//  TRY IT
//  ──────
//  1. Create a Span<Pixel> from stackalloc and fill it.
//  2. Allocate native memory, create a Span over it, and
//     verify the data round-trips correctly.
//  3. Compare the lifetime rules of each creation pattern.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Demonstrates all 9 ways to create a Span&lt;T&gt;, from safe
/// array-backed to unsafe pointer-based construction.
/// </summary>
public static class CreatingSpanDemo
{
    public static void Run()
    {
        Console.WriteLine("╔═══ Creating Span<T> — 9 Patterns ═══╗\n");

        int[] numbers = [1, 2, 3, 4, 5];

        // ── 1. Constructor: Span(T[] array) ──────────────────
        Console.WriteLine("── 1. new Span<T>(T[] array) ──");
        Span<int> span1 = new Span<int>(numbers);
        Console.Write("  Wraps entire array: ");
        PrintSpan(span1);

        // ── 2. Constructor: Span(T[] array, start, length) ───
        Console.WriteLine("\n── 2. new Span<T>(T[] array, start, length) ──");
        Span<int> span2 = new Span<int>(numbers, 1, 3);
        Console.Write("  Slice [1..4): ");
        PrintSpan(span2);

        // ── 3. Constructor: Span(T* pointer, int length) — unsafe
        Console.WriteLine("\n── 3. new Span<T>(T* pointer, length) — unsafe ──");
        unsafe
        {
            int* stackNums = stackalloc int[] { 10, 20, 30, 40, 50 };
            Span<int> span3 = new Span<int>(stackNums, 5);
            Console.Write("  From int* pointer: ");
            PrintSpan(span3);
        }

        // ── 4. Constructor: Span(void* pointer, int length) — unsafe
        Console.WriteLine("\n── 4. new Span<T>((T*)void*, length) — unsafe ──");
        unsafe
        {
            void* voidPtr = stackalloc int[] { 100, 200, 300 };
            Span<int> span4 = new Span<int>((int*)voidPtr, 3);
            Console.Write("  From void* pointer: ");
            PrintSpan(span4);
        }

        // ── 5. Native heap: Marshal.AllocHGlobal ─────────────
        Console.WriteLine("\n── 5. Marshal.AllocHGlobal — native heap ──");
        nint ptr = Marshal.AllocHGlobal(5 * sizeof(int));
        try
        {
            unsafe
            {
                Span<int> nativeSpan = new Span<int>(ptr.ToPointer(), 5);
                for (int i = 0; i < nativeSpan.Length; i++)
                    nativeSpan[i] = (i + 1) * 10;

                Console.Write("  Native heap span: ");
                PrintSpan(nativeSpan);
            }
            Console.WriteLine("  Memory lives on the native heap (unmanaged).");
            Console.WriteLine("  Must call Marshal.FreeHGlobal() to avoid leaks.");
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }

        // ── 6. Implicit conversion from array ────────────────
        Console.WriteLine("\n── 6. Implicit: Span<T> span = array ──");
        Span<int> span6 = numbers; // no constructor, no AsSpan — just assign
        Console.Write("  Implicit from array: ");
        PrintSpan(span6);
        Console.WriteLine("  The simplest and most common way in C# 14.");

        // ── 7. stackalloc assigned to Span<T> ────────────────
        Console.WriteLine("\n── 7. stackalloc → Span<T> ──");
        Span<int> span7 = stackalloc int[] { 99, 88, 77, 66, 55 };
        Console.Write("  stackalloc span: ");
        PrintSpan(span7);
        Console.WriteLine("  Stack-allocated — no GC, reclaimed on method return.");

        // ── 8. From existing Span via Slice ──────────────────
        Console.WriteLine("\n── 8. span.Slice(start, length) ──");
        Span<int> span8 = span7.Slice(1, 3);
        Console.Write("  Slice of stackalloc span: ");
        PrintSpan(span8);
        Console.WriteLine("  Zero-copy view over the same stack memory.");

        // ── 9. AsSpan() extension method ─────────────────────
        Console.WriteLine("\n── 9. array.AsSpan(start, length) ──");
        Span<int> span9 = numbers.AsSpan(2, 3);
        Console.Write("  AsSpan(2, 3): ");
        PrintSpan(span9);

        // ── Summary ──────────────────────────────────────────
        Console.WriteLine("\n── Summary: Memory Origins ──");
        Console.WriteLine("  Pattern            Memory Source     GC Managed?");
        Console.WriteLine("  ──────────────────────────────────────────────────");
        Console.WriteLine("  1-2. Constructor    Heap array        Yes");
        Console.WriteLine("  3-4. Pointer        Stack/pointer     Depends");
        Console.WriteLine("  5.   AllocHGlobal   Native heap       No (manual free)");
        Console.WriteLine("  6.   Implicit       Heap array        Yes");
        Console.WriteLine("  7.   stackalloc     Stack frame       No (auto reclaim)");
        Console.WriteLine("  8.   Slice          Same as source    Same as source");
        Console.WriteLine("  9.   AsSpan()       Heap array        Yes");
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
}
