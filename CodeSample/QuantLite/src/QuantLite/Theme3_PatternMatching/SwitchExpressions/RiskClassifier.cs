// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: Switch Expressions                            ║
// ║  Introduced: C# 8.0                                     ║
// ║  Theme: 3 — Pattern Matching                            ║
// ╚══════════════════════════════════════════════════════════╝

namespace QuantLite.Theme3_PatternMatching.SwitchExpressions;

/// <summary>
/// The flagship demonstration of Theme 3 — a single switch expression
/// that classifies trades by risk using type, property, relational,
/// and logical patterns, growing in complexity as each pattern is
/// introduced.
/// </summary>
public static class RiskClassifierDemo
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — How we did it before (C# 7.0)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Before switch expressions, risk classification required a
    /// verbose switch statement with fallthrough, breaks, and
    /// assignments — scattered logic that's hard to maintain.
    /// </summary>
    public static void BeforeSwitchExpressions()
    {
        Console.WriteLine("  BEFORE (C# 7.0 — Switch statement):");
        Console.WriteLine();

        var trades = CreateSampleTrades();

        foreach (var trade in trades)
        {
            // Verbose switch statement with assignments
            RiskLevel risk;
            switch (trade.Type)
            {
                case TradeType.Spot:
                    if (trade.NotionalValue.Amount > 100_000m)
                        risk = RiskLevel.High;
                    else if (trade.NotionalValue.Amount > 10_000m)
                        risk = RiskLevel.Medium;
                    else
                        risk = RiskLevel.Low;
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

            Console.WriteLine($"    {trade.Ticker} ({trade.Type}): {risk}");
        }

        Console.WriteLine("    ⚠ 20+ lines for classification logic");
        Console.WriteLine("    ⚠ Risk of forgetting 'break' — fallthrough bugs");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — The modern way (C# 8.0)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Switch expressions are concise, exhaustive, and return values
    /// directly — the same classification in ~10 lines.
    /// </summary>
    public static void WithSwitchExpressions()
    {
        Console.WriteLine("  AFTER (C# 8.0 — Switch expression):");
        Console.WriteLine();

        var trades = CreateSampleTrades();

        Console.WriteLine("    Risk classification via switch expression:");
        foreach (var trade in trades)
        {
            var risk = ClassifyRisk(trade);
            var symbol = risk switch
            {
                RiskLevel.Low => "🟢",
                RiskLevel.Medium => "🟡",
                RiskLevel.High => "🟠",
                RiskLevel.Critical => "🔴",
                _ => "⚪"
            };
            Console.WriteLine($"    {symbol} {trade.Ticker,-8} {trade.Type,-10} " +
                              $"Notional: {trade.NotionalValue.Amount,12:N2}  → {risk}");
        }

        Console.WriteLine();

        // Tuple pattern — matching multiple values simultaneously
        Console.WriteLine("    Tuple patterns (match multiple inputs):");
        var scenarios = new (TradeType Type, bool HasCounterparty, decimal Notional)[]
        {
            (TradeType.Spot, true, 5_000m),
            (TradeType.Option, false, 50_000m),
            (TradeType.Swap, true, 200_000m),
            (TradeType.Swap, false, 200_000m),
        };

        foreach (var (type, hasCp, notional) in scenarios)
        {
            var action = (type, hasCp, notional) switch
            {
                (TradeType.Spot, _, < 10_000m) => "Auto-approve",
                (TradeType.Spot, _, _) => "Manager review",
                (TradeType.Option, _, > 100_000m) => "Director review",
                (TradeType.Option, _, _) => "Manager review",
                (TradeType.Swap, false, _) => "REJECT — no counterparty",
                (TradeType.Swap, true, > 500_000m) => "Board approval",
                (TradeType.Swap, true, _) => "Manager review",
                _ => "Standard review"
            };
            Console.WriteLine($"      ({type}, CP={hasCp}, {notional:N0}) → {action}");
        }

        Console.WriteLine();
        Console.WriteLine("    ✓ Expression returns a value — no assignment needed");
        Console.WriteLine("    ✓ Exhaustive — compiler warns about missing cases");
        Console.WriteLine("    ✓ No break/fallthrough — each arm is independent");
        Console.WriteLine("    ✓ Tuple patterns — match multiple dimensions");
    }

    /// <summary>
    /// Classifies trade risk using a switch expression that combines
    /// property patterns, relational patterns, and logical patterns.
    /// This is the showcase method — all Theme 3 patterns in one place.
    /// </summary>
    public static RiskLevel ClassifyRisk(TradeRecord trade) => trade switch
    {
        // Critical: swaps without counterparty
        { Type: TradeType.Swap, Counterparty: null } => RiskLevel.Critical,

        // Critical: any trade with notional > $500K
        { NotionalValue.Amount: > 500_000m } => RiskLevel.Critical,

        // High: options are inherently riskier
        { Type: TradeType.Option, NotionalValue.Amount: > 50_000m } => RiskLevel.High,

        // High: large spot or forward positions
        { Type: TradeType.Spot or TradeType.Forward, NotionalValue.Amount: > 100_000m } =>
            RiskLevel.High,

        // Medium: moderate positions
        { NotionalValue.Amount: > 10_000m and <= 100_000m } => RiskLevel.Medium,

        // Medium: options with moderate size
        { Type: TradeType.Option } => RiskLevel.Medium,

        // Low: everything else (small positions)
        _ => RiskLevel.Low
    };

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // The ClassifyRisk method above is a real-world risk engine in
    // ~15 lines. It's readable (each pattern reads like a rule),
    // exhaustive (the compiler ensures all trades are classified),
    // and maintainable (adding a new rule is adding one line).
    // Compare this to the 25+ line switch statement above.

    // GOING DEEPER:
    // Switch expressions support "when" guards for additional logic:
    //   { Type: TradeType.Spot } when DateTime.UtcNow.Hour > 16
    //       => RiskLevel.High, // after-hours trading
    // Guards run after the pattern matches, for conditions that
    // can't be expressed as patterns (like time-based rules).

    // TRADE-OFF:
    // Very complex switch expressions (10+ arms) can become hard to
    // read. Consider splitting into multiple methods or using a
    // strategy pattern for truly complex classification logic.

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: Add a when guard to ClassifyRisk that classifies any
    // trade with a JPY currency as Medium risk regardless of size
    // (due to yen volatility). Place it before the catch-all.

    private static List<TradeRecord> CreateSampleTrades() =>
    [
        TradeRecord.Create("AAPL", new Money(150.00m, Currency.USD), 100, TradeType.Spot),
        TradeRecord.Create("MEGACORP", new Money(500.00m, Currency.USD), 2000, TradeType.Spot),
        TradeRecord.Create("TSLA", new Money(250.00m, Currency.USD), 300, TradeType.Option),
        TradeRecord.Create("BOND-A", new Money(1000.00m, Currency.EUR), 100, TradeType.Forward),
        TradeRecord.Create("IRS-1", new Money(100.00m, Currency.USD), 5000, TradeType.Swap, "BankA"),
        TradeRecord.Create("CDS-1", new Money(200.00m, Currency.USD), 1000, TradeType.Swap),
        TradeRecord.Create("SMALL", new Money(10.00m, Currency.USD), 5, TradeType.Spot),
    ];

    /// <summary>Runs the complete switch expressions demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: Switch Expressions (C# 8.0)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforeSwitchExpressions();
        WithSwitchExpressions();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
