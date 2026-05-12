// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: Discards (_)                                  ║
// ║  Introduced: C# 7.0                                     ║
// ║  Theme: 2 — Data Modeling                               ║
// ╚══════════════════════════════════════════════════════════╝

namespace QuantLite.Theme2_DataModeling.Discards;

/// <summary>
/// Demonstrates discards (<c>_</c>) for intentionally ignoring
/// values in deconstruction, pattern matching, and out parameters.
/// </summary>
public static class SelectiveDeconDemo
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — How we did it before (C# 6.0)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Before discards, you had to declare variables you'd never
    /// use — generating compiler warnings and cluttering the code.
    /// </summary>
    public static void BeforeDiscards()
    {
        Console.WriteLine("  BEFORE (C# 6.0 — Unused variables):");
        Console.WriteLine();

        var trade = TradeRecord.Create("AAPL", new Money(150.00m, Currency.USD), 100, TradeType.Spot);

        // Had to declare variables even when you only need one value
        var tuple = GetTradeInfo(trade);
        var ticker = tuple.Item1;     // Need this
        var quantity = tuple.Item2;   // Don't need this
        var type = tuple.Item3;       // Don't need this

        Console.WriteLine($"    Only needed ticker: {ticker}");
        Console.WriteLine($"    ⚠ Declared 'quantity' and 'type' but never used them");

        // out parameters — must declare even if unused
        if (int.TryParse("42", out var unused))
            Console.WriteLine($"    Parsed successfully, but 'unused' variable lingers");

        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — The modern way (C# 7.0+)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Discards (<c>_</c>) explicitly signal that a value is
    /// intentionally ignored — no variable is allocated.
    /// </summary>
    public static void WithDiscards()
    {
        Console.WriteLine("  AFTER (C# 7.0 — Discards):");
        Console.WriteLine();

        var trade = TradeRecord.Create("AAPL", new Money(150.00m, Currency.USD), 100, TradeType.Spot);

        // Discard unwanted tuple elements
        var (ticker, _, _) = GetTradeInfo(trade);
        Console.WriteLine($"    Deconstruction with discards: ticker = {ticker}");

        // Discard out parameters
        if (int.TryParse("42", out _))
            Console.WriteLine("    TryParse with discard: parsed successfully, value ignored");

        // Discard in pattern matching
        var description = trade.Type switch
        {
            TradeType.Spot => "Immediate settlement",
            TradeType.Option => "Derivative contract",
            _ => "Other instrument" // _ as default case
        };
        Console.WriteLine($"    Pattern match default: {description}");

        // Discard in is pattern
        if (trade is { Ticker: "AAPL", Quantity: _ })
            Console.WriteLine("    Property pattern with discard: AAPL at any quantity");

        // Discard return values intentionally
        _ = GetTradeInfo(trade); // Explicitly ignore the result
        Console.WriteLine("    Explicit discard of return value: _ = Method()");

        Console.WriteLine();
        Console.WriteLine("    ✓ _ signals intentional discard — no unused variable warnings");
        Console.WriteLine("    ✓ Works in deconstruction, out params, pattern matching");
        Console.WriteLine("    ✓ No allocation — _ is not a variable");
    }

    private static (string Ticker, int Quantity, TradeType Type) GetTradeInfo(TradeRecord trade) =>
        (trade.Ticker, trade.Quantity, trade.Type);

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // Discards improve code clarity by making intent explicit.
    // When a reader sees _, they know that value was considered
    // and deliberately ignored — it's not a bug, it's a choice.
    // This is especially important in deconstruction where multiple
    // values are returned but only some are needed.

    // GOING DEEPER:
    // The _ token has different semantics depending on context:
    // 1. In deconstruction: a true discard (no variable created)
    // 2. In switch patterns: the default/catch-all case
    // 3. As a standalone statement: _ = expr; suppresses warnings
    // 4. If you have a variable named _, it shadows the discard
    //    feature — avoid naming variables _.

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: Write a method returning (Id, Ticker, Price, Quantity, Type)
    // and call it with deconstruction, keeping only Ticker and Price.

    /// <summary>Runs the complete discards demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: Discards (C# 7.0)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforeDiscards();
        WithDiscards();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
