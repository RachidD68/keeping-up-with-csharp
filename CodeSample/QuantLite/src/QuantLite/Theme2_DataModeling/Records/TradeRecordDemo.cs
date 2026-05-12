// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: Records and Record Structs                    ║
// ║  Introduced: C# 9.0 (record) / C# 10 (record struct)   ║
// ║  Theme: 2 — Data Modeling                               ║
// ╚══════════════════════════════════════════════════════════╝

namespace QuantLite.Theme2_DataModeling.Records;

/// <summary>
/// Demonstrates records and record structs — the crown jewel of
/// C# data modeling, providing value semantics, immutability, and
/// deconstruction with minimal code.
/// </summary>
public static class TradeRecordDemoClass
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — How we did it before (C# 8.0)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Before records, creating a proper value-semantic type required
    /// ~50 lines of boilerplate: properties, constructor, Equals,
    /// GetHashCode, ToString, IEquatable, and operator overloads.
    /// </summary>
    public static void BeforeRecords()
    {
        Console.WriteLine("  BEFORE (C# 8.0 — Manual value-semantic class):");
        Console.WriteLine();

        var trade1 = new OldTrade("AAPL", 150.00m, 100, TradeType.Spot);
        var trade2 = new OldTrade("AAPL", 150.00m, 100, TradeType.Spot);

        Console.WriteLine($"    trade1: {trade1}");
        Console.WriteLine($"    trade2: {trade2}");
        Console.WriteLine($"    trade1 == trade2: {trade1 == trade2}");
        Console.WriteLine($"    ⚠ Required ~50 lines for proper Equals/GetHashCode/ToString");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — The modern way (C# 9.0+)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Records provide value equality, immutability, deconstruction,
    /// with-expressions, and ToString — all in a single line.
    /// </summary>
    public static void WithRecords()
    {
        Console.WriteLine("  AFTER (C# 9.0+ — Records):");
        Console.WriteLine();

        // record class (reference type) — one-line declaration
        var trade1 = new ModernTrade("AAPL", 150.00m, 100, TradeType.Spot);
        var trade2 = new ModernTrade("AAPL", 150.00m, 100, TradeType.Spot);

        Console.WriteLine("    record (reference type):");
        Console.WriteLine($"      trade1: {trade1}");
        Console.WriteLine($"      Value equality: trade1 == trade2: {trade1 == trade2}");
        Console.WriteLine($"      Ref equality: ReferenceEquals: {ReferenceEquals(trade1, trade2)}");

        // Deconstruction — built in
        var (ticker, price, qty, type) = trade1;
        Console.WriteLine($"      Deconstructed: ticker={ticker}, price={price}, qty={qty}");

        // with-expression — non-destructive mutation
        var amended = trade1 with { Price = 155.00m };
        Console.WriteLine($"      with-expression: {amended}");
        Console.WriteLine($"      Original unchanged: {trade1}");
        Console.WriteLine();

        // record struct (C# 10) — value type on the stack
        var money1 = new MoneySnapshot(100.00m, "USD");
        var money2 = new MoneySnapshot(100.00m, "USD");

        Console.WriteLine("    record struct (value type):");
        Console.WriteLine($"      money1: {money1}");
        Console.WriteLine($"      money1 == money2: {money1 == money2}");
        Console.WriteLine($"      Stack-allocated, zero heap pressure");
        Console.WriteLine();

        // readonly record struct (C# 10) — immutable value type
        var price1 = new PricePoint(150.25m, DateTimeOffset.UtcNow);
        Console.WriteLine("    readonly record struct:");
        Console.WriteLine($"      {price1}");
        Console.WriteLine($"      Immutable + value type = ideal for data modeling");

        Console.WriteLine();
        Console.WriteLine("    ✓ Value equality — same data = same object, logically");
        Console.WriteLine("    ✓ Immutability by default — safe to share across threads");
        Console.WriteLine("    ✓ with-expression — modify copies, not originals");
        Console.WriteLine("    ✓ Built-in ToString, Deconstruct, GetHashCode");
        Console.WriteLine("    ✓ record struct (C# 10) — value semantics on the stack");
    }

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // Records are arguably the most impactful C# feature since LINQ.
    // They make immutable data modeling the path of least resistance:
    // the easiest way to define a type is now also the safest way.
    // In financial code, immutability prevents entire categories of
    // bugs — a trade record can't be silently modified after creation.

    // GOING DEEPER:
    // record class vs record struct — when to use which:
    // - record class: For entities with identity or complex graphs.
    //   Heap-allocated, supports inheritance, nullable (T?).
    // - record struct: For small value objects (Money, Point, Color).
    //   Stack-allocated, no inheritance, no null (unless T?).
    // - readonly record struct: The gold standard for value objects.
    //   Immutable + stack-allocated. Use when Size <= ~64 bytes.
    // See also: Theme 6 — Memory & Allocation (why stack matters).

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: Create a record hierarchy: abstract record Instrument
    // with derived records StockInstrument and BondInstrument.
    // Use with-expressions to create modified copies and verify
    // that polymorphic equality works correctly.

    /// <summary>Old-style class with manual value semantics (~50 lines).</summary>
    private sealed class OldTrade : IEquatable<OldTrade>
    {
        public string Ticker { get; }
        public decimal Price { get; }
        public int Quantity { get; }
        public TradeType Type { get; }

        public OldTrade(string ticker, decimal price, int quantity, TradeType type)
        {
            Ticker = ticker;
            Price = price;
            Quantity = quantity;
            Type = type;
        }

        public bool Equals(OldTrade? other) =>
            other is not null &&
            Ticker == other.Ticker &&
            Price == other.Price &&
            Quantity == other.Quantity &&
            Type == other.Type;

        public override bool Equals(object? obj) => Equals(obj as OldTrade);

        public override int GetHashCode() =>
            HashCode.Combine(Ticker, Price, Quantity, Type);

        public static bool operator ==(OldTrade? left, OldTrade? right) =>
            Equals(left, right);

        public static bool operator !=(OldTrade? left, OldTrade? right) =>
            !Equals(left, right);

        public override string ToString() =>
            $"OldTrade {{ Ticker = {Ticker}, Price = {Price}, Qty = {Quantity}, Type = {Type} }}";
    }

    /// <summary>C# 9 record — one line, all features included.</summary>
    private record ModernTrade(string Ticker, decimal Price, int Quantity, TradeType Type);

    /// <summary>C# 10 record struct — value type with value equality.</summary>
    private record struct MoneySnapshot(decimal Amount, string CurrencyCode);

    /// <summary>C# 10 readonly record struct — immutable value type.</summary>
    private readonly record struct PricePoint(decimal Value, DateTimeOffset Timestamp);

    /// <summary>Runs the complete records demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: Records & Record Structs (C# 9.0 / 10)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforeRecords();
        WithRecords();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
