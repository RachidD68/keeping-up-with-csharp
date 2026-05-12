// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: Expression-Bodied Members                     ║
// ║  Introduced: C# 6.0 (methods/props) → C# 7.0 (all)    ║
// ║  Theme: 1 — Expressiveness & Boilerplate Reduction      ║
// ╚══════════════════════════════════════════════════════════╝

namespace DevScripts.Theme1_Expressiveness.ExpressionBodied;

/// <summary>
/// Demonstrates expression-bodied members for concise code metrics
/// calculations — from methods to constructors to finalizers.
/// </summary>
public static class MetricsCalculatorDemo
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — How we did it before (C# 5.0)
    // ──────────────────────────────────────────────────────────

    public static void BeforeExpressionBodied()
    {
        Console.WriteLine("  BEFORE (C# 5.0 — Full method bodies for one-liners):");
        Console.WriteLine();

        var calc = new VerboseCalculator("Program.cs", 150, 120, 8, 5);
        Console.WriteLine($"    Lines: {calc.Lines}");
        Console.WriteLine($"    Density: {calc.GetCodeDensity()}");
        Console.WriteLine($"    Summary: {calc.ToString()}");
        Console.WriteLine("    ⚠ Each member needs braces + return — 3 lines per one-liner");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — The modern way (C# 6.0/7.0)
    // ──────────────────────────────────────────────────────────

    public static void WithExpressionBodied()
    {
        Console.WriteLine("  AFTER (C# 6/7 — Expression-bodied members):");
        Console.WriteLine();

        var calc = new ConciseCalculator("Program.cs", 150, 120, 8, 5);
        Console.WriteLine($"    Lines: {calc.Lines}");
        Console.WriteLine($"    Density: {calc.CodeDensity:P1}");
        Console.WriteLine($"    Rating: {calc.Rating}");
        Console.WriteLine($"    Summary: {calc}");

        // Destructor/finalizer (C# 7) — expression-bodied too
        Console.WriteLine();
        Console.WriteLine("    Expression-bodied evolution:");
        Console.WriteLine("      C# 6: Methods, properties, indexers, operators");
        Console.WriteLine("      C# 7: Constructors, finalizers, get/set accessors");

        Console.WriteLine();
        Console.WriteLine("    ✓ One-liner members use => instead of { return ...; }");
        Console.WriteLine("    ✓ Reduces 3 lines to 1 — less noise, more signal");
        Console.WriteLine("    ✓ Works on all member types since C# 7");
    }

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // Most properties and simple methods are one-liners. Expression
    // bodies eliminate two-thirds of the visual noise for these
    // common cases. This isn't just about saving lines — it changes
    // how you scan code. A property with => is instantly recognized
    // as a computed value, while a property with { get { return; } }
    // makes you wonder if there's hidden complexity.

    // GOING DEEPER:
    // Expression-bodied members are compiled identically to their
    // block-bodied equivalents. There's zero runtime difference.
    // The compiler guideline: use expression bodies "when on single
    // line" — if the expression wraps to multiple lines, consider
    // a block body for readability.

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: Convert the VerboseCalculator class to use expression-bodied
    // members everywhere possible. Compare the line count.

    private sealed class VerboseCalculator
    {
        private readonly string _fileName;
        private readonly int _lines;
        private readonly int _codeLines;
        private readonly int _complexity;
        private readonly int _dependencies;

        public VerboseCalculator(string fileName, int lines, int codeLines, int complexity, int deps)
        {
            _fileName = fileName;
            _lines = lines;
            _codeLines = codeLines;
            _complexity = complexity;
            _dependencies = deps;
        }

        public int Lines
        {
            get { return _lines; }
        }

        public double GetCodeDensity()
        {
            return _lines > 0 ? (double)_codeLines / _lines : 0;
        }

        public override string ToString()
        {
            return $"{_fileName}: {_lines} lines, complexity={_complexity}";
        }
    }

    /// <summary>Same calculator with expression-bodied members — much shorter.</summary>
    private sealed class ConciseCalculator(string fileName, int lines, int codeLines, int complexity, int deps)
    {
        // Expression-bodied properties (C# 6)
        public int Lines => lines;
        public int CodeLines => codeLines;
        public double CodeDensity => lines > 0 ? (double)codeLines / lines : 0;

        // Expression-bodied property (C# 6)
        public string Rating => complexity switch
        {
            <= 5 => "Simple",
            <= 10 => "Moderate",
            _ => "Complex"
        };

        // Expression-bodied ToString (C# 6)
        public override string ToString() =>
            $"{fileName}: {lines} lines, {codeLines} code, complexity={complexity}, deps={deps}";
    }

    /// <summary>Runs the complete expression-bodied members demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: Expression-Bodied Members (C# 6/7)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforeExpressionBodied();
        WithExpressionBodied();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
