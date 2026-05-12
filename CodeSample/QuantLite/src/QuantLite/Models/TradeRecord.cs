// ╔══════════════════════════════════════════════════════════╗
// ║  Model: TradeRecord                                     ║
// ║  The central domain type — an immutable trade record    ║
// ╚══════════════════════════════════════════════════════════╝

namespace QuantLite.Models;

/// <summary>
/// Represents a single trade. This is the central domain type in QuantLite.
/// Modeled as a <c>record</c> for value-based equality, immutability by
/// default, and built-in deconstruction — all key Theme 2 concepts.
/// </summary>
/// <param name="Id">Unique trade identifier.</param>
/// <param name="Ticker">The financial instrument ticker symbol.</param>
/// <param name="Price">The trade price as a <see cref="Money"/> value.</param>
/// <param name="Quantity">Number of units traded.</param>
/// <param name="Type">The type of trade instrument.</param>
/// <param name="Timestamp">When the trade was executed.</param>
/// <param name="Counterparty">Optional counterparty name.</param>
public record TradeRecord(
    Guid Id,
    string Ticker,
    Money Price,
    int Quantity,
    TradeType Type,
    DateTimeOffset Timestamp,
    string? Counterparty = null)
{
    /// <summary>
    /// Calculates the notional value (price × quantity) of this trade.
    /// </summary>
    public Money NotionalValue => Price * Quantity;

    /// <summary>
    /// Creates a new trade with a unique ID and current timestamp.
    /// </summary>
    public static TradeRecord Create(
        string ticker,
        Money price,
        int quantity,
        TradeType type,
        string? counterparty = null) =>
        new(
            Id: Guid.NewGuid(),
            Ticker: ticker,
            Price: price,
            Quantity: quantity,
            Type: type,
            Timestamp: DateTimeOffset.UtcNow,
            Counterparty: counterparty);

    /// <inheritdoc />
    public override string ToString() =>
        $"[{Type}] {Ticker}: {Quantity}x {Price} (Notional: {NotionalValue})";
}

/// <summary>
/// The result of evaluating a trade — PnL, risk, and fees.
/// </summary>
/// <param name="TradeId">The trade this result belongs to.</param>
/// <param name="PnL">Profit and loss value.</param>
/// <param name="Risk">Classified risk level.</param>
/// <param name="Fees">Calculated fees.</param>
public record TradeResult(
    Guid TradeId,
    Money PnL,
    RiskLevel Risk,
    Money Fees);
