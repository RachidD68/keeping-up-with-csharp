// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: Nullable Value Types                          ║
// ║  Introduced: C# 2.0                                     ║
// ║  Theme: 0 — The Foundation                              ║
// ╚══════════════════════════════════════════════════════════╝

namespace QuantLite.Theme0_Foundation.NullableValueTypes;

/// <summary>
/// Demonstrates nullable value types (<c>T?</c>) for optional pricing
/// in financial calculations where a price may not yet be available.
/// </summary>
public static class OptionalPriceDemo
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — How we did it before (C# 1.0)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Before nullable value types, developers used sentinel values
    /// (e.g., <c>-1</c> or <c>decimal.MinValue</c>) to represent
    /// "no value" — error-prone and semantically misleading.
    /// </summary>
    public static void BeforeNullableValueTypes()
    {
        Console.WriteLine("  BEFORE (C# 1.0 — Sentinel values):");
        Console.WriteLine();

        // Using -1 to mean "no price available"
        decimal spotPrice = 150.25m;
        decimal forwardPrice = -1m; // sentinel: "not available yet"

        // Every consumer must know the magic value
        if (forwardPrice == -1m)
            Console.WriteLine("    Forward price not available (sentinel: -1)");

        // BUG: What if -1 is a legitimate value? (Short selling!)
        // BUG: What if someone forgets to check for the sentinel?
        var total = spotPrice + forwardPrice; // -1 gets included in calculation!
        Console.WriteLine($"    Accidental total with sentinel: {total:F2}");
        Console.WriteLine("    ⚠ Sentinel value polluted the calculation!");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — The modern way (C# 2.0+)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// With <c>decimal?</c>, the absence of a value is explicit.
    /// The compiler helps you handle the null case.
    /// </summary>
    public static void WithNullableValueTypes()
    {
        Console.WriteLine("  AFTER (C# 2.0 — Nullable<T> / T?):");
        Console.WriteLine();

        decimal spotPrice = 150.25m;
        decimal? forwardPrice = null; // Explicitly: no price available

        // HasValue and Value properties
        Console.WriteLine($"    Spot price available: {spotPrice}");
        Console.WriteLine($"    Forward price available: {forwardPrice.HasValue}");

        // Null-coalescing operator (??) for safe defaults
        var safeForward = forwardPrice ?? 0m;
        Console.WriteLine($"    Forward price (with default): {safeForward:F2}");

        // Pattern matching with nullable (modern approach)
        var message = forwardPrice switch
        {
            decimal price when price > 0 => $"Forward at {price:F2}",
            decimal price => $"Negative forward at {price:F2}",
            null => "Forward price pending"
        };
        Console.WriteLine($"    Status: {message}");

        // GetValueOrDefault — avoids exception
        Console.WriteLine($"    GetValueOrDefault: {forwardPrice.GetValueOrDefault(0m):F2}");

        Console.WriteLine();
        Console.WriteLine("    ✓ Null explicitly represents 'no value' — no sentinel needed");
        Console.WriteLine("    ✓ Compiler warns about unhandled null cases");
        Console.WriteLine("    ✓ ??, ?., and pattern matching make null handling elegant");
    }

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // In financial systems, the difference between "zero" and "unknown"
    // is critical. A trade with a $0 settlement is very different from
    // a trade whose settlement hasn't been calculated yet. Nullable
    // value types make this distinction explicit in the type system.

    // GOING DEEPER:
    // Nullable<T> is implemented as a struct with a bool HasValue field.
    // For decimal?, the struct includes the 16-byte decimal value plus
    // a HasValue bool, with alignment padding bringing the total to
    // 20 bytes in the default layout — still a value type, no heap allocation.
    // See also: Theme 5 — Nullable Reference Types (the other half of
    // C#'s null safety story, introduced in C# 8.0).

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: Create a MarketQuote record with nullable Bid? and Ask?
    // properties. Write a method that calculates the spread only when
    // both prices are available, returning decimal?.

    /// <summary>Runs the complete nullable value types demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: Nullable Value Types (C# 2.0)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforeNullableValueTypes();
        WithNullableValueTypes();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
