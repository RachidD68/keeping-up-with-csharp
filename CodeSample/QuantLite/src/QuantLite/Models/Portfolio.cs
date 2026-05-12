// ╔══════════════════════════════════════════════════════════╗
// ║  Model: Portfolio                                       ║
// ║  A collection of trades with aggregation capabilities   ║
// ╚══════════════════════════════════════════════════════════╝

namespace QuantLite.Models;

/// <summary>
/// Manages a collection of trades and provides aggregation methods.
/// Unlike <see cref="TradeRecord"/>, this is a <c>class</c> because it
/// manages mutable state (adding/removing trades) and has reference
/// identity semantics.
/// </summary>
public class Portfolio
{
    private readonly List<TradeRecord> _trades = [];
    private readonly string _name;

    /// <summary>Creates a new portfolio with the given name.</summary>
    /// <param name="name">A descriptive name for this portfolio.</param>
    public Portfolio(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        _name = name;
    }

    /// <summary>The portfolio name.</summary>
    public string Name => _name;

    /// <summary>Read-only view of all trades in this portfolio.</summary>
    public IReadOnlyList<TradeRecord> Trades => _trades.AsReadOnly();

    /// <summary>The number of trades in this portfolio.</summary>
    public int TradeCount => _trades.Count;

    /// <summary>Adds a trade to the portfolio.</summary>
    public void AddTrade(TradeRecord trade)
    {
        ArgumentNullException.ThrowIfNull(trade);
        _trades.Add(trade);
    }

    /// <summary>Adds multiple trades to the portfolio.</summary>
    public void AddTrades(IEnumerable<TradeRecord> trades)
    {
        ArgumentNullException.ThrowIfNull(trades);
        _trades.AddRange(trades);
    }

    /// <summary>
    /// Calculates the total notional value in the specified currency.
    /// </summary>
    public Money TotalNotionalValue(Currency targetCurrency)
    {
        ArgumentNullException.ThrowIfNull(targetCurrency);

        return _trades
            .Select(t => t.NotionalValue.ConvertTo(targetCurrency))
            .Aggregate(Money.Zero(targetCurrency), (sum, m) => sum + m);
    }

    /// <summary>
    /// Groups trades by type and returns the count per type.
    /// </summary>
    public IReadOnlyDictionary<TradeType, int> TradesByType() =>
        _trades
            .GroupBy(t => t.Type)
            .ToDictionary(g => g.Key, g => g.Count());

    /// <summary>
    /// Returns trades matching the specified predicate.
    /// </summary>
    public IEnumerable<TradeRecord> Filter(Func<TradeRecord, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _trades.Where(predicate);
    }

    /// <inheritdoc />
    public override string ToString() =>
        $"Portfolio '{_name}': {_trades.Count} trades";
}
