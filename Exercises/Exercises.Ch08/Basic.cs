// Chapter 8 — Low-Level & Interop Primitives — BASIC
// ----------------------------------------------------------------
// Exercise: Convert PerfBench's QueryPerformanceCounter /
//   QueryPerformanceFrequency [DllImport] declarations to
//   [LibraryImport]. Rebuild, open the generated file under
//   obj/Debug/net10.0/generated/.../LibraryImportGenerator/, and
//   read the generated marshalling code.
//
// Hint: The methods become partial and drop the CharSet parameter.
// ----------------------------------------------------------------
//
// Note: [LibraryImport] requires the containing type to be `partial`
// and the method to be `partial extern`. The Roslyn source generator
// emits the actual marshalling logic into obj/.../generated/.

using System.Runtime.InteropServices;
using System.Diagnostics;

namespace KeepUpCs.Exercises.Ch08;

// ── BEFORE (C# 8 era — runtime marshalling via [DllImport]) ─────
internal static class WinApiOld
{
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool QueryPerformanceCounter(out long count);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool QueryPerformanceFrequency(out long frequency);
}

// ── AFTER (.NET 7+ — source-generated marshalling via [LibraryImport]) ─
internal static partial class WinApiNew
{
    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool QueryPerformanceCounter(out long count);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool QueryPerformanceFrequency(out long frequency);
}

public static class BasicDemo
{
    public static void Run()
    {
        Console.WriteLine("Ch08 Basic — [DllImport] → [LibraryImport]");
        Console.WriteLine(new string('─', 60));

        if (!OperatingSystem.IsWindows())
        {
            Console.WriteLine("  (kernel32.dll is Windows-only — using Stopwatch as a fallback below.)");
            Console.WriteLine($"  Stopwatch.GetTimestamp(): {Stopwatch.GetTimestamp()}");
            Console.WriteLine($"  Stopwatch.Frequency:      {Stopwatch.Frequency} ticks/sec");
            return;
        }

        if (WinApiNew.QueryPerformanceFrequency(out long freq) &&
            WinApiNew.QueryPerformanceCounter(out long t1))
        {
            // Tiny CPU-bound delay between two QPC reads.
            int sum = 0;
            for (int i = 0; i < 1_000_000; i++) sum += i;
            WinApiNew.QueryPerformanceCounter(out long t2);

            double micros = (t2 - t1) * 1_000_000.0 / freq;
            Console.WriteLine($"  Frequency:  {freq:N0} ticks/sec");
            Console.WriteLine($"  Elapsed:    {micros:F2} µs (between QPC reads)");
            Console.WriteLine($"  Discard:    {sum}");
        }
        Console.WriteLine();
        Console.WriteLine("  Tip: look in obj/Debug/net10.0/generated/Microsoft.Interop.LibraryImportGenerator/");
        Console.WriteLine("  to read the marshalling code the source generator emitted for each method.");
    }
}
