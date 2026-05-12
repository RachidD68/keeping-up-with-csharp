namespace PerfBench.Theme7_LowLevel;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: System.Runtime.CompilerServices.Unsafe class
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  PROBLEM
//  -------
//  Many low-level operations — reinterpret casts, computing
//  struct sizes, reading unaligned memory — traditionally
//  required the `unsafe` keyword and pointer syntax.  This
//  meant entire methods (or classes) had to be marked unsafe,
//  even when only a single operation needed it.
//
//  SOLUTION
//  --------
//  The Unsafe class (System.Runtime.CompilerServices.Unsafe)
//  provides safe-looking wrappers for unsafe operations.  You
//  get Unsafe.As<TFrom, TTo> for reinterpret casts,
//  Unsafe.SizeOf<T> for struct sizes, Unsafe.Add for pointer-
//  like arithmetic, and Unsafe.ReadUnaligned/WriteUnaligned
//  for raw byte access — all without the `unsafe` keyword.
//
//  WHY IT MATTERS
//  ──────────────
//  Library authors (including the .NET runtime itself) use
//  these methods heavily to write high-performance code that
//  compiles without AllowUnsafeBlocks.  Understanding them
//  is essential for reading runtime source code and writing
//  your own zero-allocation utilities.
//
//  TRY IT
//  ──────
//  1. Add a new struct (e.g., Color3 with R/G/B but no A)
//     and reinterpret-cast it to/from a Pixel with Unsafe.As.
//  2. Use Unsafe.Add to iterate a Span by ref instead of index.
//  3. Explore Unsafe.IsAddressGreaterThan / IsAddressLessThan.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Demonstrates the Unsafe utility class for reinterpret casts,
/// struct sizing, identity checks, and unaligned memory access.
/// </summary>
public static class UnsafeUtilitiesDemo
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — Traditional unsafe reinterpret cast
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Before: reinterpret-casting a struct required unsafe code
    /// with pointer dereferencing, making the method unsafe.
    /// </summary>
    private static unsafe void ReinterpretWithPointers()
    {
        Console.WriteLine("  BEFORE (unsafe keyword + pointer cast):");
        Console.WriteLine();

        var pixel = new Pixel(0xAA, 0xBB, 0xCC, 0xFF);

        // Traditional pointer-based reinterpret cast
        uint packed;
        Pixel* pp = &pixel;
        packed = *(uint*)pp;

        Console.WriteLine($"    Pixel:  {pixel}");
        Console.WriteLine($"    As uint (pointer cast): 0x{packed:X8}");
        Console.WriteLine($"    Matches PackedValue:    0x{pixel.PackedValue:X8}");
        Console.WriteLine($"    -> Requires `unsafe` keyword on the method");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — Unsafe.As<TFrom, TTo>
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// After: Unsafe.As reinterprets the bits without needing
    /// the `unsafe` keyword.  No allocation, no copy — it is
    /// a JIT intrinsic that compiles to zero instructions.
    /// </summary>
    private static void ReinterpretWithUnsafeAs()
    {
        Console.WriteLine("  AFTER (Unsafe.As<TFrom, TTo> — no unsafe keyword):");
        Console.WriteLine();

        var pixel = new Pixel(0xAA, 0xBB, 0xCC, 0xFF);

        // Reinterpret the Pixel's bytes as a uint — zero-copy, zero-alloc
        uint packed = Unsafe.As<Pixel, uint>(ref Unsafe.AsRef(in pixel));

        Console.WriteLine($"    Pixel:  {pixel}");
        Console.WriteLine($"    As uint (Unsafe.As):    0x{packed:X8}");
        Console.WriteLine($"    Matches PackedValue:    0x{pixel.PackedValue:X8}");
        Console.WriteLine();

        // Round-trip: uint back to Pixel
        var roundTripped = Unsafe.As<uint, Pixel>(ref packed);
        Console.WriteLine($"    Round-tripped back:     {roundTripped}");
        Console.WriteLine($"    Values match: {pixel == roundTripped}");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 3: Unsafe.SizeOf<T> — struct sizing
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Unsafe.SizeOf returns the runtime size of an unmanaged
    /// struct, accounting for padding and alignment.
    /// </summary>
    private static void DemonstrateSizeOf()
    {
        Console.WriteLine("  Unsafe.SizeOf<T> — Runtime struct sizes:");
        Console.WriteLine();

        Console.WriteLine($"    Pixel:         {Unsafe.SizeOf<Pixel>()} bytes  (R,G,B,A = 4 x byte)");
        Console.WriteLine($"    PacketHeader:  {Unsafe.SizeOf<PacketHeader>()} bytes (explicit layout, Size=16)");
        Console.WriteLine($"    SensorReading: {Unsafe.SizeOf<SensorReading>()} bytes (int + long + double + byte, Pack=1)");
        Console.WriteLine($"    int:           {Unsafe.SizeOf<int>()} bytes");
        Console.WriteLine($"    double:        {Unsafe.SizeOf<double>()} bytes");
        Console.WriteLine($"    Guid:          {Unsafe.SizeOf<Guid>()} bytes");
        Console.WriteLine();

        // Compare with Marshal.SizeOf (which reports marshalled size, can differ)
        Console.WriteLine($"    Marshal.SizeOf<Pixel>():  {Marshal.SizeOf<Pixel>()} bytes (marshalled size)");
        Console.WriteLine($"    Note: Unsafe.SizeOf and Marshal.SizeOf may differ for");
        Console.WriteLine($"    structs with explicit layout or CharSet attributes.");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 4: Unsafe.IsNullRef / Unsafe.AreSame — identity checks
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Demonstrates reference identity checks using Unsafe methods.
    /// These are useful for implementing ref-returning APIs and
    /// sentinel patterns.
    /// </summary>
    private static void DemonstrateIdentityChecks()
    {
        Console.WriteLine("  Unsafe.IsNullRef / Unsafe.AreSame — reference identity:");
        Console.WriteLine();

        Pixel[] pixels = [new(10, 20, 30), new(40, 50, 60), new(70, 80, 90)];

        ref Pixel first = ref pixels[0];
        ref Pixel alsoFirst = ref pixels[0];
        ref Pixel second = ref pixels[1];

        // AreSame: do two refs point to the exact same memory location?
        bool sameRef = Unsafe.AreSame(ref first, ref alsoFirst);
        bool diffRef = Unsafe.AreSame(ref first, ref second);
        Console.WriteLine($"    first & alsoFirst same location: {sameRef}  (expected: True)");
        Console.WriteLine($"    first & second same location:    {diffRef}  (expected: False)");
        Console.WriteLine();

        // IsNullRef: check for a null reference (ref Unsafe.NullRef<T>())
        ref Pixel nullRef = ref Unsafe.NullRef<Pixel>();
        Console.WriteLine($"    Unsafe.IsNullRef(nullRef):  {Unsafe.IsNullRef(ref nullRef)}  (expected: True)");
        Console.WriteLine($"    Unsafe.IsNullRef(first):    {Unsafe.IsNullRef(ref first)}  (expected: False)");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 5: Unsafe.ReadUnaligned / WriteUnaligned
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Demonstrates reading and writing structs from arbitrary
    /// byte positions that may not be naturally aligned.
    /// </summary>
    private static void DemonstrateUnalignedAccess()
    {
        Console.WriteLine("  Unsafe.ReadUnaligned / WriteUnaligned — raw byte access:");
        Console.WriteLine();

        // Write a Pixel to a byte buffer at an unaligned offset
        byte[] buffer = new byte[32];
        var pixel = new Pixel(0xDE, 0xAD, 0xBE, 0xEF);

        // Write at offset 3 (not aligned to 4 bytes)
        const int offset = 3;
        Unsafe.WriteUnaligned(ref buffer[offset], pixel);

        Console.WriteLine($"    Wrote {pixel} at offset {offset} (unaligned)");
        Console.Write("    Buffer bytes: ");
        for (int i = 0; i < 12; i++)
            Console.Write($"{buffer[i]:X2} ");
        Console.WriteLine();

        // Read it back from the same unaligned position
        var readBack = Unsafe.ReadUnaligned<Pixel>(ref buffer[offset]);
        Console.WriteLine($"    Read back:    {readBack}");
        Console.WriteLine($"    Round-trip:   {pixel == readBack}");
        Console.WriteLine();

        // Practical use: SensorReading from a byte stream at arbitrary offset
        var reading = SensorReading.Create(42, 98.6, SensorStatus.Normal);
        byte[] stream = new byte[64];
        int readingOffset = 5; // intentionally unaligned

        Unsafe.WriteUnaligned(ref stream[readingOffset], reading);
        var restored = Unsafe.ReadUnaligned<SensorReading>(ref stream[readingOffset]);

        Console.WriteLine($"    SensorReading round-trip at offset {readingOffset}:");
        Console.WriteLine($"      Original: {reading}");
        Console.WriteLine($"      Restored: {restored}");
        Console.WriteLine($"      Match:    {reading == restored}");
        Console.WriteLine();
    }

    // WHY THIS MATTERS:
    // The Unsafe class is how the .NET runtime itself implements
    // Span<T>, MemoryMarshal, and dozens of hot-path optimizations.
    // These methods compile to zero or near-zero IL — they are JIT
    // intrinsics that map directly to CPU instructions.  Knowing
    // them lets you read the runtime source code and write libraries
    // that match its performance.

    // GOING DEEPER:
    // Unsafe.As does NOT validate that TFrom and TTo have the same
    // size.  If you cast a 4-byte Pixel to an 8-byte long, you will
    // read adjacent memory — this is undefined behavior.  Always
    // verify sizes match with Unsafe.SizeOf before reinterpreting.
    // For validated reinterpretation, prefer MemoryMarshal.Cast<TFrom, TTo>.

    // ──────────────────────────────────────────────────────────
    // TRY IT
    // ──────────────────────────────────────────────────────────

    // TODO: Use Unsafe.Add<T>(ref T, int offset) to iterate a
    //       Span<Pixel> by ref instead of by index.  Compare
    //       the generated assembly in SharpLab.

    // TODO: Create a "PixelBatch" that stores 4 Pixels contiguously
    //       and use Unsafe.As to reinterpret it as Span<uint> for
    //       bulk operations.

    /// <summary>Runs the complete Unsafe class demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: Unsafe Class (System.Runtime.CompilerServices)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        ReinterpretWithPointers();
        ReinterpretWithUnsafeAs();
        DemonstrateSizeOf();
        DemonstrateIdentityChecks();
        DemonstrateUnalignedAccess();

        Console.WriteLine("  KEY TAKEAWAY:");
        Console.WriteLine("  Unsafe class methods are JIT intrinsics — zero overhead.");
        Console.WriteLine("  They let library authors avoid the `unsafe` keyword while");
        Console.WriteLine("  still performing low-level operations.  Always validate");
        Console.WriteLine("  sizes before reinterpreting memory.");
        Console.WriteLine();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
