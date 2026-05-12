// Chapter 3 — Data Modeling & Functional Techniques — INTERMEDIATE
// ----------------------------------------------------------------
// Exercise: Build a VersionedTrade record that tracks the version
//   of a trade as it's amended. It should have Ticker, Price,
//   Quantity, and Version as required init-only properties, with
//   Version defaulting to 1 on first creation. Write an extension
//   method Amend(this VersionedTrade trade, decimal newPrice) that
//   returns trade with { Price = newPrice, Version = trade.Version + 1 }.
//   Chain three amendments and verify the version counter increments.
//
// Hint: Extension methods on records work the same way as on any
//   other type.
// ----------------------------------------------------------------

namespace KeepUpCs.Exercises.Ch03;

public record VersionedTrade
{
    public required string Ticker   { get; init; }
    public required decimal Price   { get; init; }
    public required int    Quantity { get; init; }
    public         int    Version  { get; init; } = 1;
}

public static class VersionedTradeExtensions
{
    public static VersionedTrade Amend(this VersionedTrade trade, decimal newPrice) =>
        trade with { Price = newPrice, Version = trade.Version + 1 };
}

public static class IntermediateDemo
{
    public static void Run()
    {
        Console.WriteLine("Ch03 Intermediate — VersionedTrade.Amend()");
        Console.WriteLine(new string('─', 60));

        var v1 = new VersionedTrade { Ticker = "MSFT", Price = 420m, Quantity = 50 };
        Console.WriteLine($"  v1: {v1}");

        var v2 = v1.Amend(421.50m);
        Console.WriteLine($"  v2: {v2}");

        var v3 = v2.Amend(425m);
        Console.WriteLine($"  v3: {v3}");

        var v4 = v3.Amend(419.75m);
        Console.WriteLine($"  v4: {v4}");

        Console.WriteLine();
        Console.WriteLine($"  v1.Version = {v1.Version}  (original unchanged)");
        Console.WriteLine($"  v4.Version = {v4.Version}  (counter incremented three times)");
    }
}
