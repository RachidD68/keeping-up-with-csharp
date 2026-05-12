namespace PerfBench.Theme6_Memory;

// ╔══════════════════════════════════════════════════════════════════╗
// ║  Theme 6 — Memory & Allocation                                 ║
// ║                                                                ║
// ║  Heap allocations are the silent tax on managed code.  Every   ║
// ║  `new byte[]` eventually becomes GC work, and GC pauses are    ║
// ║  the enemy of latency-sensitive applications.  Modern C# and   ║
// ║  .NET provide a rich toolkit for controlling where memory       ║
// ║  lives, how long it lasts, and whether copies ever happen:     ║
// ║                                                                ║
// ║  ● C# 7    — ref returns and ref locals                       ║
// ║  ● C# 7.2  — Span<T>, stackalloc in safe contexts             ║
// ║  ● C# 7.2  — Memory<T>, MemoryPool<T>, IMemoryOwner<T>       ║
// ║  ● C# 8    — stackalloc in nested expressions                 ║
// ║  ● C# 10   — Interpolated string handlers                     ║
// ║  ● C# 11   — ref fields in ref structs                        ║
// ║  ● .NET 6  — ArrayPool<T> improvements                        ║
// ║  ● .NET 8  — FrozenDictionary / FrozenSet                     ║
// ║                                                                ║
// ║  In PerfBench, every feature uses real domain models:          ║
// ║  Pixel, Matrix, SensorReading, PacketHeader, MemoryRegion.     ║
// ╚══════════════════════════════════════════════════════════════════╝

/// <summary>Theme 6 introduction and metadata.</summary>
public static class Theme6Intro
{
    public static string Title => "Theme 6 — Memory & Allocation";

    public static string Tagline =>
        "Zero-copy views, stack allocation, pooling, frozen lookups, and BenchmarkDotNet — control every byte.";

    public static void PrintHeader()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(new string('=', 60));
        Console.WriteLine($"  {Title}");
        Console.WriteLine($"  {Tagline}");
        Console.WriteLine(new string('=', 60));
        Console.ResetColor();
    }
}
