// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: var, Object & Collection Initializers         ║
// ║  Introduced: C# 3.0                                     ║
// ║  Theme: 0 — The Foundation                              ║
// ╚══════════════════════════════════════════════════════════╝

namespace QuantLite.Theme0_Foundation.VarAndInitializers;

/// <summary>
/// Demonstrates <c>var</c> (implicit typing), object initializers,
/// and collection initializers for concise trade creation.
/// </summary>
public static class TradeCreationDemo
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — How we did it before (C# 2.0)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Before these features, creating and populating objects
    /// required verbose constructor calls and property assignments.
    /// Type names were repeated on both sides of every declaration.
    /// </summary>
    public static void BeforeVarAndInitializers()
    {
        Console.WriteLine("  BEFORE (C# 2.0 — Explicit types, verbose construction):");
        Console.WriteLine();

        // Type repeated on both sides
        Dictionary<string, List<TradeRecord>> tradesByTicker =
            new Dictionary<string, List<TradeRecord>>();

        // Multi-step object creation
        List<TradeRecord> trades = new List<TradeRecord>();
        TradeRecord trade = TradeRecord.Create(
            "AAPL", new Money(150.00m, Currency.USD), 100, TradeType.Spot);
        trades.Add(trade);

        Console.WriteLine($"    Created {trades.Count} trade with verbose syntax");
        Console.WriteLine("    ⚠ Type 'Dictionary<string, List<TradeRecord>>' written twice");
        Console.WriteLine("    ⚠ Multi-step creation: declare, create, add");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — The modern way (C# 3.0+)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// <c>var</c> eliminates redundant type declarations.
    /// Object and collection initializers allow inline population.
    /// </summary>
    public static void WithVarAndInitializers()
    {
        Console.WriteLine("  AFTER (C# 3.0 — var + initializers):");
        Console.WriteLine();

        // var — type inferred from the right side
        var tradesByTicker = new Dictionary<string, List<TradeRecord>>();

        // Collection initializer — inline population
        var trades = new List<TradeRecord>
        {
            TradeRecord.Create("AAPL", new Money(150.00m, Currency.USD), 100, TradeType.Spot),
            TradeRecord.Create("MSFT", new Money(420.00m, Currency.USD), 50, TradeType.Forward),
        };

        Console.WriteLine($"    Created {trades.Count} trades with collection initializer");

        // Dictionary initializer (C# 6 index syntax)
        var currencyNames = new Dictionary<string, string>
        {
            ["USD"] = "US Dollar",
            ["EUR"] = "Euro",
            ["GBP"] = "British Pound",
        };

        Console.WriteLine("    Dictionary index initializer:");
        foreach (var (code, name) in currencyNames)
            Console.WriteLine($"      {code}: {name}");

        // Collection expression (C# 12) — even more concise
        // See also: Theme 1 — Collection Expressions for the full story
        List<string> tickers = ["AAPL", "MSFT", "GOOGL"];
        Console.WriteLine($"\n    Collection expression (C# 12): [{string.Join(", ", tickers)}]");

        Console.WriteLine();
        Console.WriteLine("    ✓ var reduces noise — type appears once");
        Console.WriteLine("    ✓ Object initializers — set properties inline");
        Console.WriteLine("    ✓ Collection initializers — populate inline");
        Console.WriteLine("    ✓ C# 12 collection expressions — unified [x, y, z] syntax");
    }

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // These features reduce visual noise. When the type is obvious
    // from context, repeating it adds nothing. var, initializers,
    // and (later) target-typed new all serve the same goal: let
    // the code say what matters, not what the compiler already knows.

    // GOING DEEPER:
    // var is resolved at compile time — it's NOT dynamic. The type
    // is fully known; you just don't write it. This is different from
    // 'dynamic' (Theme 0 — Dynamic Binding) which defers type
    // resolution to runtime. var is safe and has zero runtime cost.

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: Create a Portfolio using object initializer syntax
    // and populate it with 5 trades using a collection initializer.
    // Can you use var everywhere without losing readability?

    /// <summary>Runs the complete var and initializers demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: var, Object & Collection Initializers (C# 3.0)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforeVarAndInitializers();
        WithVarAndInitializers();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
