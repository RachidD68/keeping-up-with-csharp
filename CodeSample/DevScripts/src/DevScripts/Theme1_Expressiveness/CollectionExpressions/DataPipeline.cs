// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: Collection Expressions                        ║
// ║  Introduced: C# 12                                      ║
// ║  Theme: 1 — Expressiveness & Boilerplate Reduction      ║
// ╚══════════════════════════════════════════════════════════╝

using System.Collections.Immutable;

namespace DevScripts.Theme1_Expressiveness.CollectionExpressions;

/// <summary>
/// Demonstrates collection expressions — the unified <c>[x, y, z]</c>
/// syntax for creating arrays, lists, spans, and immutable collections.
/// </summary>
public static class DataPipelineDemo
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — How we did it before (C# 11)
    // ──────────────────────────────────────────────────────────

    public static void BeforeCollectionExpressions()
    {
        Console.WriteLine("  BEFORE (C# 11 — Different syntax per collection type):");
        Console.WriteLine();

        // Each collection type had different initialization syntax
        int[] array = new int[] { 1, 2, 3 };
        var list = new List<string> { "a", "b", "c" };
        var set = new HashSet<int> { 1, 2, 3 };
        Span<int> span = stackalloc int[] { 1, 2, 3 };
        var immutable = System.Collections.Immutable.ImmutableArray.Create(1, 2, 3);

        Console.WriteLine("    new int[] { 1, 2, 3 }          — array");
        Console.WriteLine("    new List<string> { \"a\", \"b\" }  — list");
        Console.WriteLine("    new HashSet<int> { 1, 2, 3 }   — set");
        Console.WriteLine("    stackalloc int[] { 1, 2, 3 }   — span");
        Console.WriteLine("    ImmutableArray.Create(1, 2, 3)  — immutable");
        Console.WriteLine("    ⚠ Five different syntaxes for 'a collection of things'");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — The modern way (C# 12)
    // ──────────────────────────────────────────────────────────

    public static void WithCollectionExpressions()
    {
        Console.WriteLine("  AFTER (C# 12 — Collection expressions [x, y, z]):");
        Console.WriteLine();

        // One syntax for all collection types
        int[] array = [1, 2, 3];
        List<string> list = ["alpha", "beta", "gamma"];
        HashSet<int> set = [1, 2, 3, 4, 5];
        ImmutableArray<int> immutable = [10, 20, 30];

        Console.WriteLine($"    int[] = [{string.Join(", ", array)}]");
        Console.WriteLine($"    List<string> = [{string.Join(", ", list)}]");
        Console.WriteLine($"    HashSet<int> = [{string.Join(", ", set)}]");
        Console.WriteLine($"    ImmutableArray<int> = [{string.Join(", ", immutable)}]");

        Console.WriteLine();

        // Spread operator (..) — merge collections
        int[] first = [1, 2, 3];
        int[] second = [4, 5, 6];
        int[] combined = [..first, ..second, 7, 8, 9];
        Console.WriteLine($"    Spread: [..first, ..second, 7, 8, 9] = [{string.Join(", ", combined)}]");

        // Empty collection
        List<string> empty = [];
        Console.WriteLine($"    Empty: [] (count={empty.Count})");

        // In method arguments — no need for Array.Empty<T>()
        ProcessFiles(["Program.cs", "Models.cs", "Utils.cs"]);

        Console.WriteLine();

        // Building data pipelines
        Console.WriteLine("    Data pipeline with collection expressions:");
        var stages = BuildPipeline(
            ["Read", "Parse"],
            ["Validate", "Transform"],
            ["Write", "Log"]);

        foreach (var stage in stages)
            Console.WriteLine($"      → {stage}");

        Console.WriteLine();
        Console.WriteLine("    ✓ [x, y, z] — one syntax for all collection types");
        Console.WriteLine("    ✓ Spread (..) — merge collections inline");
        Console.WriteLine("    ✓ [] — clean empty collection (replaces Array.Empty<T>())");
        Console.WriteLine("    ✓ Compiler chooses optimal implementation per target type");
    }

    private static void ProcessFiles(IReadOnlyList<string> files)
    {
        Console.WriteLine($"    ProcessFiles({files.Count} files): [{string.Join(", ", files)}]");
    }

    private static List<string> BuildPipeline(
        string[] inputStages, string[] transformStages, string[] outputStages) =>
        [..inputStages, ..transformStages, ..outputStages];

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // Collection expressions unify the #1 most common operation
    // across collection types. The compiler picks the optimal
    // backing implementation: for int[] it uses an array, for
    // List<T> it uses a list, for Span<T> it uses stackalloc.
    // You write [1, 2, 3] and the compiler does the right thing.

    // GOING DEEPER:
    // The spread operator (..) is not just syntactic sugar — the
    // compiler can optimize it. For [..a, ..b] targeting an array,
    // it pre-computes the total length and copies both arrays in
    // one pass. For Span<T>, it can use stackalloc for the combined
    // result. The compiler is smarter than hand-written code here.

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: Create a method that takes params ReadOnlySpan<string>
    // and call it with a collection expression. Compare with
    // the old params string[] approach.

    /// <summary>Runs the complete collection expressions demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: Collection Expressions (C# 12)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforeCollectionExpressions();
        WithCollectionExpressions();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
