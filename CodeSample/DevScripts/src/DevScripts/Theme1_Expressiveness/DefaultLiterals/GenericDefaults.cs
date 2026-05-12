// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: Default Literal Simplification                ║
// ║  Introduced: C# 7.1                                     ║
// ║  Theme: 1 — Expressiveness & Boilerplate Reduction      ║
// ╚══════════════════════════════════════════════════════════╝

namespace DevScripts.Theme1_Expressiveness.DefaultLiterals;

/// <summary>
/// Demonstrates the simplified <c>default</c> literal that infers
/// type from context, replacing verbose <c>default(T)</c>.
/// </summary>
public static class GenericDefaultsDemo
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — How we did it before (C# 7.0)
    // ──────────────────────────────────────────────────────────

    public static void BeforeDefaultLiteral()
    {
        Console.WriteLine("  BEFORE (C# 7.0 — Verbose default(T)):");
        Console.WriteLine();

        // Must specify the type explicitly
        int count = default(int);
        string? name = default(string);
        CancellationToken token = default(CancellationToken);
        Dictionary<string, List<int>>? map = default(Dictionary<string, List<int>>);

        Console.WriteLine($"    default(int) = {count}");
        Console.WriteLine($"    default(string) = {name ?? "null"}");
        Console.WriteLine($"    default(CancellationToken) = {token}");
        Console.WriteLine($"    default(Dictionary<string, List<int>>) = {map?.ToString() ?? "null"}");
        Console.WriteLine("    ⚠ Long type names repeated in default(LongTypeName)");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — The modern way (C# 7.1)
    // ──────────────────────────────────────────────────────────

    public static void WithDefaultLiteral()
    {
        Console.WriteLine("  AFTER (C# 7.1 — Simplified default literal):");
        Console.WriteLine();

        // Type inferred from context — just 'default'
        int count = default;
        string? name = default;
        CancellationToken token = default;

        Console.WriteLine($"    int count = default → {count}");
        Console.WriteLine($"    string? name = default → {name ?? "null"}");
        Console.WriteLine($"    CancellationToken = default → {token}");
        Console.WriteLine();

        // Especially useful in method parameters
        Console.WriteLine("    In method calls:");
        ProcessData("test", default, default);

        // In switch expressions and conditional returns
        Console.WriteLine("    In conditional expressions:");
        var result = GetMetricsOrDefault("missing.cs");
        Console.WriteLine($"    GetMetricsOrDefault: {result?.FileName ?? "default (null)"}");

        // In generic methods — type inferred from T
        var defaultInt = GetDefault<int>();
        var defaultString = GetDefault<string>();
        Console.WriteLine($"\n    GetDefault<int>(): {defaultInt}");
        Console.WriteLine($"    GetDefault<string>(): {defaultString ?? "null"}");

        Console.WriteLine();
        Console.WriteLine("    ✓ 'default' instead of 'default(LongTypeName)'");
        Console.WriteLine("    ✓ Type inferred from declaration, parameter, or return type");
        Console.WriteLine("    ✓ Works in all contexts: assignments, parameters, returns");
    }

    private static void ProcessData(string input, CancellationToken token, int retries)
    {
        Console.WriteLine($"      ProcessData(\"{input}\", token={token.IsCancellationRequested}, retries={retries})");
    }

    private static CodeMetrics? GetMetricsOrDefault(string fileName)
    {
        // Simulating a lookup that returns null for missing files
        return fileName == "missing.cs" ? default : new CodeMetrics(fileName, 100, 80, 5, 3);
    }

    private static T? GetDefault<T>() => default;

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // The default literal is a small convenience that removes
    // redundancy. It's most impactful with long generic type names:
    // default(Dictionary<string, List<CodeMetrics>>) → default.
    // Combined with CancellationToken parameters (where you often
    // pass default), it cleans up async API calls significantly.

    // GOING DEEPER:
    // default for reference types is null. For value types, it's
    // the "all zeros" value: 0 for int, false for bool, etc.
    // For structs, it creates an instance with all fields zeroed.
    // This is the same behavior as default(T) — the literal form
    // is purely syntactic sugar.

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: Find all uses of default(CancellationToken) in a project
    // and replace them with default. Does it improve readability?

    /// <summary>Runs the complete default literal demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: Default Literal (C# 7.1)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforeDefaultLiteral();
        WithDefaultLiteral();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
