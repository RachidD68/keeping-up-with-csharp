// ╔══════════════════════════════════════════════════════════╗
// ║  TypeForge — Interactive Demo Runner                    ║
// ║  Keeping Up with C# — Themes 4 & 10                    ║
// ║                                                          ║
// ║  Usage:                                                  ║
// ║    dotnet run              → interactive menu            ║
// ║    dotnet run -- --all     → run all demos sequentially  ║
// ╚══════════════════════════════════════════════════════════╝

var demos = BuildDemoList();

if (args.Contains("--all", StringComparer.OrdinalIgnoreCase))
{
    RunAll(demos);
    return;
}

RunInteractive(demos);

// ── Demo registry ────────────────────────────────────────────

static List<(string Name, Action Action)> BuildDemoList() =>
[
    // ── Theme 4: Type System & OOP Flexibility ───────────────
    ("Theme 4 Intro",                      Theme4Intro.PrintHeader),
    ("Default Interface Methods",           PluginDefaultsDemo.Run),
    ("Static Abstract / Virtual Members",   ShapeFactoryDemo.Run),
    ("Generic Math (INumber<T>)",           DimensionCalculatorDemo.Run),
    ("Interface Hierarchies & Composition", TypeHierarchyDemo.Run),
    ("Sealed Type Hierarchies",             TypeNodeWalkerDemo.Run),
    ("File-Scoped Types",                   InternalHelpersDemo.Run),
    ("ref struct Interfaces",               StackOnlyParserDemo.Run),
    ("Inline Arrays",                       FixedBufferPluginDemo.Run),
    ("Extension Members",                   ShapeExtensionsDemo.Run),
    ("Abstract Classes & Virtual Dispatch", TypeVisitorDemo.Run),
    ("Operator Overloading & Conversions",  UnitArithmeticDemo.Run),
    ("Generic Constraints Showcase",        ConstraintShowcaseDemo.Run),

    // ── Theme 10: Capstone Pattern ───────────────────────────
    ("Capstone: Extensible Plugin Architecture", ExtensiblePluginPatternDemo.Run),
];

// ── Interactive menu ─────────────────────────────────────────

static void RunInteractive(List<(string Name, Action Action)> demos)
{
    while (true)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("╔══════════════════════════════════════════════════╗");
        Console.WriteLine("║        TypeForge — Demo Runner                  ║");
        Console.WriteLine("║   Type System · OOP · Generics · Extensions     ║");
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
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("╔══════════════════════════════════════════════════╗");
    Console.WriteLine("║        TypeForge — Running All Demos            ║");
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
