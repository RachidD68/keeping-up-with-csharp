// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: Tuples (ValueTuple)                           ║
// ║  Introduced: C# 7.0                                     ║
// ║  Theme: 2 — Data Modeling                               ║
// ╚══════════════════════════════════════════════════════════╝

namespace QuantLite.Theme2_DataModeling.Tuples;

/// <summary>
/// Demonstrates tuples for lightweight multi-value returns
/// in trade analysis — no need for a class or out parameters.
/// </summary>
public static class TradeAnalysisDemo
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — How we did it before (C# 6.0)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Returning multiple values required either: out parameters
    /// (awkward), a dedicated DTO class (heavy), or
    /// <c>Tuple&lt;T1, T2&gt;</c> (unreadable Item1, Item2).
    /// </summary>
    public static void BeforeTuples()
    {
        Console.WriteLine("  BEFORE (C# 6.0 — out params and Tuple<T1, T2>):");
        Console.WriteLine();

        var trades = CreateSampleTrades();

        // Option 1: out parameters
        GetTradeStats_Old(trades, out var count, out var total, out var average);
        Console.WriteLine($"    out params: count={count}, total={total:N2}, avg={average:N2}");

        // Option 2: System.Tuple (reference type, Item1/Item2 names)
        var stats = Tuple.Create(count, total, average);
        Console.WriteLine($"    Tuple: Item1={stats.Item1}, Item2={stats.Item2:N2}");
        Console.WriteLine("    ⚠ Item1, Item2, Item3 — what do they mean?");
        Console.WriteLine();
    }

    private static void GetTradeStats_Old(
        List<TradeRecord> trades, out int count, out decimal total, out decimal average)
    {
        count = trades.Count;
        total = trades.Sum(t => t.NotionalValue.Amount);
        average = count > 0 ? total / count : 0;
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — The modern way (C# 7.0+)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// C# 7 tuples (ValueTuple) provide named elements, value
    /// semantics, and zero-allocation returns.
    /// </summary>
    public static void WithTuples()
    {
        Console.WriteLine("  AFTER (C# 7.0 — Named tuples / ValueTuple):");
        Console.WriteLine();

        var trades = CreateSampleTrades();

        // Named tuple return — elements have meaningful names
        var (count, total, average) = GetTradeStats(trades);
        Console.WriteLine($"    Deconstructed: count={count}, total={total:N2}, avg={average:N2}");

        // Access by name (without deconstruction)
        var stats = GetTradeStats(trades);
        Console.WriteLine($"    Named access: stats.Count={stats.Count}, stats.Total={stats.Total:N2}");

        // Tuple in LINQ projections
        var topByType = trades
            .GroupBy(t => t.Type)
            .Select(g => (Type: g.Key, Count: g.Count(), TotalNotional: g.Sum(t => t.NotionalValue.Amount)))
            .OrderByDescending(x => x.TotalNotional);

        Console.WriteLine("    Tuple projection in LINQ:");
        foreach (var (type, cnt, notional) in topByType)
            Console.WriteLine($"      {type}: {cnt} trades, {notional:N2} total");

        // Tuple equality (C# 7.3)
        var a = (Ticker: "AAPL", Qty: 100);
        var b = (Ticker: "AAPL", Qty: 100);
        Console.WriteLine($"\n    Tuple equality: {a == b}");

        Console.WriteLine();
        Console.WriteLine("    ✓ Named elements — self-documenting returns");
        Console.WriteLine("    ✓ Deconstruction — var (x, y, z) = Method()");
        Console.WriteLine("    ✓ Value type — no heap allocation");
        Console.WriteLine("    ✓ Value equality (C# 7.3)");
    }

    /// <summary>
    /// Returns trade statistics as a named tuple.
    /// </summary>
    private static (int Count, decimal Total, decimal Average) GetTradeStats(
        List<TradeRecord> trades)
    {
        var count = trades.Count;
        var total = trades.Sum(t => t.NotionalValue.Amount);
        var average = count > 0 ? total / count : 0;
        return (count, total, average);
    }

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // Tuples fill the gap between "too simple for a class" and "too
    // complex for a single return value." In financial code, methods
    // naturally return (Price, Spread) or (PnL, Risk, Fees) — tuples
    // make this ergonomic without creating dozens of one-off DTOs.

    // GOING DEEPER:
    // C# 7 tuples are ValueTuple<T1, T2, ...> — stack-allocated value
    // types. The element names (Count, Total) exist only at compile
    // time and in metadata; at runtime they're still .Item1, .Item2.
    // This means: tuples are NOT suitable for serialization or public
    // API boundaries where names matter at runtime. For those cases,
    // use records (next demo!).

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: Write a method that returns (Money Highest, Money Lowest, Money Spread)
    // for a collection of trades. Use deconstruction at the call site.

    private static List<TradeRecord> CreateSampleTrades() =>
    [
        TradeRecord.Create("AAPL", new Money(150.00m, Currency.USD), 100, TradeType.Spot),
        TradeRecord.Create("MSFT", new Money(420.00m, Currency.USD), 50, TradeType.Forward),
        TradeRecord.Create("GOOGL", new Money(175.00m, Currency.USD), 200, TradeType.Spot),
        TradeRecord.Create("TSLA", new Money(250.00m, Currency.USD), 30, TradeType.Option),
        TradeRecord.Create("AMZN", new Money(185.00m, Currency.USD), 75, TradeType.Swap),
    ];

    /// <summary>Runs the complete tuples demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: Tuples / ValueTuple (C# 7.0)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforeTuples();
        WithTuples();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
