// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: Anonymous Types                               ║
// ║  Introduced: C# 3.0                                     ║
// ║  Theme: 0 — The Foundation                              ║
// ╚══════════════════════════════════════════════════════════╝

namespace QuantLite.Theme0_Foundation.AnonymousTypes;

/// <summary>
/// Demonstrates anonymous types for ad-hoc projections in
/// trade queries — lightweight data shapes without defining classes.
/// </summary>
public static class TradeProjectionDemo
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — How we did it before (C# 2.0)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Before anonymous types, every distinct data shape in a query
    /// required a dedicated class — even for throwaway projections
    /// used in a single method.
    /// </summary>
    public static void BeforeAnonymousTypes()
    {
        Console.WriteLine("  BEFORE (C# 2.0 — Dedicated DTO classes for every projection):");
        Console.WriteLine();

        var trades = CreateSampleTrades();

        // Had to create a class just for this one-off projection
        var summaries = new List<TradeSummaryDto>();
        foreach (var trade in trades)
        {
            summaries.Add(new TradeSummaryDto
            {
                Ticker = trade.Ticker,
                TotalValue = trade.NotionalValue.Amount
            });
        }

        Console.WriteLine($"    Created {summaries.Count} TradeSummaryDto objects");
        Console.WriteLine("    ⚠ Required a dedicated class for a one-time projection");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — The modern way (C# 3.0+)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Anonymous types let you create ad-hoc shapes inline.
    /// Combined with LINQ, they enable concise query projections.
    /// </summary>
    public static void WithAnonymousTypes()
    {
        Console.WriteLine("  AFTER (C# 3.0 — Anonymous types):");
        Console.WriteLine();

        var trades = CreateSampleTrades();

        // Anonymous type projection in LINQ
        var summaries = trades
            .Select(t => new
            {
                t.Ticker,
                Value = t.NotionalValue.Amount,
                t.Type,
                IsHighValue = t.NotionalValue.Amount > 10_000m
            })
            .OrderByDescending(s => s.Value);

        Console.WriteLine("    Anonymous type projections:");
        foreach (var s in summaries)
            Console.WriteLine($"      {s.Ticker}: {s.Value:N2} ({s.Type}) " +
                              $"{(s.IsHighValue ? "⭐" : "")}");

        Console.WriteLine();

        // Anonymous types have value-based equality
        var a = new { Ticker = "AAPL", Price = 150m };
        var b = new { Ticker = "AAPL", Price = 150m };
        Console.WriteLine($"    Value equality: a.Equals(b) = {a.Equals(b)}");

        // Useful for grouping
        var byTypeAndHighValue = trades
            .GroupBy(t => new { t.Type, IsHigh = t.NotionalValue.Amount > 10_000m })
            .Select(g => new { g.Key.Type, g.Key.IsHigh, Count = g.Count() });

        Console.WriteLine("    Grouped by composite key (Type + IsHigh):");
        foreach (var g in byTypeAndHighValue)
            Console.WriteLine($"      {g.Type} (High={g.IsHigh}): {g.Count}");

        Console.WriteLine();
        Console.WriteLine("    ✓ No class definition needed for one-off projections");
        Console.WriteLine("    ✓ Value-based equality built in");
        Console.WriteLine("    ✓ Ideal for LINQ Select and GroupBy keys");
    }

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // Anonymous types eliminate the need for dozens of "DTO" classes
    // that exist only to carry data through a single query pipeline.
    // However, they can't leave the method scope (they have no name
    // you can write in a return type). For cross-method shapes,
    // see Theme 2 — Tuples and Records.

    // GOING DEEPER:
    // The compiler generates a real class behind the scenes — with
    // readonly properties, GetHashCode, Equals, and ToString.
    // If two anonymous types in the same assembly have the same
    // property names, types, and order, the compiler reuses the
    // same generated class. This is a key optimization.
    // Limitation: anonymous types are reference types (class),
    // so they allocate on the heap. For value semantics without
    // allocation, see Theme 2 — Tuples (ValueTuple).

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: Write a LINQ query that groups trades by Ticker, then
    // projects into an anonymous type with Ticker, AveragePrice,
    // and TradeCount. Which ticker has the most trades?

    /// <summary>Helper DTO used in the "before" example.</summary>
    private sealed class TradeSummaryDto
    {
        public string Ticker { get; set; } = "";
        public decimal TotalValue { get; set; }
    }

    private static List<TradeRecord> CreateSampleTrades() =>
    [
        TradeRecord.Create("AAPL", new Money(150.00m, Currency.USD), 100, TradeType.Spot),
        TradeRecord.Create("MSFT", new Money(420.00m, Currency.USD), 50, TradeType.Forward),
        TradeRecord.Create("GOOGL", new Money(175.00m, Currency.USD), 200, TradeType.Spot),
        TradeRecord.Create("TSLA", new Money(250.00m, Currency.USD), 30, TradeType.Option),
        TradeRecord.Create("AAPL", new Money(152.00m, Currency.USD), 75, TradeType.Spot),
    ];

    /// <summary>Runs the complete anonymous types demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: Anonymous Types (C# 3.0)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforeAnonymousTypes();
        WithAnonymousTypes();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
