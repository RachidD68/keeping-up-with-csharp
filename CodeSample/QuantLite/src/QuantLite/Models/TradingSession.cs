// ╔══════════════════════════════════════════════════════════╗
// ║  Model: TradingSession                                  ║
// ║  Stateful session that manages trades within a session  ║
// ╚══════════════════════════════════════════════════════════╝

namespace QuantLite.Models;

/// <summary>
/// Represents an active trading session. Manages the lifecycle of
/// trades from creation through execution, maintaining session state.
/// </summary>
public class TradingSession
{
    private readonly Portfolio _portfolio;
    private readonly List<string> _auditLog = [];
    private bool _isOpen;

    /// <summary>Creates a new trading session with the given name.</summary>
    /// <param name="sessionName">Identifier for this trading session.</param>
    public TradingSession(string sessionName)
    {
        ArgumentException.ThrowIfNullOrEmpty(sessionName);
        SessionName = sessionName;
        _portfolio = new Portfolio($"{sessionName}-Portfolio");
        _isOpen = true;
        _auditLog.Add($"Session '{sessionName}' opened at {DateTimeOffset.UtcNow:O}");
    }

    /// <summary>The session name.</summary>
    public string SessionName { get; }

    /// <summary>Whether this session is currently accepting trades.</summary>
    public bool IsOpen => _isOpen;

    /// <summary>The underlying portfolio for this session.</summary>
    public Portfolio Portfolio => _portfolio;

    /// <summary>Audit log of all session actions.</summary>
    public IReadOnlyList<string> AuditLog => _auditLog.AsReadOnly();

    /// <summary>
    /// Executes a trade within this session.
    /// </summary>
    /// <returns>The created trade record.</returns>
    /// <exception cref="InvalidOperationException">If the session is closed.</exception>
    public TradeRecord ExecuteTrade(
        string ticker,
        Money price,
        int quantity,
        TradeType type,
        string? counterparty = null)
    {
        if (!_isOpen)
            throw new InvalidOperationException(
                $"Session '{SessionName}' is closed. Cannot execute new trades.");

        ArgumentException.ThrowIfNullOrEmpty(ticker);

        var trade = TradeRecord.Create(ticker, price, quantity, type, counterparty);
        _portfolio.AddTrade(trade);
        _auditLog.Add($"Trade executed: {trade}");
        return trade;
    }

    /// <summary>
    /// Closes the session, preventing further trades.
    /// </summary>
    public void Close()
    {
        if (!_isOpen) return;
        _isOpen = false;
        _auditLog.Add($"Session '{SessionName}' closed at {DateTimeOffset.UtcNow:O}");
    }

    /// <inheritdoc />
    public override string ToString() =>
        $"Session '{SessionName}' [{(_isOpen ? "OPEN" : "CLOSED")}]: " +
        $"{_portfolio.TradeCount} trades";
}
