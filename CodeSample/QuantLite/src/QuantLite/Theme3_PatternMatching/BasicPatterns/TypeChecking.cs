// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: Basic Patterns (Type & Declaration Patterns)  ║
// ║  Introduced: C# 7.0                                     ║
// ║  Theme: 3 — Pattern Matching                            ║
// ╚══════════════════════════════════════════════════════════╝

namespace QuantLite.Theme3_PatternMatching.BasicPatterns;

/// <summary>
/// Demonstrates type patterns and declaration patterns — the
/// foundation of C#'s pattern matching system, replacing
/// verbose <c>is</c>/<c>as</c> chains.
/// </summary>
public static class TypeCheckingDemo
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — How we did it before (C# 6.0)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Before type patterns, checking an object's type and using
    /// it required separate <c>is</c> and <c>as</c> operations.
    /// </summary>
    public static void BeforeTypePatterns()
    {
        Console.WriteLine("  BEFORE (C# 6.0 — Separate is/as checks):");
        Console.WriteLine();

        object[] instruments = CreateInstruments();

        foreach (var instrument in instruments)
        {
            // Two steps: check type, then cast
            if (instrument is SpotInstrument)
            {
                var spot = (SpotInstrument)instrument; // redundant cast
                Console.WriteLine($"    Spot: {spot.Ticker} @ {spot.Price}");
            }
            else if (instrument is OptionInstrument)
            {
                var option = (OptionInstrument)instrument;
                Console.WriteLine($"    Option: {option.Ticker}, Strike={option.StrikePrice}");
            }
            else
            {
                Console.WriteLine($"    Unknown: {instrument.GetType().Name}");
            }
        }

        Console.WriteLine("    ⚠ Check + cast is redundant and error-prone");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — The modern way (C# 7.0+)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Type patterns combine the type check and variable declaration
    /// into a single expression.
    /// </summary>
    public static void WithTypePatterns()
    {
        Console.WriteLine("  AFTER (C# 7.0 — Type patterns):");
        Console.WriteLine();

        object[] instruments = CreateInstruments();

        // 'is' with type pattern — check and bind in one step
        Console.WriteLine("    Type patterns with 'is':");
        foreach (var instrument in instruments)
        {
            if (instrument is SpotInstrument spot)
                Console.WriteLine($"      Spot: {spot.Ticker} @ {spot.Price}");
            else if (instrument is OptionInstrument option)
                Console.WriteLine($"      Option: {option.Ticker}, Strike={option.StrikePrice}");
            else if (instrument is ForwardInstrument forward)
                Console.WriteLine($"      Forward: {forward.Ticker}, Delivery={forward.DeliveryDate:d}");
        }

        Console.WriteLine();

        // switch statement with type patterns
        Console.WriteLine("    Switch with type patterns:");
        foreach (var instrument in instruments)
        {
            var description = DescribeInstrument(instrument);
            Console.WriteLine($"      {description}");
        }

        Console.WriteLine();

        // Negation pattern (C# 9): 'is not null'
        object? maybeInstrument = instruments.FirstOrDefault();
        if (maybeInstrument is not null)
            Console.WriteLine($"    Negation: instrument is not null ✓");

        Console.WriteLine();
        Console.WriteLine("    ✓ Type check + variable binding in one expression");
        Console.WriteLine("    ✓ Works with is, switch, and when clauses");
        Console.WriteLine("    ✓ C# 9 adds negation (is not null)");
    }

    private static string DescribeInstrument(object instrument) => instrument switch
    {
        SpotInstrument s => $"Spot: {s.Ticker} immediate delivery @ {s.Price}",
        OptionInstrument o when o.StrikePrice > 200 =>
            $"Option: {o.Ticker} HIGH strike ({o.StrikePrice})",
        OptionInstrument o => $"Option: {o.Ticker} strike {o.StrikePrice}",
        ForwardInstrument f => $"Forward: {f.Ticker} delivery {f.DeliveryDate:d}",
        _ => $"Unknown: {instrument.GetType().Name}"
    };

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // Type patterns are the gateway to C#'s pattern matching system.
    // They solve the #1 most common type-checking pattern in C# —
    // "is this object of type X? If so, use it as X." The single-step
    // check-and-bind eliminates a class of bugs where the wrong
    // variable is cast or the cast is forgotten.

    // GOING DEEPER:
    // The switch expression (C# 8) vs switch statement:
    // - Expression: returns a value, must be exhaustive (cover all cases).
    // - Statement: traditional fall-through, doesn't require exhaustiveness.
    // Prefer switch expressions when you're computing a value.
    // Use switch statements for side effects (multiple actions per case).
    // The compiler warns about non-exhaustive switch expressions —
    // helping you handle all cases.

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: Add a SwapInstrument type and include it in the switch
    // expression. What happens if you forget to add the case?
    // (Hint: the compiler will warn you about non-exhaustive matching.)

    /// <summary>Base type for instruments.</summary>
    public record InstrumentBase(string Ticker);

    /// <summary>Spot instrument for immediate delivery.</summary>
    public record SpotInstrument(string Ticker, decimal Price) : InstrumentBase(Ticker);

    /// <summary>Option instrument with a strike price.</summary>
    public record OptionInstrument(string Ticker, decimal StrikePrice, DateOnly Expiry)
        : InstrumentBase(Ticker);

    /// <summary>Forward contract with a delivery date.</summary>
    public record ForwardInstrument(string Ticker, decimal ForwardPrice, DateOnly DeliveryDate)
        : InstrumentBase(Ticker);

    private static object[] CreateInstruments() =>
    [
        new SpotInstrument("AAPL", 150.00m),
        new OptionInstrument("MSFT", 450.00m, new DateOnly(2025, 6, 20)),
        new ForwardInstrument("GOOGL", 180.00m, new DateOnly(2025, 9, 15)),
        new OptionInstrument("TSLA", 180.00m, new DateOnly(2025, 3, 21)),
    ];

    /// <summary>Runs the complete type patterns demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: Basic Type Patterns (C# 7.0)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforeTypePatterns();
        WithTypePatterns();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
