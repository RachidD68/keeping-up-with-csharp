namespace ChatStream.Theme5_Safety;

// ╔══════════════════════════════════════════════════════════════════╗
// ║  Theme 5 — Safety & Robustness                                ║
// ║                                                                ║
// ║  C# has steadily evolved from "trust the developer" toward     ║
// ║  "make invalid states unrepresentable."  Nullable reference     ║
// ║  types (C# 8) were the watershed — for the first time the      ║
// ║  compiler could warn about potential null dereferences.  Each   ║
// ║  subsequent version tightened the safety net:                   ║
// ║                                                                ║
// ║  ● C# 6  — Null-conditional (?.) and exception filters         ║
// ║  ● C# 8  — Nullable reference types, null-coalescing assign    ║
// ║  ● C# 10 — CallerArgumentExpression for richer guards          ║
// ║  ● C# 11 — Checked user-defined operators                      ║
// ║  ● C# 13 — System.Threading.Lock for type-safe locking         ║
// ║  ● C# 14 — Null-conditional assignment (?.=)                   ║
// ║                                                                ║
// ║  In ChatStream, every feature protects real domain objects:     ║
// ║  messages, users, channels, and connections.                    ║
// ╚══════════════════════════════════════════════════════════════════╝

/// <summary>Theme 5 introduction and metadata.</summary>
public static class Theme5Intro
{
    public static string Title => "Theme 5 — Safety & Robustness";

    public static string Tagline =>
        "Make invalid states unrepresentable and null dereferences impossible.";

    public static void PrintHeader()
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(new string('═', 60));
        Console.WriteLine($"  {Title}");
        Console.WriteLine($"  {Tagline}");
        Console.WriteLine(new string('═', 60));
        Console.ResetColor();
    }
}
