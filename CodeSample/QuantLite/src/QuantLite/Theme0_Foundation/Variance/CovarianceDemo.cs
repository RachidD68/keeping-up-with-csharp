// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: Generic Variance (Covariance & Contravariance)║
// ║  Introduced: C# 4.0                                     ║
// ║  Theme: 0 — The Foundation                              ║
// ╚══════════════════════════════════════════════════════════╝

namespace QuantLite.Theme0_Foundation.Variance;

/// <summary>
/// Demonstrates covariance (<c>out T</c>) and contravariance
/// (<c>in T</c>) in generic interfaces — enabling natural
/// assignments like <c>IEnumerable&lt;Derived&gt;</c> →
/// <c>IEnumerable&lt;Base&gt;</c>.
/// </summary>
public static class CovarianceDemo
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — How we did it before (C# 3.0)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Before variance, assigning a collection of a derived type
    /// to a variable of the base collection type was a compile error,
    /// even when it was logically safe.
    /// </summary>
    public static void BeforeVariance()
    {
        Console.WriteLine("  BEFORE (C# 3.0 — No generic variance):");
        Console.WriteLine();

        // Concrete types
        var spotTrades = new List<SpotTrade>
        {
            new("AAPL", 150.00m),
            new("MSFT", 420.00m),
        };

        // This would NOT compile in C# 3.0:
        // IList<BaseTrade> trades = spotTrades; // ❌ Error

        // Workaround: manual casting
        var trades = new List<BaseTrade>();
        foreach (var spot in spotTrades)
            trades.Add(spot); // Manual upcast each item

        Console.WriteLine($"    Manually copied {trades.Count} trades to base list");
        Console.WriteLine("    ⚠ IList<SpotTrade> is NOT assignable to IList<BaseTrade>");
        Console.WriteLine("    ⚠ Must copy items manually — wasteful and error-prone");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — The modern way (C# 4.0)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// With variance annotations, <c>IEnumerable&lt;out T&gt;</c>
    /// supports covariance — derived types can be used where base
    /// types are expected, just like regular inheritance.
    /// </summary>
    public static void WithVariance()
    {
        Console.WriteLine("  AFTER (C# 4.0 — Generic variance):");
        Console.WriteLine();

        var spotTrades = new List<SpotTrade>
        {
            new("AAPL", 150.00m),
            new("MSFT", 420.00m),
        };

        // COVARIANCE (out T): IEnumerable<SpotTrade> → IEnumerable<BaseTrade>
        // This works because IEnumerable<out T> is covariant
        IEnumerable<BaseTrade> trades = spotTrades; // ✅ No copy needed!
        Console.WriteLine("    Covariance (out T):");
        Console.WriteLine($"      IEnumerable<SpotTrade> → IEnumerable<BaseTrade>: ✓");
        foreach (var trade in trades)
            Console.WriteLine($"      {trade.Ticker}: {trade.Price}");

        Console.WriteLine();

        // CONTRAVARIANCE (in T): IComparer<BaseTrade> → IComparer<SpotTrade>
        // A comparer that works on any BaseTrade also works on SpotTrade
        IComparer<BaseTrade> baseComparer = new TradeByPriceComparer();
        IComparer<SpotTrade> spotComparer = baseComparer; // ✅ Contravariance!
        spotTrades.Sort(spotComparer);
        Console.WriteLine("    Contravariance (in T):");
        Console.WriteLine($"      IComparer<BaseTrade> → IComparer<SpotTrade>: ✓");
        Console.WriteLine($"      Sorted spots: {string.Join(", ", spotTrades.Select(t => t.Price))}");

        Console.WriteLine();

        // Practical use: accepting derived collections in methods
        PrintTrades(spotTrades); // IEnumerable<BaseTrade> param accepts List<SpotTrade>

        Console.WriteLine();
        Console.WriteLine("    ✓ Covariance: produce derived, consume as base (out T)");
        Console.WriteLine("    ✓ Contravariance: accept base, use with derived (in T)");
        Console.WriteLine("    ✓ No manual copying — zero overhead");
    }

    private static void PrintTrades(IEnumerable<BaseTrade> trades)
    {
        Console.WriteLine("    Method accepting IEnumerable<BaseTrade>:");
        foreach (var trade in trades)
            Console.WriteLine($"      → {trade.Ticker}: {trade.Price}");
    }

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // Variance makes generic interfaces behave intuitively with
    // inheritance. Without it, you'd need explicit casts or copies
    // everywhere a derived collection meets a base-typed parameter.
    // It's the reason IEnumerable<string> works where
    // IEnumerable<object> is expected.

    // GOING DEEPER:
    // Why can IEnumerable<T> be covariant but IList<T> cannot?
    // Because IList<T> has both input (Add) and output (indexer)
    // positions for T. If IList<Animal> animals = catList were
    // allowed, animals.Add(new Dog()) would put a Dog in a Cat list!
    // Covariance (out) = T only in return positions (safe to upcast).
    // Contravariance (in) = T only in parameter positions.
    // The compiler enforces this at the interface definition.

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: Define an ITradeProcessor<in T> interface with a
    // Process(T trade) method. Implement it for BaseTrade.
    // Verify that ITradeProcessor<BaseTrade> can be assigned
    // to ITradeProcessor<SpotTrade>.

    /// <summary>Base class for trade types used in variance demos.</summary>
    public class BaseTrade(string ticker, decimal price)
    {
        public string Ticker { get; } = ticker;
        public decimal Price { get; } = price;
    }

    /// <summary>Spot trade — inherits from BaseTrade.</summary>
    public sealed class SpotTrade(string ticker, decimal price)
        : BaseTrade(ticker, price);

    /// <summary>Comparer that works on any BaseTrade — demonstrates contravariance.</summary>
    private sealed class TradeByPriceComparer : IComparer<BaseTrade>
    {
        public int Compare(BaseTrade? x, BaseTrade? y) =>
            (x?.Price ?? 0m).CompareTo(y?.Price ?? 0m);
    }

    /// <summary>Runs the complete variance demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: Generic Variance (C# 4.0)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforeVariance();
        WithVariance();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
