namespace PerfBench.Theme7_LowLevel;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: Function Pointers  (C# 9)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  PROBLEM
//  -------
//  Delegates in C# are reference types — each invocation
//  involves a heap-allocated object, a virtual dispatch, and
//  potential closure captures.  For hot paths that invoke
//  callbacks millions of times per frame (image processing,
//  physics simulations, audio DSP), this overhead is
//  measurable and unnecessary.
//
//  SOLUTION
//  --------
//  C# 9 introduced `delegate*` — function pointers that are
//  value types, carry zero allocation overhead, and compile
//  to a direct call instruction.  They require static methods
//  (no instance state) and the `unsafe` context.
//
//  WHY IT MATTERS
//  ──────────────
//  Function pointers close the last significant performance
//  gap between C# and native C for callback-heavy workloads.
//  They are used internally by the .NET runtime for JIT stubs,
//  COM interop, and native-to-managed transitions.
//
//  TRY IT
//  ──────
//  1. Add a new transform (e.g., sepia filter) and call it
//     via function pointer.
//  2. Benchmark delegate vs function pointer with BenchmarkDotNet.
//  3. Try `delegate* unmanaged` with [UnmanagedCallersOnly].
//  4. Combine function pointers with Span<Pixel> for maximum
//     throughput.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Demonstrates function pointers (delegate*) for zero-allocation
/// pixel transforms, compared to standard Func delegates.
/// </summary>
public static class HighPerfCallbacksDemo
{
    // ──────────────────────────────────────────────────────────
    // Transform functions — must be static for function pointers
    // ──────────────────────────────────────────────────────────

    /// <summary>Inverts a pixel's color channels.</summary>
    private static Pixel InvertPixel(Pixel p) =>
        new((byte)(255 - p.R), (byte)(255 - p.G), (byte)(255 - p.B), p.A);

    /// <summary>Converts a pixel to grayscale using luminance weights.</summary>
    private static Pixel GrayscalePixel(Pixel p)
    {
        byte gray = (byte)(0.299f * p.R + 0.587f * p.G + 0.114f * p.B);
        return new(gray, gray, gray, p.A);
    }

    /// <summary>Boosts brightness by 25%.</summary>
    private static Pixel BrightenPixel(Pixel p) =>
        new(ClampByte(p.R * 1.25f),
            ClampByte(p.G * 1.25f),
            ClampByte(p.B * 1.25f),
            p.A);

    /// <summary>Applies a warm tint (boost red, reduce blue).</summary>
    private static Pixel WarmTintPixel(Pixel p) =>
        new(ClampByte(p.R * 1.1f),
            p.G,
            ClampByte(p.B * 0.9f),
            p.A);

    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — Func<Pixel, Pixel> allocates
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Before: applying transforms via Func delegates.
    /// Each Func is a heap-allocated object with a method table
    /// pointer and a target reference.  Invocation goes through
    /// a virtual dispatch.
    /// </summary>
    private static Pixel[] ApplyWithDelegate(Pixel[] pixels, Func<Pixel, Pixel> transform)
    {
        var result = new Pixel[pixels.Length];

        for (int i = 0; i < pixels.Length; i++)
        {
            result[i] = transform(pixels[i]); // virtual dispatch + possible null check
        }

        return result;
    }

    private static void DemonstrateDelegateApproach(Pixel[] pixels)
    {
        Console.WriteLine("  BEFORE (Func<Pixel, Pixel> — heap-allocated delegate):");
        Console.WriteLine();

        // Each of these creates a delegate object on the heap
        Func<Pixel, Pixel> invertFunc = InvertPixel;
        Func<Pixel, Pixel> grayFunc = GrayscalePixel;
        Func<Pixel, Pixel> brightFunc = BrightenPixel;

        Console.WriteLine("    Func<Pixel, Pixel> is a reference type:");
        Console.WriteLine($"    - invertFunc  type: {invertFunc.GetType().Name}");
        Console.WriteLine($"    - Each delegate is a heap object (~48 bytes on x64)");
        Console.WriteLine($"    - Invocation: virtual dispatch through method table");
        Console.WriteLine();

        var inverted = ApplyWithDelegate(pixels, invertFunc);
        var grayed = ApplyWithDelegate(pixels, grayFunc);
        var brightened = ApplyWithDelegate(pixels, brightFunc);

        Console.WriteLine("    Results:");
        Console.WriteLine($"    Invert:    {pixels[0]} -> {inverted[0]}");
        Console.WriteLine($"    Grayscale: {pixels[1]} -> {grayed[1]}");
        Console.WriteLine($"    Brighten:  {pixels[2]} -> {brightened[2]}");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — delegate* (function pointer)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// After: applying transforms via function pointers.
    /// delegate* is a value type — no heap allocation, no
    /// virtual dispatch.  The call compiles to a direct `call`
    /// or `jmp` instruction.
    /// </summary>
    private static unsafe Pixel[] ApplyWithFunctionPointer(
        Pixel[] pixels,
        delegate*<Pixel, Pixel> transform)
    {
        var result = new Pixel[pixels.Length];

        for (int i = 0; i < pixels.Length; i++)
        {
            result[i] = transform(pixels[i]); // direct call — no allocation, no dispatch
        }

        return result;
    }

    private static unsafe void DemonstrateFunctionPointerApproach(Pixel[] pixels)
    {
        Console.WriteLine("  AFTER (delegate*<Pixel, Pixel> — zero-alloc function pointer):");
        Console.WriteLine();

        // Function pointers are obtained with the address-of operator (&)
        // They must point to static methods (no instance state)
        delegate*<Pixel, Pixel> invertFp = &InvertPixel;
        delegate*<Pixel, Pixel> grayFp = &GrayscalePixel;
        delegate*<Pixel, Pixel> brightFp = &BrightenPixel;
        delegate*<Pixel, Pixel> warmFp = &WarmTintPixel;

        Console.WriteLine("    delegate*<Pixel, Pixel> is a value type:");
        Console.WriteLine($"    - Size: {sizeof(delegate*<Pixel, Pixel>)} bytes (just a native pointer)");
        Console.WriteLine($"    - No heap allocation, no GC pressure");
        Console.WriteLine($"    - Invocation: direct call instruction");
        Console.WriteLine();

        var inverted = ApplyWithFunctionPointer(pixels, invertFp);
        var grayed = ApplyWithFunctionPointer(pixels, grayFp);
        var brightened = ApplyWithFunctionPointer(pixels, brightFp);
        var warmed = ApplyWithFunctionPointer(pixels, warmFp);

        Console.WriteLine("    Results:");
        Console.WriteLine($"    Invert:    {pixels[0]} -> {inverted[0]}");
        Console.WriteLine($"    Grayscale: {pixels[1]} -> {grayed[1]}");
        Console.WriteLine($"    Brighten:  {pixels[2]} -> {brightened[2]}");
        Console.WriteLine($"    Warm Tint: {pixels[3]} -> {warmed[3]}");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 3: Pipeline — chaining function pointers
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Demonstrates chaining multiple function pointers in a
    /// transform pipeline — each stage is a zero-alloc call.
    /// </summary>
    private static unsafe void DemonstratePipeline(Pixel[] pixels)
    {
        Console.WriteLine("  PIPELINE — Chaining function pointers:");
        Console.WriteLine();

        // Build a pipeline of transforms (stored as an array of function pointers)
        // Note: we use stackalloc-style thinking, but function pointers
        // are just native-sized integers
        delegate*<Pixel, Pixel> step1 = &BrightenPixel;
        delegate*<Pixel, Pixel> step2 = &WarmTintPixel;
        delegate*<Pixel, Pixel> step3 = &GrayscalePixel;

        Console.WriteLine("    Pipeline: Brighten -> Warm Tint -> Grayscale");
        Console.WriteLine();

        for (int i = 0; i < Math.Min(4, pixels.Length); i++)
        {
            Pixel original = pixels[i];
            Pixel result = original;

            // Each call is a direct instruction — zero overhead per stage
            result = step1(result);
            result = step2(result);
            result = step3(result);

            Console.WriteLine($"    [{i}] {original} -> {result}  (3 stages, 0 allocations)");
        }
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 4: Comparison summary
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Summarizes the trade-offs between delegates and function pointers.
    /// </summary>
    private static void ShowComparisonTable()
    {
        Console.WriteLine("  COMPARISON — Delegate vs Function Pointer:");
        Console.WriteLine();
        Console.WriteLine("    Aspect                Func<T,TResult>      delegate*<T,TResult>");
        Console.WriteLine("    ─────────────────────────────────────────────────────────────────");
        Console.WriteLine("    Type category         Reference type       Value type (nint)");
        Console.WriteLine("    Heap allocation       Yes (~48 bytes)      None");
        Console.WriteLine("    GC pressure           Yes                  None");
        Console.WriteLine("    Invocation             Virtual dispatch     Direct call");
        Console.WriteLine("    Instance methods       Yes                  No (static only)");
        Console.WriteLine("    Closures               Yes (captures)       No");
        Console.WriteLine("    Null safety            Runtime check        No (undefined behavior)");
        Console.WriteLine("    Multicast              Yes (+=)             No");
        Console.WriteLine("    Requires unsafe        No                   Yes");
        Console.WriteLine("    LINQ compatible        Yes                  No");
        Console.WriteLine("    Interop with C         No                   Yes (unmanaged)");
        Console.WriteLine();
        Console.WriteLine("    VERDICT: Use Func/Action for 99% of code.  Reserve delegate*");
        Console.WriteLine("    for measured hot paths where delegate overhead is proven.");
        Console.WriteLine();
    }

    // WHY THIS MATTERS:
    // Function pointers are the tool that closed the gap between
    // C# and C for callback-intensive code.  Before C# 9, interop
    // with native callback APIs required marshalling through
    // delegates or IntPtr — function pointers give you direct,
    // type-safe, zero-overhead calls.

    // GOING DEEPER:
    // `delegate* unmanaged` creates a pointer compatible with
    // native calling conventions (cdecl, stdcall, etc.).  Combined
    // with [UnmanagedCallersOnly], you can pass C# static methods
    // directly to native C code as function pointers — no delegate
    // marshalling, no GC handle, no thunks.  This is how the .NET
    // runtime implements its own native-to-managed transitions.

    // ──────────────────────────────────────────────────────────
    // TRY IT
    // ──────────────────────────────────────────────────────────

    // TODO: Add a sepia filter: R = min(255, 0.393*R + 0.769*G + 0.189*B)
    //       G = min(255, 0.349*R + 0.686*G + 0.168*B)
    //       B = min(255, 0.272*R + 0.534*G + 0.131*B)
    //       Call it via both delegate and function pointer.

    // TODO: Benchmark ApplyWithDelegate vs ApplyWithFunctionPointer
    //       using BenchmarkDotNet with 1M pixels.

    // TODO: Try `delegate* unmanaged[Cdecl]<Pixel, Pixel>` and
    //       mark a method with [UnmanagedCallersOnly(CallConvs = ...)]

    /// <summary>Runs the complete function pointers demonstration.</summary>
    public static unsafe void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: Function Pointers (C# 9)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        // Create test pixel data
        Pixel[] pixels =
        [
            new(200, 100, 50),
            new(80, 160, 240),
            new(30, 30, 30),
            new(255, 255, 255),
            new(128, 0, 255),
            new(0, 200, 100),
        ];

        Console.WriteLine($"  Source pixels ({pixels.Length} total):");
        for (int i = 0; i < Math.Min(4, pixels.Length); i++)
            Console.WriteLine($"    [{i}] {pixels[i]} (brightness: {pixels[i].Brightness:F2})");
        Console.WriteLine();

        // Delegate approach
        DemonstrateDelegateApproach(pixels);

        // Function pointer approach
        DemonstrateFunctionPointerApproach(pixels);

        // Pipeline
        DemonstratePipeline(pixels);

        // Comparison
        ShowComparisonTable();

        Console.WriteLine("  KEY TAKEAWAY:");
        Console.WriteLine("  Use delegate* for hot paths only — delegates are fine for");
        Console.WriteLine("  99% of code.  Function pointers trade safety (no null checks,");
        Console.WriteLine("  no closures, no multicast) for zero-overhead invocation.");
        Console.WriteLine();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }

    // ── Helpers ────────────────────────────────────────────────

    private static byte ClampByte(float value) =>
        (byte)Math.Clamp((int)value, 0, 255);
}
