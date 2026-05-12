// Chapter 1 — The Foundation (C# 1–4) — INTERMEDIATE
// ----------------------------------------------------------------
// Exercise: Write a LINQ query against a List<TradeRecord> that
//   groups trades by Ticker, computes the total notional value per
//   ticker, and returns the top three tickers ordered by total
//   notional descending. Use method syntax.
//
// Hint: GroupBy produces IGrouping<TKey, TElement>; follow it with
//   Select projecting new { Ticker = g.Key, Total = g.Sum(...) },
//   then OrderByDescending and Take(3).
// ----------------------------------------------------------------

namespace KeepUpCs.Exercises.Ch01;

// Local extension of TradeRecord with the Notional property.
public static class TradeExtensions
{
    public static decimal Notional(this TradeRecord t) => t.Price * t.Quantity;
}

public static class IntermediateDemo
{
    /// <summary>
    /// Top three tickers by total notional, descending.
    /// </summary>
    public static IReadOnlyList<(string Ticker, decimal Total)> TopThreeByNotional(
        List<TradeRecord> trades)
    {
        return trades
            .GroupBy(t => t.Ticker)
            .Select(g => new { Ticker = g.Key, Total = g.Sum(t => t.Notional()) })
            .OrderByDescending(x => x.Total)
            .Take(3)
            .Select(x => (x.Ticker, x.Total))
            .ToList();
    }

    public static void Run()
    {
        Console.WriteLine("Ch01 Intermediate — Top-3 tickers by total notional");
        Console.WriteLine(new string('─', 60));

        var trades = new List<TradeRecord>
        {
            new(Guid.NewGuid(), "AAPL", 150m, 100),   //  15 000
            new(Guid.NewGuid(), "AAPL", 152m,  50),   //   7 600
            new(Guid.NewGuid(), "MSFT", 420m, 200),   //  84 000
            new(Guid.NewGuid(), "TSLA", 250m,  80),   //  20 000
            new(Guid.NewGuid(), "MSFT", 425m,  10),   //   4 250
            new(Guid.NewGuid(), "NVDA", 900m,  30),   //  27 000
            new(Guid.NewGuid(), "GOOG", 140m,  60),   //   8 400
        };

        var top = TopThreeByNotional(trades);

        Console.WriteLine("  Ticker   Total notional");
        Console.WriteLine("  ──────   ──────────────");
        foreach (var (ticker, total) in top)
            Console.WriteLine($"  {ticker,-7}  {total,14:N2}");

        // Expected: MSFT 88_250.00, NVDA 27_000.00, AAPL 22_600.00
    }
}
