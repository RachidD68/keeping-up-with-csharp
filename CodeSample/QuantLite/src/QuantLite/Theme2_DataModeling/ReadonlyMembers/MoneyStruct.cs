// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: Readonly Members (on structs)                 ║
// ║  Introduced: C# 8.0                                     ║
// ║  Theme: 2 — Data Modeling                               ║
// ╚══════════════════════════════════════════════════════════╝

namespace QuantLite.Theme2_DataModeling.ReadonlyMembers;

/// <summary>
/// Demonstrates <c>readonly</c> members on structs — marking individual
/// methods and properties as non-mutating so the compiler can optimize
/// and readers can trust the contract.
/// </summary>
public static class MoneyStructDemo
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — How we did it before (C# 7.2)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Before readonly members, you had two choices for structs:
    /// make the entire struct readonly (all members non-mutating)
    /// or make none of it readonly (no optimization guarantees).
    /// </summary>
    public static void BeforeReadonlyMembers()
    {
        Console.WriteLine("  BEFORE (C# 7.2 — All-or-nothing readonly struct):");
        Console.WriteLine();

        // Option A: Mutable struct — no readonly guarantees
        var mutablePrice = new MutablePricePoint { Value = 100m, Timestamp = DateTimeOffset.UtcNow };
        Console.WriteLine($"    Mutable: {mutablePrice.Display()}");

        // Option B: Fully readonly struct — can't have any mutable methods
        var readonlyPrice = new FullyReadonlyPrice(100m, DateTimeOffset.UtcNow);
        Console.WriteLine($"    Readonly: {readonlyPrice.Display()}");

        Console.WriteLine("    ⚠ No middle ground: either ALL readonly or NONE");
        Console.WriteLine("    ⚠ With mutable struct, compiler makes defensive copies in readonly context");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — The modern way (C# 8.0)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Readonly members let you mark individual methods/properties
    /// as non-mutating, even in a mutable struct. The compiler uses
    /// this to avoid unnecessary defensive copies.
    /// </summary>
    public static void WithReadonlyMembers()
    {
        Console.WriteLine("  AFTER (C# 8.0 — Readonly members):");
        Console.WriteLine();

        var position = new TradingPosition
        {
            Ticker = "AAPL",
            Quantity = 100,
            AveragePrice = 150.00m
        };

        // readonly methods — guaranteed not to modify the struct
        Console.WriteLine($"    Position: {position.Summary}");
        Console.WriteLine($"    Notional: {position.CalculateNotional()}");
        Console.WriteLine($"    Is Long: {position.IsLong}");

        // Non-readonly method — can modify
        position.AdjustQuantity(50);
        Console.WriteLine($"    After adjustment: {position.Summary}");

        // Readonly + in parameter — no defensive copy needed
        PrintPosition(in position);

        Console.WriteLine();
        Console.WriteLine("    ✓ Fine-grained readonly — per-member, not per-struct");
        Console.WriteLine("    ✓ Compiler avoids defensive copies for readonly members");
        Console.WriteLine("    ✓ Self-documenting: readers know which methods don't mutate");
    }

    // readonly member on the method — compiler verifies it doesn't mutate
    private static void PrintPosition(in TradingPosition position)
    {
        // 'in' parameter + readonly members = zero defensive copies
        Console.WriteLine($"    [in parameter] {position.Summary}");
    }

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // Structs are value types — when you pass one by reference with
    // 'in' (read-only reference), the compiler must ensure the method
    // doesn't modify it. Without readonly members, the compiler makes
    // a "defensive copy" (duplicating the struct) before calling any
    // method. This silent copy can negate the performance benefit
    // of using structs. Readonly members eliminate this overhead.

    // GOING DEEPER:
    // The defensive copy problem is subtle and dangerous:
    //   readonly TradingPosition _position; // field
    //   var n = _position.CalculateNotional(); // defensive copy!
    // If CalculateNotional is NOT marked readonly, the compiler copies
    // the entire struct to a temporary, calls the method on the copy,
    // and discards it. For large structs, this is expensive.
    // Marking it readonly: the compiler knows it's safe to call
    // directly — no copy needed.
    // See also: Theme 6 — ref struct and Span<T> (performance-critical
    // value types where defensive copies really hurt).

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: Add a readonly method to TradingPosition called
    // CalculatePnL(decimal currentPrice) that returns the unrealized
    // P&L. Verify it compiles and doesn't require a defensive copy.

    private struct MutablePricePoint
    {
        public decimal Value;
        public DateTimeOffset Timestamp;

        public string Display() => $"{Value:F2} @ {Timestamp:HH:mm:ss}";
    }

    private readonly struct FullyReadonlyPrice(decimal value, DateTimeOffset timestamp)
    {
        public decimal Value { get; } = value;
        public DateTimeOffset Timestamp { get; } = timestamp;

        public string Display() => $"{Value:F2} @ {Timestamp:HH:mm:ss}";
    }

    /// <summary>
    /// A struct with a mix of readonly and non-readonly members.
    /// Readonly members are guaranteed not to modify state.
    /// </summary>
    private struct TradingPosition
    {
        public string Ticker;
        public int Quantity;
        public decimal AveragePrice;

        // readonly property — compiler verifies no mutation
        public readonly string Summary =>
            $"{Ticker}: {Quantity} @ {AveragePrice:F2} (Notional: {CalculateNotional():N2})";

        // readonly property
        public readonly bool IsLong => Quantity > 0;

        // readonly method — safe to call on 'in' parameters
        public readonly decimal CalculateNotional() =>
            Math.Abs(Quantity) * AveragePrice;

        // NON-readonly method — mutates state
        public void AdjustQuantity(int delta)
        {
            Quantity += delta;
        }
    }

    /// <summary>Runs the complete readonly members demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: Readonly Members (C# 8.0)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforeReadonlyMembers();
        WithReadonlyMembers();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
