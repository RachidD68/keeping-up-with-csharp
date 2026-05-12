// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: With-Expressions (Non-Destructive Mutation)   ║
// ║  Introduced: C# 9.0                                     ║
// ║  Theme: 2 — Data Modeling                               ║
// ╚══════════════════════════════════════════════════════════╝

namespace QuantLite.Theme2_DataModeling.WithExpressions;

/// <summary>
/// Demonstrates with-expressions for creating modified copies of
/// records without mutating the original — essential for immutable
/// trade amendment workflows.
/// </summary>
public static class TradeAmendmentDemo
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — How we did it before (C# 8.0)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Before with-expressions, creating a modified copy of an
    /// immutable object required manually copying every field —
    /// tedious and error-prone when types have many properties.
    /// </summary>
    public static void BeforeWithExpressions()
    {
        Console.WriteLine("  BEFORE (C# 8.0 — Manual copy-and-modify):");
        Console.WriteLine();

        var original = TradeRecord.Create(
            "AAPL", new Money(150.00m, Currency.USD), 100, TradeType.Spot, "BankA");

        // Manual copy: must repeat every field
        var amended = new TradeRecord(
            original.Id,
            original.Ticker,
            new Money(155.00m, Currency.USD), // Changed!
            original.Quantity,
            original.Type,
            original.Timestamp,
            original.Counterparty);

        Console.WriteLine($"    Original: {original.Price}");
        Console.WriteLine($"    Amended:  {amended.Price}");
        Console.WriteLine("    ⚠ Must copy all 7 fields — easy to forget one");
        Console.WriteLine("    ⚠ Adding a new field? Must update every copy site");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — The modern way (C# 9.0)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// With-expressions copy all properties and override only the
    /// ones you specify — safe, concise, and maintenance-free.
    /// </summary>
    public static void WithWithExpressions()
    {
        Console.WriteLine("  AFTER (C# 9.0 — with-expressions):");
        Console.WriteLine();

        var original = TradeRecord.Create(
            "AAPL", new Money(150.00m, Currency.USD), 100, TradeType.Spot, "BankA");

        // Change just the price — everything else copied automatically
        var priceAmend = original with { Price = new Money(155.00m, Currency.USD) };
        Console.WriteLine($"    Price amendment: {original.Price} → {priceAmend.Price}");
        Console.WriteLine($"    Other fields preserved: Ticker={priceAmend.Ticker}, Qty={priceAmend.Quantity}");

        // Multiple field changes
        var fullAmend = original with
        {
            Price = new Money(160.00m, Currency.USD),
            Quantity = 150,
            Counterparty = "BankB"
        };
        Console.WriteLine($"    Full amendment: {fullAmend}");

        // Chain with-expressions for workflow
        Console.WriteLine("\n    Amendment workflow chain:");
        var workflow = original
            with { Price = new Money(155.00m, Currency.USD) }; // Price correction
        var workflow2 = workflow
            with { Quantity = 120 }; // Quantity adjustment
        var workflow3 = workflow2
            with { Counterparty = "BankC" }; // Counterparty change

        Console.WriteLine($"      Step 0 (original):    {original}");
        Console.WriteLine($"      Step 1 (price fix):   {workflow}");
        Console.WriteLine($"      Step 2 (qty adjust):  {workflow2}");
        Console.WriteLine($"      Step 3 (cp change):   {workflow3}");

        // Original is never modified
        Console.WriteLine($"\n    Original still intact: {original}");
        Console.WriteLine($"    original == priceAmend: {original == priceAmend}");

        Console.WriteLine();
        Console.WriteLine("    ✓ Change only what's needed — rest copied automatically");
        Console.WriteLine("    ✓ Adding new fields doesn't break existing copy sites");
        Console.WriteLine("    ✓ Chainable for multi-step workflows");
        Console.WriteLine("    ✓ Original is never mutated — safe for audit trails");
    }

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // Trade amendments are a core workflow in finance: change the price,
    // adjust the quantity, update the counterparty — but always keep
    // the original for audit. With-expressions make this natural:
    // each amendment creates a new version, the old one is preserved.
    // This is functional programming's "persistent data" pattern,
    // now native to C#.

    // GOING DEEPER:
    // Under the hood, with-expressions call the compiler-generated
    // <Clone>$ method, which creates a memberwise copy using the
    // copy constructor. Then the specified properties are overwritten.
    // For record structs, this is a simple struct copy (cheap).
    // For record classes, it's a shallow clone — reference-type
    // properties point to the same objects. If you need deep copies,
    // you must implement that manually.

    // TRADE-OFF:
    // Shallow copy means nested mutable objects are shared between
    // original and copy. For immutable records (like TradeRecord
    // containing Money), this is fine — Money is a readonly struct.
    // For mutable reference types, this could be a bug source.

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: Create a VersionedTrade record that includes a Version
    // counter. Write an Amend() method that returns trade with
    // { Price = newPrice, Version = Version + 1 } to auto-increment
    // the version on each amendment.

    /// <summary>Runs the complete with-expressions demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: With-Expressions (C# 9.0)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforeWithExpressions();
        WithWithExpressions();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
