// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: LINQ (Language Integrated Query)              ║
// ║  Introduced: C# 3.0                                     ║
// ║  Theme: 0 — The Foundation                              ║
// ╚══════════════════════════════════════════════════════════╝

namespace QuantLite.Theme0_Foundation.Linq;

/// <summary>
/// Demonstrates LINQ over trade collections — query syntax vs.
/// method syntax, deferred execution, and aggregation.
/// </summary>
public static class TradeQueriesDemo
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — How we did it before (C# 2.0)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Before LINQ, querying data meant verbose loops, manual
    /// filtering, sorting, and grouping — lots of boilerplate
    /// that obscured the actual query intent.
    /// </summary>
    public static void BeforeLinq()
    {
        Console.WriteLine("  BEFORE (C# 2.0 — Manual loops for querying):");
        Console.WriteLine();

        var trades = CreateSampleTrades();

        // Group trades by type manually
        var groupedByType = new Dictionary<TradeType, List<TradeRecord>>();
        foreach (var trade in trades)
        {
            if (!groupedByType.ContainsKey(trade.Type))
                groupedByType[trade.Type] = [];

            groupedByType[trade.Type].Add(trade);
        }

        // Sort each group by notional value manually
        foreach (var kvp in groupedByType)
        {
            kvp.Value.Sort((a, b) =>
                b.NotionalValue.Amount.CompareTo(a.NotionalValue.Amount));
        }

        Console.WriteLine("    Manual grouping + sorting:");
        foreach (var kvp in groupedByType)
        {
            Console.WriteLine($"      {kvp.Key}: {kvp.Value.Count} trades");
        }
        Console.WriteLine("    ⚠ 15+ lines of boilerplate for a simple group-and-sort");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — The modern way (C# 3.0+)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// LINQ transforms data querying into a declarative, composable
    /// pipeline — the same operation in 2-3 lines.
    /// </summary>
    public static void WithLinq()
    {
        Console.WriteLine("  AFTER (C# 3.0 — LINQ):");
        Console.WriteLine();

        var trades = CreateSampleTrades();

        // Method syntax — the most common style in modern C#
        Console.WriteLine("    Method syntax (fluent):");
        var topTrades = trades
            .Where(t => t.NotionalValue.Amount > 5_000m)
            .OrderByDescending(t => t.NotionalValue.Amount)
            .Take(3);

        foreach (var trade in topTrades)
            Console.WriteLine($"      {trade.Ticker}: {trade.NotionalValue}");

        Console.WriteLine();

        // Query syntax — closer to SQL, sometimes more readable
        Console.WriteLine("    Query syntax (SQL-like):");
        var grouped =
            from trade in trades
            group trade by trade.Type into g
            orderby g.Count() descending
            select new { Type = g.Key, Count = g.Count(), Trades = g.ToList() };

        foreach (var group in grouped)
            Console.WriteLine($"      {group.Type}: {group.Count} trades");

        Console.WriteLine();

        // Aggregation
        Console.WriteLine("    Aggregation:");
        var totalNotional = trades.Sum(t => t.NotionalValue.Amount);
        var avgNotional = trades.Average(t => t.NotionalValue.Amount);
        var maxTrade = trades.MaxBy(t => t.NotionalValue.Amount);
        Console.WriteLine($"      Total notional: {totalNotional:N2}");
        Console.WriteLine($"      Average notional: {avgNotional:N2}");
        Console.WriteLine($"      Largest trade: {maxTrade?.Ticker} ({maxTrade?.NotionalValue})");

        Console.WriteLine();
        Console.WriteLine("    ✓ Declarative — express 'what' not 'how'");
        Console.WriteLine("    ✓ Composable — chain operators into pipelines");
        Console.WriteLine("    ✓ Deferred execution — evaluates lazily");
    }

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // LINQ unified querying across data sources: in-memory collections,
    // databases (via EF Core), XML, JSON, and more. The same syntax
    // works everywhere. It's the feature that made C# feel like a
    // modern language and is used in virtually every C# codebase.

    // GOING DEEPER:
    // LINQ is built on three C# 3.0 features working together:
    // 1. Extension methods — Where(), Select() extend IEnumerable<T>
    // 2. Lambda expressions — the predicate/selector arguments
    // 3. Expression trees — enable LINQ providers to translate to SQL
    // Understanding this trinity explains why LINQ is so powerful
    // and why the same query can work against SQL Server or in-memory.
    // See also: Theme 0 — Extension Methods, Lambda Expressions,
    // Expression Trees (all demonstrated in this theme).

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: Write a LINQ query that finds all Spot trades with notional
    // value > 10,000, groups them by ticker, and returns the ticker
    // with the highest total notional value.

    private static List<TradeRecord> CreateSampleTrades() =>
    [
        TradeRecord.Create("AAPL", new Money(150.00m, Currency.USD), 100, TradeType.Spot),
        TradeRecord.Create("MSFT", new Money(420.00m, Currency.USD), 50, TradeType.Forward),
        TradeRecord.Create("GOOGL", new Money(175.00m, Currency.USD), 200, TradeType.Spot),
        TradeRecord.Create("TSLA", new Money(250.00m, Currency.USD), 30, TradeType.Option),
        TradeRecord.Create("AMZN", new Money(185.00m, Currency.USD), 75, TradeType.Swap),
        TradeRecord.Create("AAPL", new Money(152.00m, Currency.USD), 60, TradeType.Spot),
        TradeRecord.Create("NVDA", new Money(890.00m, Currency.USD), 20, TradeType.Forward),
    ];

    /// <summary>Runs the complete LINQ demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: LINQ (C# 3.0)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforeLinq();
        WithLinq();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
