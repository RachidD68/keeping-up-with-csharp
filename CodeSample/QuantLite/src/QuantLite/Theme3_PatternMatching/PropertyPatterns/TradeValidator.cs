// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: Property Patterns                             ║
// ║  Introduced: C# 8.0                                     ║
// ║  Theme: 3 — Pattern Matching                            ║
// ╚══════════════════════════════════════════════════════════╝

namespace QuantLite.Theme3_PatternMatching.PropertyPatterns;

/// <summary>
/// Demonstrates property patterns for validating trades based
/// on their properties — matching on shape rather than type.
/// </summary>
public static class TradeValidatorDemo
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — How we did it before (C# 7.0)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Before property patterns, validating object properties
    /// required chains of <c>if</c> conditions on individual
    /// properties — verbose and hard to read.
    /// </summary>
    public static void BeforePropertyPatterns()
    {
        Console.WriteLine("  BEFORE (C# 7.0 — if/else chains on properties):");
        Console.WriteLine();

        var trade = TradeRecord.Create("AAPL", new Money(150.00m, Currency.USD), 100, TradeType.Spot);

        // Validation via if/else — each condition is a separate line
        string status;
        if (trade.Type == TradeType.Spot && trade.Quantity > 0 && trade.NotionalValue.Amount > 10_000m)
            status = "Valid high-value spot trade";
        else if (trade.Type == TradeType.Option && trade.Quantity > 0)
            status = "Valid option trade";
        else if (trade.Quantity <= 0)
            status = "Invalid: non-positive quantity";
        else
            status = "Requires review";

        Console.WriteLine($"    Validation result: {status}");
        Console.WriteLine("    ⚠ Conditions scattered across multiple if branches");
        Console.WriteLine("    ⚠ Hard to see the pattern at a glance");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — The modern way (C# 8.0+)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Property patterns match on the shape of an object — checking
    /// multiple properties in a single, readable pattern.
    /// </summary>
    public static void WithPropertyPatterns()
    {
        Console.WriteLine("  AFTER (C# 8.0 — Property patterns):");
        Console.WriteLine();

        var trades = CreateSampleTrades();

        foreach (var trade in trades)
        {
            var validation = ValidateTrade(trade);
            Console.WriteLine($"    {trade.Ticker} ({trade.Type}, qty={trade.Quantity}): {validation}");
        }

        Console.WriteLine();

        // Nested property patterns (C# 10 — extended property patterns)
        Console.WriteLine("    Nested property patterns (C# 10):");
        foreach (var trade in trades)
        {
            var currencyCheck = CheckCurrency(trade);
            Console.WriteLine($"      {trade.Ticker}: {currencyCheck}");
        }

        Console.WriteLine();
        Console.WriteLine("    ✓ Match on object shape — multiple properties at once");
        Console.WriteLine("    ✓ Nested patterns — drill into sub-properties");
        Console.WriteLine("    ✓ Combine with type patterns for powerful matching");
    }

    /// <summary>
    /// Validates a trade using property patterns in a switch expression.
    /// </summary>
    private static string ValidateTrade(TradeRecord trade) => trade switch
    {
        // Property pattern: { Property: value }
        { Quantity: <= 0 } => "REJECTED: Non-positive quantity",

        // Nested properties with multiple conditions
        { Type: TradeType.Spot, Quantity: > 0, NotionalValue.Amount: > 50_000m } =>
            "APPROVED: High-value spot (senior review complete)",

        { Type: TradeType.Spot, Quantity: > 0 } =>
            "APPROVED: Standard spot trade",

        { Type: TradeType.Option, NotionalValue.Amount: > 100_000m } =>
            "ESCALATED: Large option position",

        { Type: TradeType.Option } =>
            "APPROVED: Standard option trade",

        { Type: TradeType.Swap, Counterparty: null } =>
            "REJECTED: Swap requires counterparty",

        { Type: TradeType.Swap, Counterparty: not null } =>
            "APPROVED: Swap with counterparty",

        _ => "REVIEW: Unclassified trade"
    };

    /// <summary>
    /// Extended property patterns (C# 10) — nested property access
    /// with dot notation instead of nested braces.
    /// </summary>
    private static string CheckCurrency(TradeRecord trade) => trade switch
    {
        // C# 10: trade.Price.Currency.Code instead of { Price: { Currency: { Code: "USD" } } }
        { Price.Currency.Code: "USD" } => "USD-denominated",
        { Price.Currency.Code: "EUR" } => "EUR-denominated",
        { Price.Currency.Code: var code } => $"Other currency: {code}",
    };

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // Property patterns turn validation logic from imperative code
    // (check this, then check that) into declarative patterns
    // (match this shape). The switch expression makes validation
    // exhaustive — the compiler ensures every case is handled.
    // In trade validation, this prevents the "forgot to check
    // the counterparty on swaps" category of bugs.

    // GOING DEEPER:
    // Extended property patterns (C# 10) simplified deeply nested
    // matching. Before: { Price: { Currency: { Code: "USD" } } }
    // After: { Price.Currency.Code: "USD" }
    // This makes property patterns practical for deep object graphs
    // common in financial domain models.

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: Add a MarketData property pattern that classifies
    // market conditions: { Spread: < 0.01m } => "Tight",
    // { Volume: > 1_000_000 } => "High volume", etc.

    private static List<TradeRecord> CreateSampleTrades() =>
    [
        TradeRecord.Create("AAPL", new Money(150.00m, Currency.USD), 100, TradeType.Spot),
        TradeRecord.Create("MSFT", new Money(420.00m, Currency.USD), 200, TradeType.Spot),
        TradeRecord.Create("TSLA", new Money(250.00m, Currency.USD), 500, TradeType.Option),
        TradeRecord.Create("AMZN", new Money(185.00m, Currency.USD), 0, TradeType.Forward),
        TradeRecord.Create("SWAP1", new Money(50.00m, Currency.EUR), 1000, TradeType.Swap, "BankA"),
        TradeRecord.Create("SWAP2", new Money(75.00m, Currency.EUR), 500, TradeType.Swap),
    ];

    /// <summary>Runs the complete property patterns demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: Property Patterns (C# 8.0 / 10)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforePropertyPatterns();
        WithPropertyPatterns();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
