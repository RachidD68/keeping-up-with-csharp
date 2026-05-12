// ╔══════════════════════════════════════════════════════════╗
// ║  Model: MarketData                                      ║
// ║  Real-time market data snapshot for a ticker            ║
// ╚══════════════════════════════════════════════════════════╝

namespace QuantLite.Models;

/// <summary>
/// A snapshot of market data for a single financial instrument.
/// Immutable by design — each tick produces a new record.
/// </summary>
/// <param name="Ticker">The instrument ticker symbol.</param>
/// <param name="Bid">Current bid price.</param>
/// <param name="Ask">Current ask price.</param>
/// <param name="Volume">Trading volume in the current session.</param>
/// <param name="Timestamp">When this snapshot was captured.</param>
public record MarketData(
    string Ticker,
    decimal Bid,
    decimal Ask,
    long Volume,
    DateTimeOffset Timestamp)
{
    /// <summary>
    /// The bid-ask spread — a key liquidity indicator.
    /// </summary>
    public decimal Spread => Ask - Bid;

    /// <summary>
    /// The mid-price, commonly used as a reference price.
    /// </summary>
    public decimal MidPrice => (Bid + Ask) / 2m;

    /// <inheritdoc />
    public override string ToString() =>
        $"{Ticker}: Bid={Bid:F2} Ask={Ask:F2} Spread={Spread:F4} Vol={Volume:N0}";
}
