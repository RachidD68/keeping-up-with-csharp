namespace PerfBench.Theme7_LowLevel;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: Unsafe Code & Pointers  (classic, evolved)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  PROBLEM
//  -------
//  Some low-level operations require direct memory access
//  that the safe type system cannot express: scanning pixel
//  buffers, manipulating hardware-mapped memory, or calling
//  into native code that expects raw pointers.
//
//  SOLUTION
//  --------
//  The `unsafe` keyword unlocks C-style pointers within C#.
//  Combined with the `fixed` statement (which pins a managed
//  object so the GC won't move it), you can perform pointer
//  arithmetic, dereference memory, and use `sizeof` — all
//  within a managed application.
//
//  WHY IT MATTERS
//  ──────────────
//  Pointer access can eliminate bounds checks and enable
//  algorithms that operate at native speed.  However, modern
//  C# offers Span<T> and ref-returns that deliver nearly the
//  same performance without leaving the safe world.  Unsafe
//  code is a last resort, not a first choice.
//
//  TRY IT
//  ──────
//  1. Change the brightness factor and observe the output.
//  2. Remove the `fixed` statement — observe the compiler error.
//  3. Try the Span version and compare the clarity vs pointer version.
//  4. Add a "grayscale" filter using pointer arithmetic.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Demonstrates unsafe pointer arithmetic on a Pixel array,
/// then contrasts it with the safe Span-based equivalent.
/// </summary>
public static class PointerArithmeticDemo
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — Safe indexer-based pixel processing
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Before: iterate with a standard indexer.
    /// Each access performs a bounds check — safe but not the
    /// fastest option for hot image-processing loops.
    /// </summary>
    private static Pixel[] ApplyBrightnessWithIndexer(Pixel[] pixels, float factor)
    {
        Console.WriteLine("  BEFORE (Safe indexer — bounds-checked access):");
        Console.WriteLine();

        var result = new Pixel[pixels.Length];

        for (int i = 0; i < pixels.Length; i++)
        {
            var p = pixels[i]; // bounds check on every access
            result[i] = new Pixel(
                ClampByte(p.R * factor),
                ClampByte(p.G * factor),
                ClampByte(p.B * factor),
                p.A);
        }

        Console.WriteLine($"    Processed {pixels.Length} pixels via indexer (bounds check per access)");
        PrintFirstPixels(result, "    ");
        Console.WriteLine();
        return result;
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — Unsafe pointer scan
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// After: pin the array and walk it with a raw pointer.
    /// No bounds checks, no managed overhead — but you own
    /// every byte and every potential segfault.
    /// </summary>
    private static unsafe Pixel[] ApplyBrightnessWithPointers(Pixel[] pixels, float factor)
    {
        Console.WriteLine("  AFTER (Unsafe pointer scan — no bounds checks):");
        Console.WriteLine();

        var result = new Pixel[pixels.Length];

        // `fixed` pins the managed array so the GC cannot relocate it.
        // The pointer is valid only inside this block.
        fixed (Pixel* srcBase = pixels)
        fixed (Pixel* dstBase = result)
        {
            Pixel* src = srcBase;
            Pixel* dst = dstBase;
            Pixel* end = srcBase + pixels.Length; // pointer arithmetic: + N means + N * sizeof(Pixel)

            Console.WriteLine($"    sizeof(Pixel)  = {sizeof(Pixel)} bytes");
            Console.WriteLine($"    Array base addr = 0x{(nint)srcBase:X}");
            Console.WriteLine($"    End addr        = 0x{(nint)end:X}");
            Console.WriteLine($"    Total bytes     = {pixels.Length * sizeof(Pixel)}");
            Console.WriteLine();

            while (src < end)
            {
                // Dereference the source pointer, transform, write to dest
                Pixel p = *src;
                *dst = new Pixel(
                    ClampByte(p.R * factor),
                    ClampByte(p.G * factor),
                    ClampByte(p.B * factor),
                    p.A);

                src++; // advance by sizeof(Pixel) bytes
                dst++;
            }
        }
        // Outside `fixed`: pointers are no longer valid.
        // The GC is free to move the arrays again.

        Console.WriteLine($"    Processed {pixels.Length} pixels via pointer scan");
        PrintFirstPixels(result, "    ");
        Console.WriteLine();
        return result;
    }

    // ──────────────────────────────────────────────────────────
    // STEP 3: THE BETTER WAY — Span<T> (prefer this!)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Modern approach: Span gives you pointer-like performance
    /// with bounds safety.  Prefer this over raw pointers in
    /// virtually all scenarios.
    /// </summary>
    private static Pixel[] ApplyBrightnessWithSpan(Pixel[] pixels, float factor)
    {
        Console.WriteLine("  MODERN (Span<T> — safe AND fast):");
        Console.WriteLine();

        var result = new Pixel[pixels.Length];
        Span<Pixel> src = pixels;
        Span<Pixel> dst = result;

        // Span access is bounds-checked in Debug, elided by JIT in Release.
        // You get safety without sacrificing performance.
        for (int i = 0; i < src.Length; i++)
        {
            ref readonly Pixel p = ref src[i];
            dst[i] = new Pixel(
                ClampByte(p.R * factor),
                ClampByte(p.G * factor),
                ClampByte(p.B * factor),
                p.A);
        }

        Console.WriteLine($"    Processed {pixels.Length} pixels via Span<Pixel>");
        Console.WriteLine("    -> Bounds-checked in Debug, JIT-optimized in Release");
        Console.WriteLine("    -> No `unsafe` keyword required");
        PrintFirstPixels(result, "    ");
        Console.WriteLine();
        return result;
    }

    // WHY THIS MATTERS:
    // Pointers give you raw speed but remove every safety net.
    // A single off-by-one error can corrupt memory silently.
    // Span<T> delivers the same performance profile in Release
    // builds while keeping bounds checks in Debug — the best
    // of both worlds.  Use unsafe only when Span is genuinely
    // insufficient (e.g., interop with native APIs that require
    // raw pointers, or SIMD intrinsics).

    // GOING DEEPER:
    // The `fixed` statement generates a GCHandle pin. Pinning
    // too many objects fragments the GC heap — avoid pinning
    // in tight loops.  For long-lived pinned buffers, consider
    // allocating with GC.AllocateArray<T>(length, pinned: true)
    // or NativeMemory.Alloc.

    // ──────────────────────────────────────────────────────────
    // TRY IT
    // ──────────────────────────────────────────────────────────

    // TODO: Add a grayscale filter using pointer arithmetic.
    //       For each pixel: gray = (byte)(0.299*R + 0.587*G + 0.114*B)
    //       Then set R = G = B = gray.

    // TODO: Try using GC.AllocateArray<Pixel>(length, pinned: true)
    //       instead of `fixed` — does it simplify the code?

    /// <summary>Runs the complete unsafe code demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: Unsafe Code & Pointers");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        // Create a small pixel buffer simulating an image strip
        Pixel[] pixels =
        [
            new(200, 100, 50),
            new(80, 160, 240),
            new(30, 30, 30),
            new(255, 255, 255),
            new(128, 0, 255),
            new(0, 200, 100),
            new(220, 180, 60),
            new(10, 10, 10),
        ];

        Console.WriteLine($"  Source pixels ({pixels.Length} total):");
        PrintFirstPixels(pixels, "  ");
        Console.WriteLine();

        const float brightnessFactor = 1.5f;
        Console.WriteLine($"  Applying brightness factor: {brightnessFactor:F1}x");
        Console.WriteLine();

        // 1. Safe indexer approach
        ApplyBrightnessWithIndexer(pixels, brightnessFactor);

        // 2. Unsafe pointer approach
        ApplyBrightnessWithPointers(pixels, brightnessFactor);

        // 3. Span-based approach (recommended)
        ApplyBrightnessWithSpan(pixels, brightnessFactor);

        // Key takeaway
        Console.WriteLine("  KEY TAKEAWAY:");
        Console.WriteLine("  Prefer Span<T> — use unsafe only when Span isn't enough.");
        Console.WriteLine("  Pointers bypass bounds checks, but Span is JIT-optimized");
        Console.WriteLine("  to the same speed in Release builds with safety retained.");
        Console.WriteLine();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }

    // ── Helpers ────────────────────────────────────────────────

    private static byte ClampByte(float value) =>
        (byte)Math.Clamp((int)value, 0, 255);

    private static void PrintFirstPixels(Pixel[] pixels, string indent, int count = 4)
    {
        int show = Math.Min(count, pixels.Length);
        for (int i = 0; i < show; i++)
            Console.WriteLine($"{indent}  [{i}] {pixels[i]} (brightness: {pixels[i].Brightness:F2})");
        if (pixels.Length > show)
            Console.WriteLine($"{indent}  ... and {pixels.Length - show} more");
    }
}
