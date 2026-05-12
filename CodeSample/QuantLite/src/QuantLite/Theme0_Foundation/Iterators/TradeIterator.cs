// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: Iterators (yield return)                      ║
// ║  Introduced: C# 2.0                                     ║
// ║  Theme: 0 — The Foundation                              ║
// ╚══════════════════════════════════════════════════════════╝

namespace QuantLite.Theme0_Foundation.Iterators;

/// <summary>
/// Demonstrates iterators using <c>yield return</c> for lazy,
/// on-demand trade filtering and generation.
/// </summary>
public static class TradeIteratorDemo
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — How we did it before (C# 1.0)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Before iterators, producing a filtered sequence required
    /// materializing the entire result into a list upfront —
    /// wasteful when you only need the first few matches.
    /// </summary>
    public static void BeforeIterators()
    {
        Console.WriteLine("  BEFORE (C# 1.0 — Eager materialization):");
        Console.WriteLine();

        var trades = CreateSampleTrades();

        // Must build the entire list, even if we only need the first item
        var highValueTrades = new List<TradeRecord>();
        foreach (var trade in trades)
        {
            if (trade.NotionalValue.Amount > 10_000m)
            {
                highValueTrades.Add(trade);
            }
        }

        Console.WriteLine($"    Built list of {highValueTrades.Count} high-value trades");
        Console.WriteLine($"    ⚠ All items evaluated and stored in memory upfront");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — The modern way (C# 2.0+)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// With <c>yield return</c>, the iterator produces items lazily —
    /// each item is generated only when the consumer asks for it.
    /// </summary>
    public static void WithIterators()
    {
        Console.WriteLine("  AFTER (C# 2.0 — yield return, lazy evaluation):");
        Console.WriteLine();

        var trades = CreateSampleTrades();

        // Lazy — nothing executes until we enumerate
        var highValueTrades = FilterHighValue(trades, 10_000m);
        Console.WriteLine("    Iterator created (nothing executed yet)");

        // Only now does execution begin, and only as far as needed
        var first = highValueTrades.First();
        Console.WriteLine($"    First high-value trade: {first.Ticker} ({first.NotionalValue})");
        Console.WriteLine("    ✓ Only evaluated trades until the first match was found");
        Console.WriteLine();

        // Chaining iterators — composable pipelines
        Console.WriteLine("    Chained iterators (composable pipeline):");
        var pipeline = trades
            .Where(t => t.Type == TradeType.Spot)
            .Select(t => $"      {t.Ticker}: {t.NotionalValue}");

        foreach (var line in pipeline)
            Console.WriteLine(line);

        Console.WriteLine();
        Console.WriteLine("    ✓ Lazy evaluation — items produced on demand");
        Console.WriteLine("    ✓ Composable — chain multiple iterators into pipelines");
        Console.WriteLine("    ✓ Memory efficient — no intermediate collections");
    }

    /// <summary>
    /// An iterator method using <c>yield return</c> to lazily
    /// filter trades above a threshold.
    /// </summary>
    private static IEnumerable<TradeRecord> FilterHighValue(
        IEnumerable<TradeRecord> trades, decimal threshold)
    {
        Console.WriteLine("    [Iterator] Starting evaluation...");
        foreach (var trade in trades)
        {
            Console.WriteLine($"    [Iterator] Evaluating: {trade.Ticker}");
            if (trade.NotionalValue.Amount > threshold)
            {
                yield return trade;
            }
        }
    }

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // Iterators are the foundation of LINQ. Every LINQ operator
    // (Where, Select, OrderBy, etc.) is implemented using yield return.
    // Understanding iterators means understanding LINQ's deferred
    // execution model — the #1 source of confusion for C# developers.

    // GOING DEEPER:
    // The compiler transforms an iterator method into a state machine
    // class that implements IEnumerable<T> and IEnumerator<T>. Each
    // yield return becomes a state transition. This is similar to how
    // async/await works (Theme 8) — both use compiler-generated state
    // machines. The pattern: yield return → iterator state machine,
    // await → async state machine.

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: Write an iterator method GenerateRandomTrades(int count)
    // that uses yield return to generate trades one at a time with
    // random tickers and prices. Then use .Take(5) to grab only 5.

    private static List<TradeRecord> CreateSampleTrades() =>
    [
        TradeRecord.Create("AAPL", new Money(150.00m, Currency.USD), 100, TradeType.Spot),
        TradeRecord.Create("MSFT", new Money(420.00m, Currency.USD), 50, TradeType.Forward),
        TradeRecord.Create("GOOGL", new Money(175.00m, Currency.USD), 200, TradeType.Spot),
        TradeRecord.Create("TSLA", new Money(250.00m, Currency.USD), 30, TradeType.Option),
        TradeRecord.Create("AMZN", new Money(185.00m, Currency.USD), 75, TradeType.Swap),
    ];

    /// <summary>Runs the complete iterators demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: Iterators / yield return (C# 2.0)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforeIterators();
        WithIterators();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
