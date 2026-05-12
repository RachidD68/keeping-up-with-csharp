// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: Named & Optional Parameters                   ║
// ║  Introduced: C# 4.0                                     ║
// ║  Theme: 0 — The Foundation                              ║
// ╚══════════════════════════════════════════════════════════╝

namespace QuantLite.Theme0_Foundation.NamedOptionalParams;

/// <summary>
/// Demonstrates named and optional parameters in order-building
/// APIs — reducing overload proliferation and improving call-site
/// readability.
/// </summary>
public static class OrderBuilderDemo
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — How we did it before (C# 3.0)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Without optional parameters, providing flexible APIs required
    /// many overloads — the "telescoping constructor" anti-pattern.
    /// </summary>
    public static void BeforeNamedOptional()
    {
        Console.WriteLine("  BEFORE (C# 3.0 — Overload proliferation):");
        Console.WriteLine();

        // Multiple overloads for different combinations
        var order1 = OldOrderApi.CreateOrder("AAPL", 100);
        var order2 = OldOrderApi.CreateOrder("AAPL", 100, 150.00m);
        var order3 = OldOrderApi.CreateOrder("AAPL", 100, 150.00m, TradeType.Spot);

        Console.WriteLine($"    3 overloads needed for optional arguments");
        Console.WriteLine($"    Order 1: {order1}");
        Console.WriteLine($"    Order 2: {order2}");
        Console.WriteLine($"    Order 3: {order3}");
        Console.WriteLine("    ⚠ N optional params → 2^N potential overloads");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — The modern way (C# 4.0)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Named and optional parameters collapse multiple overloads
    /// into a single method while improving readability at call sites.
    /// </summary>
    public static void WithNamedOptional()
    {
        Console.WriteLine("  AFTER (C# 4.0 — Named & optional parameters):");
        Console.WriteLine();

        // All defaults
        var order1 = CreateOrder("AAPL", 100);
        Console.WriteLine($"    All defaults: {order1}");

        // Skip middle parameters with named args
        var order2 = CreateOrder("MSFT", 50, type: TradeType.Forward);
        Console.WriteLine($"    Named arg (skip price): {order2}");

        // Named args for clarity at call site
        var order3 = CreateOrder(
            ticker: "GOOGL",
            quantity: 200,
            limitPrice: 175.00m,
            type: TradeType.Spot,
            counterparty: "BankA");
        Console.WriteLine($"    All named: {order3}");

        // Named args can be in any order
        var order4 = CreateOrder(
            quantity: 30,
            ticker: "TSLA",
            counterparty: "BankB",
            type: TradeType.Option);
        Console.WriteLine($"    Reordered named: {order4}");

        Console.WriteLine();
        Console.WriteLine("    ✓ Single method replaces N overloads");
        Console.WriteLine("    ✓ Named arguments improve readability at call sites");
        Console.WriteLine("    ✓ Skip parameters you don't care about");
        Console.WriteLine("    ✓ Named args can appear in any order");
    }

    /// <summary>
    /// Single method with optional parameters — replaces 4+ overloads.
    /// </summary>
    private static string CreateOrder(
        string ticker,
        int quantity,
        decimal? limitPrice = null,
        TradeType type = TradeType.Spot,
        string? counterparty = null)
    {
        var priceStr = limitPrice.HasValue ? $"@ {limitPrice:F2}" : "@ Market";
        var cpStr = counterparty ?? "OTC";
        return $"{type} {ticker} x{quantity} {priceStr} [{cpStr}]";
    }

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // Named parameters transform cryptic call sites like
    // CreateOrder("AAPL", 100, true, false, null) into self-documenting
    // CreateOrder(ticker: "AAPL", quantity: 100, isMarket: true).
    // This is especially valuable in financial APIs where parameter
    // order mistakes can be costly.

    // GOING DEEPER:
    // Default parameter values are baked into the call site at
    // compile time, not stored in the method. This means: if you
    // change a default value and only recompile the library (not
    // the caller), the caller still uses the old default. This is
    // a versioning concern for public APIs — prefer overloads for
    // public library methods where defaults might change.

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: Add a "currency" optional parameter (default: Currency.USD)
    // and an "urgency" parameter (default: "Normal"). Call the method
    // using named arguments to set only the urgency to "High".

    private static class OldOrderApi
    {
        public static string CreateOrder(string ticker, int qty) =>
            $"Spot {ticker} x{qty} @ Market [OTC]";

        public static string CreateOrder(string ticker, int qty, decimal price) =>
            $"Spot {ticker} x{qty} @ {price:F2} [OTC]";

        public static string CreateOrder(string ticker, int qty, decimal price, TradeType type) =>
            $"{type} {ticker} x{qty} @ {price:F2} [OTC]";
    }

    /// <summary>Runs the complete named & optional parameters demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: Named & Optional Parameters (C# 4.0)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforeNamedOptional();
        WithNamedOptional();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
