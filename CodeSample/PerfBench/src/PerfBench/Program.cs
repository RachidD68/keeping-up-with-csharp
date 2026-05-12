// ╔══════════════════════════════════════════════════════════╗
// ║  PerfBench — Interactive Demo Runner                                           ║
// ║  Keeping Up with C# — Themes 6, 7 & 10                                       ║
// ║                                                                                                 ║
// ║  Usage:                                                                                     ║
// ║    dotnet run              → interactive menu                                       ║
// ║    dotnet run -- --all     → run all demos sequentially                         ║
// ║    dotnet run -- --bench   → run BenchmarkDotNet suite                     ║
// ╚══════════════════════════════════════════════════════════╝

using PerfBench.Theme6_Memory.Benchmarks;
using PerfBench.Theme6_Memory.MemoryT;
using PerfBench.Theme6_Memory.SpanAndMemory;

var demos = BuildDemoList();

if (args.Contains("--all", StringComparer.OrdinalIgnoreCase))
{
    RunAll(demos);
    return;
}

if (args.Contains("--bench", StringComparer.OrdinalIgnoreCase))
{
    var filteredArgs = args
        .Where(a => !a.Equals("--bench", StringComparison.OrdinalIgnoreCase))
        .ToArray();
    BenchmarkSwitcher
        .FromAssembly(typeof(Program).Assembly)
        .Run(filteredArgs);
    return;
}

RunInteractive(demos);

// ── Demo registry ────────────────────────────────────────────

static List<(string Name, Action Action)> BuildDemoList() =>
[
    // ── Theme 6: Memory & Allocation ─────────────────────────
    ("Theme 6 Intro",                    Theme6Intro.PrintHeader),
    ("Creating Span<T> — 9 Patterns",   CreatingSpanDemo.Run),
    ("Span<T> — Zero-Copy Slicing",     BufferProcessorDemo.Run),
    ("Span<T> API — Properties & Methods", SpanApiDemo.Run),
    ("  \u2514\u2500 Benchmark: Span vs Array",   () => RunBenchmark<SpanBenchmarks>()),
    ("  \u2514\u2500 Benchmark: Date Parsing",    () => RunBenchmark<DateParsingBenchmarks>()),
    ("Memory<T> & MemoryPool<T>",        AsyncMemoryProcessorDemo.Run),
    ("  \u2514\u2500 Benchmark: Memory Pools",    () => RunBenchmark<MemoryPoolBenchmarks>()),
    ("stackalloc in Safe Contexts",      StackAllocDemoRunner.Run),
    ("  \u2514\u2500 Benchmark: Stack vs Heap",   () => RunBenchmark<StackallocBenchmarks>()),
    ("ArrayPool<T>",                     PooledProcessingDemo.Run),
    ("  \u2514\u2500 Benchmark: ArrayPool",       () => RunBenchmark<ArrayPoolBenchmarks>()),
    ("ref Returns & ref Locals",         RefAccessorsDemo.Run),
    ("Interpolated String Handlers",     ZeroAllocFormatterDemo.Run),
    ("  \u2514\u2500 Benchmark: String Handlers", () => RunBenchmark<StringHandlerBenchmarks>()),
    ("FrozenDictionary / FrozenSet",     ImmutableLookupDemo.Run),
    ("  \u2514\u2500 Benchmark: Frozen vs Dict",  () => RunBenchmark<FrozenCollectionBenchmarks>()),
    ("ref Fields in ref Structs",        RefStructContainersDemo.Run),

    // ── Theme 7: Low-Level & Interop ─────────────────────────
    ("Theme 7 Intro",                    Theme7Intro.PrintHeader),
    ("Unsafe Code & Pointers",           PointerArithmeticDemo.Run),
    ("Unsafe Utilities Class",           UnsafeUtilitiesDemo.Run),
    ("P/Invoke & LibraryImport",         NativeInteropDemo.Run),
    ("StructLayout & Binary Protocols",  BinaryProtocolDemo.Run),
    ("Function Pointers",               HighPerfCallbacksDemo.Run),

    // ── Theme 10: Capstone Patterns ──────────────────────────
    ("Capstone: Zero-Allocation Pipeline",   ZeroAllocPipelineDemo.Run),
    ("Capstone: Native Interop Wrapper",     NativeInteropWrapperDemo.Run),
];

static void RunBenchmark<T>() where T : class
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"Running benchmark: {typeof(T).Name}");
    Console.WriteLine("(Release configuration recommended for accurate results)\n");
    Console.ResetColor();

    BenchmarkRunner.Run<T>();
}

// ── Interactive menu ─────────────────────────────────────────

static void RunInteractive(List<(string Name, Action Action)> demos)
{
    while (true)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("╔══════════════════════════════════════════════════╗");
        Console.WriteLine("║        PerfBench — Demo Runner                                      ║");
        Console.WriteLine("║   Memory · Allocation · Low-Level · Interop                       ║");
        Console.WriteLine("╚══════════════════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine();

        for (var i = 0; i < demos.Count; i++)
        {
            bool isBenchmark = demos[i].Name.Contains("Benchmark");
            var prefix = demos[i].Name.StartsWith("Theme")
                ? "\n  "
                : demos[i].Name.StartsWith("Capstone")
                    ? "\n  "
                    : "  ";

            if (isBenchmark)
                Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"{prefix}{i + 1,2}. {demos[i].Name}");
            if (isBenchmark)
                Console.ResetColor();
        }

        Console.WriteLine($"\n   0. Exit");
        Console.Write($"\nSelect demo [1–{demos.Count}]: ");

        var input = Console.ReadLine()?.Trim();
        if (input is null or "0" or "q" or "Q")
            break;

        if (int.TryParse(input, out var choice) && choice >= 1 && choice <= demos.Count)
        {
            Console.Clear();
            try
            {
                demos[choice - 1].Action();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n  Error: {ex.Message}");
                Console.ResetColor();
            }
            Console.WriteLine("\n  Press any key to return to menu...");
            Console.ReadKey(true);
        }
    }
}

// ── Run all demos sequentially ───────────────────────────────

static void RunAll(List<(string Name, Action Action)> demos)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("╔══════════════════════════════════════════════════╗");
    Console.WriteLine("║        PerfBench — Running All Demos                               ║");
    Console.WriteLine("╚══════════════════════════════════════════════════╝");
    Console.ResetColor();

    for (int i = 0; i < demos.Count; i++)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"\n{"".PadRight(60, '─')}");
        Console.WriteLine($" [{i + 1}/{demos.Count}] {demos[i].Name}");
        Console.WriteLine($"{"".PadRight(60, '─')}");
        Console.ResetColor();

        try
        {
            demos[i].Action();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  Error: {ex.Message}");
            Console.ResetColor();
        }
    }

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"\n{"".PadRight(60, '═')}");
    Console.WriteLine($" All {demos.Count} demos complete!");
    Console.WriteLine($"{"".PadRight(60, '═')}");
    Console.ResetColor();
}
