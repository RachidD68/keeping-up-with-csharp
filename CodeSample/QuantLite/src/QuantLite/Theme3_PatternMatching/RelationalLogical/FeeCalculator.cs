// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: Relational & Logical Patterns                 ║
// ║  Introduced: C# 9.0                                     ║
// ║  Theme: 3 — Pattern Matching                            ║
// ╚══════════════════════════════════════════════════════════╝

namespace QuantLite.Theme3_PatternMatching.RelationalLogical;

/// <summary>
/// Demonstrates relational patterns (<c>&lt;</c>, <c>&gt;</c>,
/// <c>&lt;=</c>, <c>&gt;=</c>) and logical patterns (<c>and</c>,
/// <c>or</c>, <c>not</c>) for tier-based fee calculations.
/// </summary>
public static class FeeCalculatorDemo
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — How we did it before (C# 8.0)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Before relational patterns, range-based logic required
    /// verbose if/else chains with repeated variable references.
    /// </summary>
    public static void BeforeRelationalPatterns()
    {
        Console.WriteLine("  BEFORE (C# 8.0 — if/else chains for ranges):");
        Console.WriteLine();

        decimal[] notionals = [1_000m, 15_000m, 75_000m, 250_000m, 750_000m];

        foreach (var notional in notionals)
        {
            decimal feeRate;
            if (notional < 5_000m)
                feeRate = 0.005m; // 0.5%
            else if (notional >= 5_000m && notional < 25_000m)
                feeRate = 0.003m; // 0.3%
            else if (notional >= 25_000m && notional < 100_000m)
                feeRate = 0.002m; // 0.2%
            else if (notional >= 100_000m && notional < 500_000m)
                feeRate = 0.001m; // 0.1%
            else
                feeRate = 0.0005m; // 0.05%

            Console.WriteLine($"    {notional,12:N0} → {feeRate:P2} = {notional * feeRate:N2} fee");
        }

        Console.WriteLine("    ⚠ 'notional' repeated in every condition");
        Console.WriteLine("    ⚠ Easy to get boundary conditions wrong (< vs <=)");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — The modern way (C# 9.0)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Relational and logical patterns express ranges directly
    /// in the pattern — clean, correct, and exhaustive.
    /// </summary>
    public static void WithRelationalPatterns()
    {
        Console.WriteLine("  AFTER (C# 9.0 — Relational & logical patterns):");
        Console.WriteLine();

        decimal[] notionals = [1_000m, 15_000m, 75_000m, 250_000m, 750_000m];

        Console.WriteLine("    Fee tier calculation:");
        foreach (var notional in notionals)
        {
            var (rate, tier) = CalculateFee(notional);
            var fee = notional * rate;
            Console.WriteLine($"    {notional,12:N0} → {tier,-12} ({rate:P2}) = {fee:N2}");
        }

        Console.WriteLine();

        // Logical patterns: and, or, not
        Console.WriteLine("    Logical patterns (and, or, not):");
        TradeType[] types = [TradeType.Spot, TradeType.Forward, TradeType.Option, TradeType.Swap];
        foreach (var type in types)
        {
            var category = ClassifyType(type);
            Console.WriteLine($"      {type,-10} → {category}");
        }

        Console.WriteLine();

        // Combined: relational + logical + property patterns
        Console.WriteLine("    Combined patterns in trade validation:");
        var trades = CreateSampleTrades();
        foreach (var trade in trades)
        {
            var feeCategory = CategorizeFee(trade);
            Console.WriteLine($"      {trade.Ticker,-8} ({trade.Type}, {trade.NotionalValue.Amount:N0}) → {feeCategory}");
        }

        Console.WriteLine();
        Console.WriteLine("    ✓ Relational: <, >, <=, >= directly in patterns");
        Console.WriteLine("    ✓ Logical: 'and', 'or', 'not' combine patterns");
        Console.WriteLine("    ✓ Ranges: '> 5000 and < 25000' — no variable repetition");
    }

    /// <summary>
    /// Calculates fee rate and tier name using relational patterns.
    /// </summary>
    private static (decimal Rate, string Tier) CalculateFee(decimal notional) => notional switch
    {
        < 5_000m => (0.005m, "Retail"),
        >= 5_000m and < 25_000m => (0.003m, "Standard"),
        >= 25_000m and < 100_000m => (0.002m, "Professional"),
        >= 100_000m and < 500_000m => (0.001m, "Institutional"),
        >= 500_000m => (0.0005m, "Prime"),
    };

    /// <summary>
    /// Classifies trade types using logical patterns (or, not).
    /// </summary>
    private static string ClassifyType(TradeType type) => type switch
    {
        TradeType.Spot or TradeType.Forward => "Linear instrument",
        not (TradeType.Spot or TradeType.Forward) => "Non-linear/structured",
    };

    /// <summary>
    /// Combines property, relational, and logical patterns.
    /// </summary>
    private static string CategorizeFee(TradeRecord trade) => trade switch
    {
        { Type: TradeType.Spot, NotionalValue.Amount: < 10_000m } =>
            "Standard spot fee",

        { Type: TradeType.Spot or TradeType.Forward, NotionalValue.Amount: >= 10_000m and < 100_000m } =>
            "Discounted linear fee",

        { Type: TradeType.Spot or TradeType.Forward, NotionalValue.Amount: >= 100_000m } =>
            "Prime linear fee",

        { Type: TradeType.Option or TradeType.Swap, NotionalValue.Amount: >= 50_000m } =>
            "Complex product premium",

        { Type: not TradeType.Spot } =>
            "Standard complex fee",

        _ => "Standard fee"
    };

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // Fee tiers, tax brackets, credit ratings — financial systems
    // are full of range-based classification. Relational patterns
    // make these ranges readable and correct. The 'and' pattern
    // eliminates the classic off-by-one error in boundary conditions
    // because you express the range directly, not as two separate
    // conditions.

    // GOING DEEPER:
    // The 'not' pattern is particularly useful for null checking:
    //   obj is not null   — clearer than !(obj is null)
    //   str is not ""     — express what IS valid, not what isn't
    // Combined with C# 9's 'and'/'or', you can express complex
    // conditions that would require parenthesized boolean expressions
    // in traditional if/else, directly in the pattern language.

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: Add a CalculateDiscount method that gives volume discounts:
    // - quantity 1-99: 0%, 100-499: 5%, 500-999: 10%, 1000+: 15%
    // Use relational patterns with 'and' for the ranges.

    private static List<TradeRecord> CreateSampleTrades() =>
    [
        TradeRecord.Create("AAPL", new Money(50.00m, Currency.USD), 100, TradeType.Spot),
        TradeRecord.Create("MSFT", new Money(420.00m, Currency.USD), 100, TradeType.Forward),
        TradeRecord.Create("TSLA", new Money(250.00m, Currency.USD), 500, TradeType.Option),
        TradeRecord.Create("BOND", new Money(1000.00m, Currency.USD), 200, TradeType.Swap),
    ];

    /// <summary>Runs the complete relational/logical patterns demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: Relational & Logical Patterns (C# 9.0)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforeRelationalPatterns();
        WithRelationalPatterns();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
