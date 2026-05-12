// Chapter 4 — Evolution of Control Flow — BASIC
// ----------------------------------------------------------------
// Exercise: Convert the BeforeSwitchExpressions statement version
//   of ClassifyRisk into a single switch expression. Verify every
//   trade produces the same output.
//
// Hint: Flatten the nested if chains inside each case into separate
//   switch arms.
// ----------------------------------------------------------------

namespace KeepUpCs.Exercises.Ch04;

public enum TradeType { Spot, Forward, Option, Swap }
public enum RiskLevel { Low, Medium, High, Critical }

public record Money(decimal Amount, string Currency);
public record Trade(string Ticker, TradeType Type, Money Notional, string? Counterparty = null);

// ── BEFORE (C# 7 — verbose switch statement) ────────────────────
public static class ClassifierBefore
{
    public static RiskLevel ClassifyRisk(Trade trade)
    {
        RiskLevel risk;
        switch (trade.Type)
        {
            case TradeType.Spot:
                if (trade.Notional.Amount > 100_000m) risk = RiskLevel.High;
                else if (trade.Notional.Amount > 10_000m) risk = RiskLevel.Medium;
                else risk = RiskLevel.Low;
                break;
            case TradeType.Option:
                risk = RiskLevel.High;
                break;
            case TradeType.Swap:
                risk = trade.Counterparty is not null ? RiskLevel.Medium : RiskLevel.Critical;
                break;
            default:
                risk = RiskLevel.Medium;
                break;
        }
        return risk;
    }
}

// ── AFTER (C# 8 — switch expression) ────────────────────────────
public static class ClassifierAfter
{
    public static RiskLevel ClassifyRisk(Trade trade) => trade switch
    {
        { Type: TradeType.Spot,   Notional.Amount: > 100_000m }       => RiskLevel.High,
        { Type: TradeType.Spot,   Notional.Amount: > 10_000m  }       => RiskLevel.Medium,
        { Type: TradeType.Spot                                 }       => RiskLevel.Low,
        { Type: TradeType.Option                              }       => RiskLevel.High,
        { Type: TradeType.Swap,   Counterparty: not null      }       => RiskLevel.Medium,
        { Type: TradeType.Swap                                }       => RiskLevel.Critical,
        _                                                              => RiskLevel.Medium
    };
}

public static class BasicDemo
{
    public static void Run()
    {
        Console.WriteLine("Ch04 Basic — switch statement → switch expression");
        Console.WriteLine(new string('─', 60));

        var trades = new[]
        {
            new Trade("AAPL",  TradeType.Spot,   new Money(150_000m, "USD")),
            new Trade("MSFT",  TradeType.Spot,   new Money( 20_000m, "USD")),
            new Trade("TSLA",  TradeType.Spot,   new Money(  5_000m, "USD")),
            new Trade("OPT",   TradeType.Option, new Money(      1m, "USD")),
            new Trade("SWAP1", TradeType.Swap,   new Money(      1m, "USD"), "BankA"),
            new Trade("SWAP2", TradeType.Swap,   new Money(      1m, "USD")),
        };

        int parity = 0;
        foreach (var t in trades)
        {
            var before = ClassifierBefore.ClassifyRisk(t);
            var after  = ClassifierAfter .ClassifyRisk(t);
            var same   = before == after;
            if (same) parity++;
            Console.WriteLine($"  {t.Ticker,-6} {t.Type,-7}  before={before,-8} after={after,-8}  {(same ? "✓" : "✗ MISMATCH")}");
        }
        Console.WriteLine();
        Console.WriteLine($"  parity: {parity}/{trades.Length} trades produced identical output");
    }
}
