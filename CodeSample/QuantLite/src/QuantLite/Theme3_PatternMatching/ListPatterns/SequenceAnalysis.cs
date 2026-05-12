// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: List Patterns                                 ║
// ║  Introduced: C# 11                                      ║
// ║  Theme: 3 — Pattern Matching                            ║
// ╚══════════════════════════════════════════════════════════╝

namespace QuantLite.Theme3_PatternMatching.ListPatterns;

/// <summary>
/// Demonstrates list patterns for matching on sequences —
/// analyzing price sequences, trade batches, and time series
/// by their structure.
/// </summary>
public static class SequenceAnalysisDemo
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — How we did it before (C# 10)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Before list patterns, analyzing sequences by structure
    /// required index-based access with bounds checking —
    /// verbose and easy to get wrong.
    /// </summary>
    public static void BeforeListPatterns()
    {
        Console.WriteLine("  BEFORE (C# 10 — Index-based sequence analysis):");
        Console.WriteLine();

        decimal[] prices = [100m, 105m, 103m, 110m, 108m];

        // Manual bounds-checked analysis
        string trend;
        if (prices.Length >= 2 && prices[^1] > prices[0])
            trend = "Upward";
        else if (prices.Length >= 2 && prices[^1] < prices[0])
            trend = "Downward";
        else if (prices.Length == 1)
            trend = "Single point";
        else if (prices.Length == 0)
            trend = "No data";
        else
            trend = "Flat";

        Console.WriteLine($"    Prices: [{string.Join(", ", prices)}]");
        Console.WriteLine($"    Trend: {trend}");
        Console.WriteLine("    ⚠ Manual bounds checking, index arithmetic");
        Console.WriteLine("    ⚠ Easy to have off-by-one errors");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — The modern way (C# 11)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// List patterns match on the structure and content of
    /// sequences — like regex for arrays and lists.
    /// </summary>
    public static void WithListPatterns()
    {
        Console.WriteLine("  AFTER (C# 11 — List patterns):");
        Console.WriteLine();

        // Basic list patterns
        decimal[][] priceSequences =
        [
            [],
            [100m],
            [100m, 110m],
            [100m, 105m, 103m, 110m, 108m],
            [100m, 95m, 90m, 85m],
            [100m, 100m, 100m],
        ];

        Console.WriteLine("    Sequence analysis with list patterns:");
        foreach (var prices in priceSequences)
        {
            var analysis = AnalyzeSequence(prices);
            var display = prices.Length == 0 ? "[]" : $"[{string.Join(", ", prices)}]";
            Console.WriteLine($"      {display,-35} → {analysis}");
        }

        Console.WriteLine();

        // Slice pattern (..) with capture
        Console.WriteLine("    Slice patterns (..) with capture:");
        decimal[] weekPrices = [100m, 102m, 98m, 105m, 103m];

        if (weekPrices is [var monday, .. var midWeek, var friday])
        {
            Console.WriteLine($"      Monday: {monday}");
            Console.WriteLine($"      Mid-week ({midWeek.Length} days): [{string.Join(", ", midWeek)}]");
            Console.WriteLine($"      Friday: {friday}");
        }

        Console.WriteLine();

        // Nested patterns within list patterns
        Console.WriteLine("    Trade batch validation:");
        var batches = CreateTradeBatches();
        foreach (var batch in batches)
        {
            var validation = ValidateBatch(batch);
            var tickers = string.Join(", ", batch.Select(t => t.Ticker));
            Console.WriteLine($"      [{tickers}] → {validation}");
        }

        Console.WriteLine();
        Console.WriteLine("    ✓ Match on sequence structure — length, elements, ranges");
        Console.WriteLine("    ✓ Slice patterns (..) — match 'the rest' of a sequence");
        Console.WriteLine("    ✓ Nest other patterns inside list patterns");
        Console.WriteLine("    ✓ Works with arrays, lists, Span<T>, and any indexable/countable type");
    }

    /// <summary>
    /// Analyzes a price sequence using list patterns.
    /// </summary>
    private static string AnalyzeSequence(decimal[] prices) => prices switch
    {
        // Empty sequence
        [] => "No data",

        // Single price point
        [var only] => $"Single point: {only}",

        // Exactly two points — direct comparison
        [var first, var last] when first < last => $"Up: {first} → {last}",
        [var first, var last] when first > last => $"Down: {first} → {last}",
        [var first, var last] => $"Flat: {first} = {last}",

        // Three or more: check first and last via slice
        [var first, .., var last] when first < last => $"Uptrend ({prices.Length} points)",
        [var first, .., var last] when first > last => $"Downtrend ({prices.Length} points)",
        [_, ..] => $"Sideways ({prices.Length} points)",
    };

    /// <summary>
    /// Validates a batch of trades using list patterns
    /// with nested property patterns.
    /// </summary>
    private static string ValidateBatch(TradeRecord[] batch) => batch switch
    {
        // Empty batch
        [] => "Empty batch — rejected",

        // Single trade — auto-approve if small
        [{ NotionalValue.Amount: < 10_000m }] => "Single small trade — auto-approved",

        // Single trade — needs review if large
        [_] => "Single trade — needs review",

        // All same type
        [{ Type: var t }, .. var rest] when rest.All(r => r.Type == t) =>
            $"Uniform batch ({t}) — {batch.Length} trades",

        // Mixed types
        _ => $"Mixed batch — {batch.Length} trades, manual review"
    };

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // Financial data is inherently sequential — price time series,
    // trade batches, order books. List patterns let you express
    // "the first trade, then zero or more middle trades, then the
    // last trade" directly in the pattern. This is pattern matching
    // meeting functional programming's list decomposition.

    // GOING DEEPER:
    // List patterns work with any type that has an indexer and
    // a Count/Length property. This includes:
    // - Arrays, List<T>, ImmutableArray<T>
    // - Span<T> and ReadOnlySpan<T> (important for Theme 6!)
    // - string (matches on characters)
    // The slice pattern (..) calls the type's Slice method or
    // creates a sub-array/span. For Span<T>, this is zero-copy.

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: Write a method that uses list patterns to detect
    // common candlestick patterns in price arrays:
    // - [low, high, low] → "Spike" (up then down)
    // - [high, low, high] → "Dip" (down then up)
    // - [a, b, c] where a < b < c → "Three White Soldiers"

    private static List<TradeRecord[]> CreateTradeBatches() =>
    [
        [],
        [TradeRecord.Create("AAPL", new Money(50.00m, Currency.USD), 10, TradeType.Spot)],
        [TradeRecord.Create("MEGA", new Money(500.00m, Currency.USD), 1000, TradeType.Spot)],
        [
            TradeRecord.Create("AAPL", new Money(150.00m, Currency.USD), 100, TradeType.Spot),
            TradeRecord.Create("MSFT", new Money(420.00m, Currency.USD), 50, TradeType.Spot),
            TradeRecord.Create("GOOGL", new Money(175.00m, Currency.USD), 200, TradeType.Spot),
        ],
        [
            TradeRecord.Create("AAPL", new Money(150.00m, Currency.USD), 100, TradeType.Spot),
            TradeRecord.Create("OPT1", new Money(250.00m, Currency.USD), 30, TradeType.Option),
        ],
    ];

    /// <summary>Runs the complete list patterns demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: List Patterns (C# 11)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforeListPatterns();
        WithListPatterns();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
