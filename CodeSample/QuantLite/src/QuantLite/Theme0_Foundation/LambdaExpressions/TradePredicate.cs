// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: Lambda Expressions                                                      ║
// ║  Introduced: C# 3.0                                                                     ║
// ║  Theme: 0 — The Foundation                                                        ║
// ╚══════════════════════════════════════════════════════════╝

namespace QuantLite.Theme0_Foundation.LambdaExpressions;

/// <summary>
/// Demonstrates lambda expressions for trade filtering —
/// from delegate syntax to modern lambda improvements.
/// </summary>
public static class TradePredicateDemo
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — How we did it before (C# 1.0/2.0)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Before lambdas, filtering required named methods or verbose
    /// anonymous delegates — lots of ceremony for simple predicates.
    /// </summary>
    public static void BeforeLambdas()
    {
        Console.WriteLine("  BEFORE (C# 2.0 — Anonymous delegates):");
        Console.WriteLine();

        var trades = CreateSampleTrades();

        // C# 1.0: Named method as delegate
        // Predicate<TradeRecord> filter = new Predicate<TradeRecord>(IsHighValue);

        // C# 2.0: Anonymous delegate — less boilerplate but still verbose
        var highValue = trades.FindAll(delegate (TradeRecord t)
        {
            return t.NotionalValue.Amount > 10_000m;
        });

        Console.WriteLine($"    Anonymous delegate found {highValue.Count} high-value trades");
        Console.WriteLine("    ⚠ Verbose syntax: 'delegate(TradeRecord t) { return ...; }'");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — The modern way (C# 3.0+)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Lambdas replace anonymous delegates with concise arrow
    /// syntax and enable the full power of LINQ.
    /// </summary>
    public static void WithLambdas()
    {
        Console.WriteLine("  AFTER (C# 3.0+ — Lambda expressions):");
        Console.WriteLine();

        var trades = CreateSampleTrades();

        // Statement lambda (C# 3.0)
        Func<TradeRecord, bool> isHighValue = (trade) =>
        {
            var threshold = 10_000m;
            return trade.NotionalValue.Amount > threshold;
        };

        // Expression lambda — the most common form
        Func<TradeRecord, bool> isSpot = t => t.Type == TradeType.Spot;

        // Natural type inference for lambdas (C# 10)
        var isOption = (TradeRecord t) => t.Type == TradeType.Option;

        // Lambda with return type annotation (C# 10)
        var describe = string (TradeRecord t) => $"{t.Ticker}: {t.NotionalValue}";

        Console.WriteLine("    Expression lambda (t => t.Type == TradeType.Spot):");
        var spots = trades.Where(isSpot).ToList();
        foreach (var trade in spots)
            Console.WriteLine($"      {describe(trade)}");

        Console.WriteLine();
        Console.WriteLine("    Statement lambda with closure:");
        var highValue = trades.Where(isHighValue).ToList();
        Console.WriteLine($"      Found {highValue.Count} high-value trades");

        Console.WriteLine();
        Console.WriteLine("    Composing predicates:");
        // Combine predicates using lambdas
        var highValueSpots = trades
            .Where(t => isHighValue(t) && isSpot(t))
            .ToList();
        Console.WriteLine($"      High-value spots: {highValueSpots.Count}");

        Console.WriteLine();
        Console.WriteLine("    ✓ Concise syntax — 't => t.X' vs 'delegate(T t) { return t.X; }'");
        Console.WriteLine("    ✓ Closures capture outer variables naturally");
        Console.WriteLine("    ✓ C# 10: natural type + return type annotations");
    }

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // Lambdas transformed C# from an OOP-only language into a
    // multi-paradigm language supporting functional programming
    // patterns. They're the glue between LINQ, event handlers,
    // Task-based async, and virtually every modern C# API.

    // GOING DEEPER:
    // Lambdas are syntactic sugar for anonymous methods, which are
    // syntactic sugar for compiler-generated private classes.
    // When a lambda captures a variable, the compiler creates a
    // "display class" to hold the captured state. This allocation
    // can matter in hot paths — see Theme 6 for static lambdas
    // and other allocation-reducing techniques.

    // TRADE-OFF:
    // Lambdas that capture variables allocate a closure object
    // on the heap. In performance-critical code, consider:
    // 1. Static lambdas (C# 9): static t => t.Price — compile
    //    error if you accidentally capture a variable.
    // 2. Passing state via a separate parameter (some APIs support this).

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: Create a PredicateBuilder that combines multiple
    // Func<TradeRecord, bool> predicates with And() and Or() methods,
    // returning a single composite predicate.

    private static List<TradeRecord> CreateSampleTrades() =>
    [
        TradeRecord.Create("AAPL", new Money(150.00m, Currency.USD), 100, TradeType.Spot),
        TradeRecord.Create("MSFT", new Money(420.00m, Currency.USD), 50, TradeType.Forward),
        TradeRecord.Create("GOOGL", new Money(175.00m, Currency.USD), 200, TradeType.Spot),
        TradeRecord.Create("TSLA", new Money(250.00m, Currency.USD), 30, TradeType.Option),
        TradeRecord.Create("AMZN", new Money(185.00m, Currency.USD), 75, TradeType.Swap),
    ];

    /// <summary>Runs the complete lambda expressions demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: Lambda Expressions (C# 3.0)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforeLambdas();
        WithLambdas();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
