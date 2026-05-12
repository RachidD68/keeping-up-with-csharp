// ╔══════════════════════════════════════════════════════════╗
// ║  QuantLite — A Simplified Financial Calculator           ║
// ║  Keeping Up with C#: Themes 0, 2, 3, and 10 (Capstone) ║
// ╚══════════════════════════════════════════════════════════╝

using QuantLite.Theme0_Foundation.Generics;
using QuantLite.Theme0_Foundation.NullableValueTypes;
using QuantLite.Theme0_Foundation.Iterators;
using QuantLite.Theme0_Foundation.Linq;
using QuantLite.Theme0_Foundation.ExtensionMethods;
using QuantLite.Theme0_Foundation.LambdaExpressions;
using QuantLite.Theme0_Foundation.ExpressionTrees;
using QuantLite.Theme0_Foundation.AnonymousTypes;
using QuantLite.Theme0_Foundation.VarAndInitializers;
using QuantLite.Theme0_Foundation.AutoProperties;
using QuantLite.Theme0_Foundation.DynamicBinding;
using QuantLite.Theme0_Foundation.NamedOptionalParams;
using QuantLite.Theme0_Foundation.Variance;
using QuantLite.Theme2_DataModeling.Tuples;
using QuantLite.Theme2_DataModeling.Discards;
using QuantLite.Theme2_DataModeling.Records;
using QuantLite.Theme2_DataModeling.InitOnly;
using QuantLite.Theme2_DataModeling.RequiredMembers;
using QuantLite.Theme2_DataModeling.WithExpressions;
using QuantLite.Theme2_DataModeling.ReadonlyMembers;
using QuantLite.Theme3_PatternMatching.BasicPatterns;
using QuantLite.Theme3_PatternMatching.PropertyPatterns;
using QuantLite.Theme3_PatternMatching.SwitchExpressions;
using QuantLite.Theme3_PatternMatching.RelationalLogical;
using QuantLite.Theme3_PatternMatching.ListPatterns;

Console.OutputEncoding = System.Text.Encoding.UTF8;

PrintBanner();

var demos = BuildDemoList();

if (args.Length > 0 && args[0] == "--all")
{
    RunAll(demos);
    return;
}

RunInteractiveMenu(demos);

// ──────────────────────────────────────────────────────────
// Menu and runner infrastructure
// ──────────────────────────────────────────────────────────

static void PrintBanner()
{
    Console.WriteLine();
    Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
    Console.WriteLine("║                                                          ║");
    Console.WriteLine("║   QuantLite — Simplified Financial Calculator            ║");
    Console.WriteLine("║   Keeping Up with C#: From C# 5 to C# 14               ║");
    Console.WriteLine("║                                                          ║");
    Console.WriteLine("║   Themes: 0 (Foundation), 2 (Data Modeling),            ║");
    Console.WriteLine("║           3 (Pattern Matching), 10 (Capstone)           ║");
    Console.WriteLine("║                                                          ║");
    Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
    Console.WriteLine();
}

static List<(string Category, string Name, Action Run)> BuildDemoList() =>
[
    // Theme 0 — Foundation
    ("Theme 0: Foundation", "Generics", GenericRepositoryDemo.Run),
    ("Theme 0: Foundation", "Nullable Value Types", OptionalPriceDemo.Run),
    ("Theme 0: Foundation", "Iterators (yield return)", TradeIteratorDemo.Run),
    ("Theme 0: Foundation", "LINQ", TradeQueriesDemo.Run),
    ("Theme 0: Foundation", "Extension Methods", TradeExtensionsDemo.Run),
    ("Theme 0: Foundation", "Lambda Expressions", TradePredicateDemo.Run),
    ("Theme 0: Foundation", "Expression Trees", DynamicFilterDemo.Run),
    ("Theme 0: Foundation", "Anonymous Types", TradeProjectionDemo.Run),
    ("Theme 0: Foundation", "var & Initializers", TradeCreationDemo.Run),
    ("Theme 0: Foundation", "Auto-Properties", SimpleModelsDemo.Run),
    ("Theme 0: Foundation", "Dynamic Binding", DynamicDispatchDemo.Run),
    ("Theme 0: Foundation", "Named & Optional Params", OrderBuilderDemo.Run),
    ("Theme 0: Foundation", "Generic Variance", CovarianceDemo.Run),

    // Theme 2 — Data Modeling
    ("Theme 2: Data Modeling", "Tuples", TradeAnalysisDemo.Run),
    ("Theme 2: Data Modeling", "Discards", SelectiveDeconDemo.Run),
    ("Theme 2: Data Modeling", "Records & Record Structs", TradeRecordDemoClass.Run),
    ("Theme 2: Data Modeling", "Init-Only Properties", ImmutableConfigDemo.Run),
    ("Theme 2: Data Modeling", "Required Members", ValidatedOrderDemo.Run),
    ("Theme 2: Data Modeling", "With-Expressions", TradeAmendmentDemo.Run),
    ("Theme 2: Data Modeling", "Readonly Members", MoneyStructDemo.Run),

    // Theme 3 — Pattern Matching
    ("Theme 3: Pattern Matching", "Basic Type Patterns", TypeCheckingDemo.Run),
    ("Theme 3: Pattern Matching", "Property Patterns", TradeValidatorDemo.Run),
    ("Theme 3: Pattern Matching", "Switch Expressions", RiskClassifierDemo.Run),
    ("Theme 3: Pattern Matching", "Relational & Logical Patterns", FeeCalculatorDemo.Run),
    ("Theme 3: Pattern Matching", "List Patterns", SequenceAnalysisDemo.Run),

    // Theme 10 — Capstone
    ("Theme 10: Capstone", "Immutable Data Pattern", ImmutableDataPatternDemo.Run),
];

static void RunInteractiveMenu(List<(string Category, string Name, Action Run)> demos)
{
    while (true)
    {
        Console.WriteLine("  Select a demo to run:");
        Console.WriteLine("  ─────────────────────────────────────────────");
        Console.WriteLine();

        var currentCategory = "";
        for (var i = 0; i < demos.Count; i++)
        {
            if (demos[i].Category != currentCategory)
            {
                currentCategory = demos[i].Category;
                Console.WriteLine($"  {currentCategory}");
            }
            Console.WriteLine($"    [{i + 1,2}] {demos[i].Name}");
        }

        Console.WriteLine();
        Console.WriteLine($"    [ A] Run all demos");
        Console.WriteLine($"    [ Q] Quit");
        Console.WriteLine();
        Console.Write("  Enter choice: ");

        var input = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(input) || input.Equals("Q", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("\n  Thank you for exploring QuantLite!");
            return;
        }

        if (input.Equals("A", StringComparison.OrdinalIgnoreCase))
        {
            RunAll(demos);
            continue;
        }

        if (int.TryParse(input, out var choice) && choice >= 1 && choice <= demos.Count)
        {
            Console.WriteLine();
            try
            {
                demos[choice - 1].Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Error: {ex.Message}");
            }
            Console.WriteLine();
            Console.WriteLine("  Press Enter to continue...");
            Console.ReadLine();
        }
        else
        {
            Console.WriteLine("  Invalid choice. Please try again.");
        }
    }
}

static void RunAll(List<(string Category, string Name, Action Run)> demos)
{
    var currentCategory = "";
    foreach (var (category, name, run) in demos)
    {
        if (category != currentCategory)
        {
            currentCategory = category;
            Console.WriteLine();
            Console.WriteLine($"═══════════════════════════════════════════════════");
            Console.WriteLine($"  {category}");
            Console.WriteLine($"═══════════════════════════════════════════════════");
        }

        Console.WriteLine();
        try
        {
            run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Error in {name}: {ex.Message}");
        }
    }

    Console.WriteLine();
    Console.WriteLine("═══════════════════════════════════════════════════");
    Console.WriteLine("  All demos complete!");
    Console.WriteLine("═══════════════════════════════════════════════════");
}
