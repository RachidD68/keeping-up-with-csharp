// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: Auto-Implemented Properties                   ║
// ║  Introduced: C# 3.0                                     ║
// ║  Theme: 0 — The Foundation                              ║
// ╚══════════════════════════════════════════════════════════╝

namespace QuantLite.Theme0_Foundation.AutoProperties;

/// <summary>
/// Demonstrates the evolution of properties from verbose backing
/// fields to modern auto-properties with init-only setters.
/// </summary>
public static class SimpleModelsDemo
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — How we did it before (C# 2.0)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Before auto-properties, every property required an explicit
    /// backing field — boilerplate that cluttered even the simplest
    /// data classes.
    /// </summary>
    public static void BeforeAutoProperties()
    {
        Console.WriteLine("  BEFORE (C# 2.0 — Explicit backing fields):");
        Console.WriteLine();

        var order = new OldStyleOrder();
        order.Ticker = "AAPL";
        order.Quantity = 100;
        order.Price = 150.00m;

        Console.WriteLine($"    Order: {order.Ticker}, {order.Quantity} @ {order.Price}");
        Console.WriteLine("    ⚠ 3 properties = 3 fields + 6 accessor methods = ~30 lines");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — The modern way (C# 3.0+)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Auto-properties eliminate backing field boilerplate. Combined
    /// with later features (init, required), they provide concise
    /// yet safe property declarations.
    /// </summary>
    public static void WithAutoProperties()
    {
        Console.WriteLine("  AFTER (C# 3.0+ — Auto-properties evolution):");
        Console.WriteLine();

        // C# 3.0: Basic auto-property
        Console.WriteLine("    C# 3.0 — Basic auto-property:");
        var basic = new BasicOrder { Ticker = "AAPL", Quantity = 100, Price = 150.00m };
        Console.WriteLine($"      {basic.Ticker}, {basic.Quantity} @ {basic.Price}");

        // C# 6.0: Getter-only auto-property with initializer
        Console.WriteLine("    C# 6.0 — Getter-only with initializer:");
        var readOnly = new ReadOnlyOrder("MSFT", 50, 420.00m);
        Console.WriteLine($"      {readOnly.Ticker}, {readOnly.Quantity} @ {readOnly.Price}");
        // readOnly.Ticker = "GOOGL"; // ✅ Compile error — getter-only

        // C# 9.0: Init-only setter
        Console.WriteLine("    C# 9.0 — Init-only setter:");
        var initOnly = new InitOnlyOrder { Ticker = "GOOGL", Quantity = 200, Price = 175.00m };
        Console.WriteLine($"      {initOnly.Ticker}, {initOnly.Quantity} @ {initOnly.Price}");
        // initOnly.Ticker = "TSLA"; // ✅ Compile error — init-only

        Console.WriteLine();
        Console.WriteLine("    ✓ Auto-properties: 1 line per property vs. ~10 lines");
        Console.WriteLine("    ✓ Getter-only: immutable after construction");
        Console.WriteLine("    ✓ Init-only: settable in initializer, then immutable");
    }

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // Properties are the most common members in C# types. Auto-properties
    // reduced the most common type definition from 30+ lines to ~5 lines.
    // This wasn't just convenience — it changed how developers think about
    // encapsulation. When properties are easy to write, developers use
    // them instead of public fields, maintaining encapsulation by default.

    // GOING DEEPER:
    // The compiler still generates a backing field for auto-properties.
    // You can see it with [field: NonSerialized] (C# 10) or with the
    // new 'field' keyword (C# 14). See Theme 1 — Field Keyword.
    // For records (Theme 2), positional parameters become auto-properties
    // automatically, taking this evolution even further.

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: Convert OldStyleOrder to use auto-properties, then to
    // a record. Compare the line count at each stage.

    /// <summary>Old-style class with explicit backing fields (C# 2.0 approach).</summary>
    private sealed class OldStyleOrder
    {
        private string _ticker = "";
        private int _quantity;
        private decimal _price;

        public string Ticker
        {
            get { return _ticker; }
            set { _ticker = value; }
        }

        public int Quantity
        {
            get { return _quantity; }
            set { _quantity = value; }
        }

        public decimal Price
        {
            get { return _price; }
            set { _price = value; }
        }
    }

    /// <summary>C# 3.0 — Basic auto-implemented properties.</summary>
    private sealed class BasicOrder
    {
        public string Ticker { get; set; } = "";
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

    /// <summary>C# 6.0 — Getter-only auto-properties, set in constructor.</summary>
    private sealed class ReadOnlyOrder(string ticker, int quantity, decimal price)
    {
        public string Ticker { get; } = ticker;
        public int Quantity { get; } = quantity;
        public decimal Price { get; } = price;
    }

    /// <summary>C# 9.0 — Init-only setters for immutable initialization.</summary>
    private sealed class InitOnlyOrder
    {
        public required string Ticker { get; init; }
        public required int Quantity { get; init; }
        public required decimal Price { get; init; }
    }

    /// <summary>Runs the complete auto-properties demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: Auto-Implemented Properties (C# 3.0+)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforeAutoProperties();
        WithAutoProperties();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
