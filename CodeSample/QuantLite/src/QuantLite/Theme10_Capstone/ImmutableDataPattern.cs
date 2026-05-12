// ╔══════════════════════════════════════════════════════════╗
// ║  Theme 10 — Capstone: Immutable Data Pattern            ║
// ║  "Combine records, with-expressions, pattern matching,  ║
// ║   and value types into a production-grade immutable      ║
// ║   domain model"                                         ║
// ╚══════════════════════════════════════════════════════════╝
//
// Features Combined:
// ┌─────────────────────────────┬──────────┬─────────────────┐
// │ Feature                     │ C# Ver.  │ Theme           │
// ├─────────────────────────────┼──────────┼─────────────────┤
// │ Records                     │ C# 9     │ Theme 2         │
// │ With-Expressions            │ C# 9     │ Theme 2         │
// │ Init-Only Properties        │ C# 9     │ Theme 2         │
// │ Required Members            │ C# 11    │ Theme 2         │
// │ Switch Expressions          │ C# 8     │ Theme 3         │
// │ Property Patterns           │ C# 8     │ Theme 3         │
// │ Collection Expressions      │ C# 12    │ Theme 1         │
// └─────────────────────────────┴──────────┴─────────────────┘
//
// Scenario: A trade lifecycle system where trades progress through
// states (Pending → Validated → Executed → Settled) as immutable
// records. Each state transition creates a new version, preserving
// the full audit trail. Pattern matching drives the state machine.

namespace QuantLite.Theme10_Capstone;

// ──────────────────────────────────────────────────────────
// Domain: Immutable Trade Lifecycle
// ──────────────────────────────────────────────────────────

/// <summary>Represents the current state of a trade in its lifecycle.</summary>
public enum TradeState
{
    Pending,
    Validated,
    Executed,
    Settled,
    Rejected,
    Cancelled
}

/// <summary>
/// An immutable trade that progresses through lifecycle states.
/// Each state transition creates a new record via with-expressions,
/// preserving the complete audit trail.
/// </summary>
/// <remarks>
/// This record combines: positional parameters (C# 9 records),
/// init-only properties, required members, and value-based equality.
/// </remarks>
public record LifecycleTrade
{
    public required Guid Id { get; init; }
    public required string Ticker { get; init; }
    public required Money Price { get; init; }
    public required int Quantity { get; init; }
    public required TradeType Type { get; init; }
    public required TradeState State { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ValidatedAt { get; init; }
    public DateTimeOffset? ExecutedAt { get; init; }
    public DateTimeOffset? SettledAt { get; init; }
    public string? RejectionReason { get; init; }
    public int Version { get; init; } = 1;

    /// <summary>Creates a new pending trade.</summary>
    public static LifecycleTrade Create(string ticker, Money price, int quantity, TradeType type) =>
        new()
        {
            Id = Guid.NewGuid(),
            Ticker = ticker,
            Price = price,
            Quantity = quantity,
            Type = type,
            State = TradeState.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };

    /// <summary>Notional value for risk classification.</summary>
    public decimal NotionalValue => Price.Amount * Quantity;
}

/// <summary>Represents one entry in the audit trail — a snapshot of the trade at a point in time.</summary>
public record AuditEntry(LifecycleTrade Trade, string Action, DateTimeOffset Timestamp);

/// <summary>
/// The immutable trade lifecycle engine. Uses pattern matching to
/// drive state transitions and with-expressions to create new
/// trade versions.
/// </summary>
public static class ImmutableDataPatternDemo
{
    /// <summary>
    /// Runs the complete Immutable Data Pattern capstone demonstration.
    /// </summary>
    public static void Run()
    {
        Console.WriteLine("═══════════════════════════════════════════════════");
        Console.WriteLine("  Theme 10 Capstone: Immutable Data Pattern");
        Console.WriteLine("  Combining: Records, with-expressions, patterns,");
        Console.WriteLine("  init-only, required members, collection expressions");
        Console.WriteLine("═══════════════════════════════════════════════════");
        Console.WriteLine();

        // ── Create initial trades ───────────────────────────────
        List<LifecycleTrade> trades =
        [
            LifecycleTrade.Create("AAPL", new Money(150.00m, Currency.USD), 100, TradeType.Spot),
            LifecycleTrade.Create("MEGACORP", new Money(500.00m, Currency.USD), 2000, TradeType.Swap),
            LifecycleTrade.Create("TSLA", new Money(250.00m, Currency.USD), 0, TradeType.Option),
            LifecycleTrade.Create("BOND-A", new Money(100.00m, Currency.EUR), 50, TradeType.Forward),
        ];

        Console.WriteLine("  Step 1: Created pending trades");
        PrintTrades(trades);

        // ── Validate trades (pattern matching drives decisions) ──
        Console.WriteLine("  Step 2: Validating trades...");
        var auditTrail = new List<AuditEntry>();
        var validated = new List<LifecycleTrade>();

        foreach (var trade in trades)
        {
            var (nextState, reason) = ValidateTrade(trade);
            var updated = trade with
            {
                State = nextState,
                ValidatedAt = DateTimeOffset.UtcNow,
                RejectionReason = reason,
                Version = trade.Version + 1
            };

            validated.Add(updated);
            auditTrail.Add(new AuditEntry(updated, $"Validation: {nextState}", DateTimeOffset.UtcNow));
        }

        PrintTrades(validated);

        // ── Execute valid trades ─────────────────────────────────
        Console.WriteLine("  Step 3: Executing validated trades...");
        var executed = validated.Select(t => t.State switch
            {
                TradeState.Validated => t with
                {
                    State = TradeState.Executed,
                    ExecutedAt = DateTimeOffset.UtcNow,
                    Version = t.Version + 1
                },
                _ => t // Rejected/cancelled trades pass through unchanged
            })
            .ToList();

        PrintTrades(executed);

        // ── Settle executed trades ───────────────────────────────
        Console.WriteLine("  Step 4: Settling executed trades...");
        var settled = executed
            .Select(t => t.State switch
            {
                TradeState.Executed => t with
                {
                    State = TradeState.Settled,
                    SettledAt = DateTimeOffset.UtcNow,
                    Version = t.Version + 1
                },
                _ => t
            })
            .ToList();

        PrintTrades(settled);

        // ── Audit trail ──────────────────────────────────────────
        Console.WriteLine("  Audit Trail (original trades preserved in memory):");
        Console.WriteLine($"    Original trades still at Version 1:");
        foreach (var trade in trades)
            Console.WriteLine($"      {trade.Ticker}: State={trade.State}, Version={trade.Version}");

        Console.WriteLine($"\n    Final trades at Version {settled.Max(t => t.Version)}:");
        foreach (var trade in settled)
            Console.WriteLine($"      {trade.Ticker}: State={trade.State}, Version={trade.Version}");

        // ── Demonstrate value equality across versions ───────────
        Console.WriteLine("\n  Value Equality Across Versions:");
        var v1 = trades[0];
        var v4 = settled[0];
        Console.WriteLine($"    v1 == v4: {v1 == v4} (different state → not equal)");

        var v1Copy = LifecycleTrade.Create("AAPL", new Money(150.00m, Currency.USD), 100, TradeType.Spot);
        Console.WriteLine($"    Two separate creates: Equal by value? {v1.Ticker == v1Copy.Ticker && v1.Price == v1Copy.Price}");
        Console.WriteLine($"    (Different Ids, so full record equality is false)");

        // ── PRODUCTION NOTES ─────────────────────────────────────
        // PRODUCTION NOTES:
        // 1. Immutable records are naturally thread-safe — no locks needed
        //    for read access. Multiple threads can process the same trade
        //    list without synchronization.
        //
        // 2. The with-expression creates shallow copies. For deep object
        //    graphs, ensure nested types are also immutable (like Money).
        //
        // 3. Audit trail: in production, persist each version to a database
        //    with an event-sourcing pattern. The Version field enables
        //    optimistic concurrency control.
        //
        // 4. Memory: each with-expression creates a new object. For bulk
        //    operations (millions of trades), consider using record structs
        //    or pooling strategies.
        //
        // 5. State machine: the pattern-matching validation could be
        //    enforced with a method that takes the current state and
        //    returns allowed transitions — making invalid states
        //    unrepresentable.

        Console.WriteLine();
        Console.WriteLine("───────────────────────────────────────────────────────");
    }

    /// <summary>
    /// Validates a trade using property patterns and relational patterns.
    /// Returns the next state and an optional rejection reason.
    /// </summary>
    private static (TradeState State, string? Reason) ValidateTrade(LifecycleTrade trade) => trade switch
    {
        // Invalid quantity
        { Quantity: <= 0 } =>
            (TradeState.Rejected, "Quantity must be positive"),

        // Swap without counterparty-like validation (simplified)
        { Type: TradeType.Swap, NotionalValue: > 500_000m } =>
            (TradeState.Rejected, "Swap notional exceeds limit without approval"),

        // Valid trades
        { Quantity: > 0, NotionalValue: < 1_000_000m } =>
            (TradeState.Validated, null),

        // Catch-all for very large trades
        _ => (TradeState.Rejected, "Exceeds automated validation limits")
    };

    private static void PrintTrades(List<LifecycleTrade> trades)
    {
        foreach (var t in trades)
        {
            var stateIcon = t.State switch
            {
                TradeState.Pending => "⏳",
                TradeState.Validated => "✅",
                TradeState.Executed => "📈",
                TradeState.Settled => "💰",
                TradeState.Rejected => "❌",
                TradeState.Cancelled => "🚫",
                _ => "❓"
            };
            var reason = t.RejectionReason is not null ? $" ({t.RejectionReason})" : "";
            Console.WriteLine($"    {stateIcon} {t.Ticker,-10} {t.State,-12} v{t.Version} " +
                              $"Notional: {t.NotionalValue:N2}{reason}");
        }
        Console.WriteLine();
    }
}
