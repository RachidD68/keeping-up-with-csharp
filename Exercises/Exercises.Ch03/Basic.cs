// Chapter 3 — Data Modeling & Functional Techniques — BASIC
// ----------------------------------------------------------------
// Exercise: Take QuantLite's OldTrade class (the 50-line
//   IEquatable<T> implementation) and rewrite it as a record named
//   CompactTrade with the same four properties (Ticker, Price,
//   Quantity, Type). Verify that == returns true for two instances
//   with identical data, that Deconstruct works, and that
//   ToString() produces a readable output.
//
// Hint: The entire replacement is one line.
// ----------------------------------------------------------------

namespace KeepUpCs.Exercises.Ch03;

public enum TradeType { Spot, Forward, Option, Swap, Future }

// ── AFTER (C# 9 — one line) ─────────────────────────────────────
public record CompactTrade(string Ticker, decimal Price, int Quantity, TradeType Type);

public static class BasicDemo
{
    public static void Run()
    {
        Console.WriteLine("Ch03 Basic — OldTrade → CompactTrade record");
        Console.WriteLine(new string('─', 60));

        var a = new CompactTrade("AAPL", 150m, 100, TradeType.Spot);
        var b = new CompactTrade("AAPL", 150m, 100, TradeType.Spot);
        var c = new CompactTrade("AAPL", 155m, 100, TradeType.Spot);

        Console.WriteLine($"  a == b (same data)?      {a == b}");      // True
        Console.WriteLine($"  a == c (different price)?{a == c}");      // False
        Console.WriteLine($"  ReferenceEquals(a, b)?   {ReferenceEquals(a, b)}");

        // Deconstruct
        var (ticker, price, qty, type) = a;
        Console.WriteLine($"  deconstructed: ticker={ticker}, price={price}, qty={qty}, type={type}");

        // ToString (generated)
        Console.WriteLine($"  ToString(): {a}");
    }
}
