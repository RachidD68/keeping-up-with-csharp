// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: Required Members                              ║
// ║  Introduced: C# 11                                      ║
// ║  Theme: 2 — Data Modeling                               ║
// ╚══════════════════════════════════════════════════════════╝

namespace QuantLite.Theme2_DataModeling.RequiredMembers;

/// <summary>
/// Demonstrates <c>required</c> members for enforcing property
/// initialization at compile time — no more forgotten fields.
/// </summary>
public static class ValidatedOrderDemo
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — How we did it before (C# 10)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Before required members, init-only properties could be
    /// silently skipped — the compiler didn't warn you about
    /// missing initializations.
    /// </summary>
    public static void BeforeRequired()
    {
        Console.WriteLine("  BEFORE (C# 10 — Init-only without enforcement):");
        Console.WriteLine();

        // Oops: forgot to set Ticker and Quantity!
        var order = new OptionalInitOrder
        {
            Price = 150.00m
            // Ticker is default("") — silent bug!
            // Quantity is 0 — probably not intended
        };

        Console.WriteLine($"    Order: Ticker='{order.Ticker}', Qty={order.Quantity}, Price={order.Price}");
        Console.WriteLine("    ⚠ Ticker is empty string — silent default, no compiler warning");
        Console.WriteLine("    ⚠ Quantity is 0 — probably not what was intended");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — The modern way (C# 11)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// The <c>required</c> modifier ensures properties must be set
    /// in the object initializer — the compiler enforces it.
    /// </summary>
    public static void WithRequired()
    {
        Console.WriteLine("  AFTER (C# 11 — required members):");
        Console.WriteLine();

        // All required members must be provided
        var order = new ValidatedOrder
        {
            Ticker = "AAPL",
            Quantity = 100,
            Price = 150.00m,
            Type = TradeType.Spot
        };

        // This would NOT compile:
        // var bad = new ValidatedOrder { Price = 150.00m };
        // ✅ Error: Required member 'Ticker' must be set

        Console.WriteLine($"    Order: {order.Ticker} x{order.Quantity} @ {order.Price} ({order.Type})");
        Console.WriteLine($"    Notes: {order.Notes ?? "(none)"}");

        // required + init = enforced immutable initialization
        // order.Ticker = "MSFT"; // ✅ Compile error: init-only

        Console.WriteLine();

        // Works with records too
        var config = new OrderConfig
        {
            MaxQuantity = 10_000,
            AllowedTypes = [TradeType.Spot, TradeType.Forward]
        };
        Console.WriteLine($"    Config: max={config.MaxQuantity}, types={string.Join(",", config.AllowedTypes)}");

        Console.WriteLine();
        Console.WriteLine("    ✓ Compiler enforces initialization — no silent defaults");
        Console.WriteLine("    ✓ Works with init-only for enforced immutability");
        Console.WriteLine("    ✓ Non-required properties remain optional");
        Console.WriteLine("    ✓ Works with classes, structs, and records");
    }

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // In financial systems, an order without a ticker or quantity
    // is meaningless — and dangerous. Required members shift these
    // errors from runtime (NullReferenceException, wrong trades)
    // to compile time. This is the "pit of success" principle:
    // the compiler won't let you create an invalid order.

    // GOING DEEPER:
    // required is enforced via the [SetsRequiredMembers] attribute.
    // A constructor decorated with [SetsRequiredMembers] tells the
    // compiler "I set all required members, trust me." This allows
    // constructors and required members to coexist. Use with care —
    // it opts out of the compile-time check for that constructor.

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: Create a required-members class for ConnectionSettings
    // with required Host, Port, and optional TimeoutSeconds.
    // Verify the compiler catches missing required properties.

    private sealed class OptionalInitOrder
    {
        public string Ticker { get; init; } = "";
        public int Quantity { get; init; }
        public decimal Price { get; init; }
    }

    /// <summary>
    /// Order with required members — the compiler enforces that
    /// Ticker, Quantity, Price, and Type are set in every initializer.
    /// </summary>
    private sealed class ValidatedOrder
    {
        public required string Ticker { get; init; }
        public required int Quantity { get; init; }
        public required decimal Price { get; init; }
        public required TradeType Type { get; init; }

        /// <summary>Optional — not required, defaults to null.</summary>
        public string? Notes { get; init; }
    }

    /// <summary>Record-style config with required members.</summary>
    private sealed class OrderConfig
    {
        public required int MaxQuantity { get; init; }
        public required IReadOnlyList<TradeType> AllowedTypes { get; init; }
        public bool EnableLogging { get; init; } = true;
    }

    /// <summary>Runs the complete required members demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: Required Members (C# 11)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforeRequired();
        WithRequired();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
