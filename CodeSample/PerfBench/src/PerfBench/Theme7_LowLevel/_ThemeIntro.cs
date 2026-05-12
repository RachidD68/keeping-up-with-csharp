namespace PerfBench.Theme7_LowLevel;

// ╔══════════════════════════════════════════════════════════════════╗
// ║  Theme 7 — Low-Level & Interop                                 ║
// ║                                                                  ║
// ║  C# started as a managed-only language, but modern workloads    ║
// ║  demand the ability to reach below the abstraction layer when   ║
// ║  performance or native integration requires it.  Theme 7        ║
// ║  explores the features that let you work at the metal while     ║
// ║  staying within the managed ecosystem:                          ║
// ║                                                                  ║
// ║  - Unsafe code & pointers    — direct memory manipulation       ║
// ║  - Unsafe class (S.R.C.S)    — safe wrappers for unsafe ops     ║
// ║  - P/Invoke & LibraryImport  — calling native C/OS libraries    ║
// ║  - StructLayout & FieldOffset — precise binary layouts          ║
// ║  - Function pointers         — zero-alloc, value-type callbacks ║
// ║                                                                  ║
// ║  In PerfBench, every feature operates on real domain models:    ║
// ║  Pixel arrays for image processing, PacketHeaders for binary    ║
// ║  protocols, SensorReadings for telemetry, and MemoryRegions     ║
// ║  for allocation tracking.                                       ║
// ║                                                                  ║
// ║  The golden rule: prefer safe abstractions (Span<T>, Memory<T>) ║
// ║  for 99% of code.  Drop to low-level only when you have         ║
// ║  measured proof that the abstraction cost is unacceptable.      ║
// ╚══════════════════════════════════════════════════════════════════╝

/// <summary>Theme 7 introduction and metadata.</summary>
public static class Theme7Intro
{
    public static string Title => "Theme 7 — Low-Level & Interop";

    public static string Tagline =>
        "Reach below the abstraction layer — carefully, deliberately, and only when you must.";

    public static void PrintHeader()
    {
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine(new string('=', 64));
        Console.WriteLine($"  {Title}");
        Console.WriteLine($"  {Tagline}");
        Console.WriteLine(new string('=', 64));
        Console.ResetColor();
    }
}
