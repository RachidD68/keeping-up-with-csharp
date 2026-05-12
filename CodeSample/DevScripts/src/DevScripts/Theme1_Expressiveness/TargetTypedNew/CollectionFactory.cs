// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: Target-Typed new Expressions                  ║
// ║  Introduced: C# 9.0                                     ║
// ║  Theme: 1 — Expressiveness & Boilerplate Reduction      ║
// ╚══════════════════════════════════════════════════════════╝

namespace DevScripts.Theme1_Expressiveness.TargetTypedNew;

/// <summary>
/// Demonstrates target-typed <c>new</c> expressions — when the type
/// is already known from context, you can omit it from the <c>new</c>.
/// </summary>
public static class CollectionFactoryDemo
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — How we did it before (C# 8.0)
    // ──────────────────────────────────────────────────────────

    public static void BeforeTargetTypedNew()
    {
        Console.WriteLine("  BEFORE (C# 8.0 — Type repeated on both sides):");
        Console.WriteLine();

        // Type appears twice: left side AND right side
        Dictionary<string, List<CodeMetrics>> metricsByFile =
            new Dictionary<string, List<CodeMetrics>>();

        List<LogEntry> logs = new List<LogEntry>();

        Console.WriteLine($"    Dictionary<string, List<CodeMetrics>> metricsByFile = ");
        Console.WriteLine($"        new Dictionary<string, List<CodeMetrics>>();");
        Console.WriteLine("    ⚠ Long generic type names repeated verbatim");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — The modern way (C# 9.0)
    // ──────────────────────────────────────────────────────────

    public static void WithTargetTypedNew()
    {
        Console.WriteLine("  AFTER (C# 9.0 — Target-typed new):");
        Console.WriteLine();

        // Type inferred from the left side — DRY
        Dictionary<string, List<CodeMetrics>> metricsByFile = new();
        List<LogEntry> logs = new();

        Console.WriteLine("    Dictionary<string, List<CodeMetrics>> metricsByFile = new();");
        Console.WriteLine("    ✓ Type written once, inferred on the right side");
        Console.WriteLine();

        // Especially useful in field declarations
        var factory = new MetricsFactory();
        Console.WriteLine($"    Factory created: {factory}");

        // Target-typed new in method arguments
        ProcessMetrics(new("Demo.cs", 50, 40, 3, 2));
        Console.WriteLine();

        // Contrast with var (type on the right):
        // var metricsByFile = new Dictionary<string, List<CodeMetrics>>();  ← var
        // Dictionary<string, List<CodeMetrics>> metricsByFile = new();     ← new()
        Console.WriteLine("    var vs. new() — opposite sides of the same coin:");
        Console.WriteLine("      var x = new SomeType();   ← type on right, inferred left");
        Console.WriteLine("      SomeType x = new();       ← type on left, inferred right");
        Console.WriteLine("    Both write the type exactly once.");

        Console.WriteLine();
        Console.WriteLine("    ✓ Reduces noise in field/property declarations");
        Console.WriteLine("    ✓ Cleaner method arguments: Method(new(...))");
        Console.WriteLine("    ✓ Complementary to var — choose based on readability");
    }

    private static void ProcessMetrics(CodeMetrics metrics) =>
        Console.WriteLine($"    Processed: {metrics.FileName} ({metrics.Lines} lines)");

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // Target-typed new is the counterpart to var. Together they
    // ensure the type is written exactly once, regardless of
    // which side of the assignment it appears on. Use var when
    // the constructor makes the type obvious (var x = new Foo()).
    // Use new() when the declaration makes it obvious (Foo x = new()).

    // GOING DEEPER:
    // Target-typed new works anywhere the compiler knows the
    // expected type: field initializers, property initializers,
    // return statements, method arguments, array elements,
    // conditional expressions (condition ? new() : existing).
    // It does NOT work with var (var x = new() — what type?).

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: Convert a class with multiple fields using explicit
    // constructors to use target-typed new(). Which is more readable?

    private sealed class MetricsFactory
    {
        // target-typed new in field declarations — concise
        private readonly List<CodeMetrics> _metrics = new();
        private readonly Dictionary<string, int> _fileIndex = new();
        private readonly StringBuilder _report = new();

        public void Add(CodeMetrics metrics)
        {
            _metrics.Add(metrics);
            _fileIndex[metrics.FileName] = _metrics.Count - 1;
        }

        public override string ToString() =>
            $"MetricsFactory: {_metrics.Count} files indexed";
    }

    /// <summary>Runs the complete target-typed new demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: Target-Typed new (C# 9.0)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforeTargetTypedNew();
        WithTargetTypedNew();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
