// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: Extension Methods                             ║
// ║  Introduced: C# 3.0                                     ║
// ║  Theme: 0 — The Foundation                              ║
// ╚══════════════════════════════════════════════════════════╝

namespace QuantLite.Theme0_Foundation.ExtensionMethods;

/// <summary>
/// Demonstrates extension methods by adding fluent query capabilities
/// to trade collections without modifying the original types.
/// </summary>
public static class TradeExtensionsDemo
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — How we did it before (C# 2.0)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Before extension methods, adding utility operations meant
    /// either modifying the class (if you owned it) or creating
    /// static helper classes with awkward calling syntax.
    /// </summary>
    public static void BeforeExtensionMethods()
    {
        Console.WriteLine("  BEFORE (C# 2.0 — Static helper classes):");
        Console.WriteLine();

        var trades = CreateSampleTrades();

        // Static helper method — inverted, less readable call order
        var summary = TradeHelper.GetSummary(trades);
        var highValue = TradeHelper.FilterHighValue(trades, 10_000m);

        Console.WriteLine($"    TradeHelper.GetSummary(trades): {summary}");
        Console.WriteLine($"    TradeHelper.FilterHighValue(trades, 10000): {highValue.Count()} trades");
        Console.WriteLine("    ⚠ Utility class with static methods — not discoverable, not fluent");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — The modern way (C# 3.0+)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Extension methods let you "add" methods to existing types.
    /// They appear in IntelliSense and support fluent chaining.
    /// </summary>
    public static void WithExtensionMethods()
    {
        Console.WriteLine("  AFTER (C# 3.0 — Extension methods):");
        Console.WriteLine();

        var trades = CreateSampleTrades();

        // Extension methods — fluent, discoverable, chainable
        var summary = trades.ToSummary();
        Console.WriteLine($"    trades.ToSummary(): {summary}");

        var topTrades = trades
            .HigherThan(10_000m)
            .SortedByValue()
            .ToSummaryLines();

        Console.WriteLine("    Fluent chain: trades.HigherThan(10000).SortedByValue():");
        foreach (var line in topTrades)
            Console.WriteLine($"      {line}");

        // Extension method on a single trade
        var trade = trades[0];
        Console.WriteLine($"    trade.RiskCategory(): {trade.RiskCategory()}");

        Console.WriteLine();
        Console.WriteLine("    ✓ Fluent API — reads left to right like natural language");
        Console.WriteLine("    ✓ Discoverable via IntelliSense");
        Console.WriteLine("    ✓ Chainable — compose operations into pipelines");
    }

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // Extension methods are the mechanism that makes LINQ possible.
    // Every LINQ operator (Where, Select, GroupBy) is an extension
    // method on IEnumerable<T>. They also enable the "fluent API"
    // pattern used in ASP.NET Core configuration, EF Core queries,
    // and countless libraries.

    // GOING DEEPER:
    // Extension methods are pure syntactic sugar — the compiler
    // transforms trades.HigherThan(10000) into
    // TradeCollectionExtensions.HigherThan(trades, 10000).
    // This means: (1) they can't access private members,
    // (2) instance methods always win over extensions with the
    // same signature, (3) they're resolved at compile time, not
    // runtime (no virtual dispatch).
    // See also: Theme 4 — Extension Members (C# 14 enhances this
    // with extension properties, operators, and static methods).

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: Write an extension method on Money called
    // IsPositive() that returns true if Amount > 0.
    // Then chain it: trades.Where(t => t.NotionalValue.IsPositive())

    // Old-style static helper (for comparison)
    private static class TradeHelper
    {
        public static string GetSummary(IEnumerable<TradeRecord> trades) =>
            $"{trades.Count()} trades";

        public static IEnumerable<TradeRecord> FilterHighValue(
            IEnumerable<TradeRecord> trades, decimal threshold) =>
            trades.Where(t => t.NotionalValue.Amount > threshold);
    }

    private static List<TradeRecord> CreateSampleTrades() =>
    [
        TradeRecord.Create("AAPL", new Money(150.00m, Currency.USD), 100, TradeType.Spot),
        TradeRecord.Create("MSFT", new Money(420.00m, Currency.USD), 50, TradeType.Forward),
        TradeRecord.Create("GOOGL", new Money(175.00m, Currency.USD), 200, TradeType.Spot),
        TradeRecord.Create("TSLA", new Money(250.00m, Currency.USD), 30, TradeType.Option),
        TradeRecord.Create("AMZN", new Money(185.00m, Currency.USD), 75, TradeType.Swap),
    ];

    /// <summary>Runs the complete extension methods demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: Extension Methods (C# 3.0)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforeExtensionMethods();
        WithExtensionMethods();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}

/// <summary>
/// Extension methods on <see cref="IEnumerable{TradeRecord}"/>
/// providing fluent trade collection operations.
/// </summary>
public static class TradeCollectionExtensions
{
    /// <summary>Filters trades with notional value above the threshold.</summary>
    public static IEnumerable<TradeRecord> HigherThan(
        this IEnumerable<TradeRecord> trades, decimal threshold) =>
        trades.Where(t => t.NotionalValue.Amount > threshold);

    /// <summary>Sorts trades by notional value descending.</summary>
    public static IOrderedEnumerable<TradeRecord> SortedByValue(
        this IEnumerable<TradeRecord> trades) =>
        trades.OrderByDescending(t => t.NotionalValue.Amount);

    /// <summary>Produces summary lines for console display.</summary>
    public static IEnumerable<string> ToSummaryLines(
        this IEnumerable<TradeRecord> trades) =>
        trades.Select(t => $"{t.Ticker}: {t.NotionalValue} ({t.Type})");

    /// <summary>Produces a one-line summary of the collection.</summary>
    public static string ToSummary(this IEnumerable<TradeRecord> trades)
    {
        var list = trades.ToList();
        var total = list.Sum(t => t.NotionalValue.Amount);
        return $"{list.Count} trades, total notional: {total:N2} USD";
    }

    /// <summary>Returns a simple risk category for a single trade.</summary>
    public static string RiskCategory(this TradeRecord trade) =>
        trade.NotionalValue.Amount switch
        {
            > 50_000m => "High",
            > 10_000m => "Medium",
            _ => "Low"
        };
}
