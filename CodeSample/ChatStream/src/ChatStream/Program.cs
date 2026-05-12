// ╔══════════════════════════════════════════════════════════╗
// ║  ChatStream — Interactive Demo Runner                   ║
// ║  Keeping Up with C# — Themes 5, 8 & 10                 ║
// ║                                                          ║
// ║  Usage:                                                  ║
// ║    dotnet run              → interactive menu            ║
// ║    dotnet run -- --all     → run all demos sequentially  ║
// ╚══════════════════════════════════════════════════════════╝

var demos = BuildDemoList();

if (args.Contains("--all", StringComparer.OrdinalIgnoreCase))
{
    await RunAllAsync(demos);
    return;
}

await RunInteractiveAsync(demos);

// ── Demo registry ────────────────────────────────────────────

static List<(string Name, Func<Task> Action)> BuildDemoList() =>
[
    // ── Theme 5: Safety & Robustness ─────────────────────────
    ("Theme 5 Intro",                 () => { Theme5Intro.PrintHeader(); return Task.CompletedTask; }),
    ("Nullable Reference Types",      () => { SafeMessageApiDemo.Run(); return Task.CompletedTask; }),
    ("Exception Filters",             () => { SmartErrorHandlerDemo.Run(); return Task.CompletedTask; }),
    ("Null-Conditional Operators",    () => { OptionalChainingDemo.Run(); return Task.CompletedTask; }),
    ("Null-Coalescing Assignment",    () => { DefaultValuesDemo.Run(); return Task.CompletedTask; }),
    ("Null-Conditional Assignment",   () => { ConditionalUpdateDemo.Run(); return Task.CompletedTask; }),
    ("Checked User-Defined Operators",() => { SafeCounterDemo.Run(); return Task.CompletedTask; }),
    ("CallerArgumentExpression Guard",() => { GuardDemo.Run(); return Task.CompletedTask; }),
    ("System.Threading.Lock",        () => { ThreadSafeRoomDemo.Run(); return Task.CompletedTask; }),

    // ── Theme 8: Async, Concurrency & Streaming ──────────────
    ("Theme 8 Intro",                 () => { Theme8Intro.PrintHeader(); return Task.CompletedTask; }),
    ("async / await",                 () => BasicMessagingDemo.RunAsync()),
    ("Async State Machine",           () => StateMachineExplorerDemo.RunAsync()),
    ("ValueTask<T>",                  () => CachedLookupDemo.RunAsync()),
    ("IAsyncEnumerable<T>",           () => MessageStreamDemo.RunAsync()),
    ("IAsyncDisposable",              () => ConnectionManagerDemo.RunAsync()),
    ("ConfigureAwait",                () => ContextExplorerDemo.RunAsync()),
    ("Cancellation & Shutdown",       () => GracefulShutdownDemo.RunAsync()),
    ("Task Combinators",              () => ParallelFetchDemo.RunAsync()),
    ("System.Threading.Channels",     () => MessageBusDemo.RunAsync()),

    // ── Theme 10: Capstone Patterns ──────────────────────────
    ("Capstone: Null-Safe API Pattern",  () => { NullSafeApiPatternDemo.Run(); return Task.CompletedTask; }),
    ("Capstone: Async Pipeline Pattern", () => AsyncPipelinePatternDemo.RunAsync()),
];

// ── Interactive menu ─────────────────────────────────────────

static async Task RunInteractiveAsync(List<(string Name, Func<Task> Action)> demos)
{
    while (true)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("╔══════════════════════════════════════════════════╗");
        Console.WriteLine("║        ChatStream — Demo Runner                 ║");
        Console.WriteLine("║   Safety · Async · Concurrency · Streaming      ║");
        Console.WriteLine("╚══════════════════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine();

        for (int i = 0; i < demos.Count; i++)
        {
            var prefix = demos[i].Name.StartsWith("Theme")
                ? "\n  "
                : demos[i].Name.StartsWith("Capstone")
                    ? "\n  "
                    : "  ";
            Console.WriteLine($"{prefix}{i + 1,2}. {demos[i].Name}");
        }

        Console.WriteLine($"\n   0. Exit");
        Console.Write($"\nSelect demo [1–{demos.Count}]: ");

        var input = Console.ReadLine()?.Trim();
        if (input is null or "0" or "q" or "Q")
            break;

        if (int.TryParse(input, out int choice) && choice >= 1 && choice <= demos.Count)
        {
            Console.Clear();
            try
            {
                await demos[choice - 1].Action();
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

static async Task RunAllAsync(List<(string Name, Func<Task> Action)> demos)
{
    Console.ForegroundColor = ConsoleColor.Magenta;
    Console.WriteLine("╔══════════════════════════════════════════════════╗");
    Console.WriteLine("║        ChatStream — Running All Demos           ║");
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
            await demos[i].Action();
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
