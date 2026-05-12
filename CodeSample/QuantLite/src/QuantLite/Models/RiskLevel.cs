// ╔══════════════════════════════════════════════════════════╗
// ║  Model: RiskLevel                                       ║
// ║  Risk classification for trades and portfolios          ║
// ╚══════════════════════════════════════════════════════════╝

namespace QuantLite.Models;

/// <summary>
/// Risk classification levels used by the risk engine to
/// categorize trades and portfolio positions.
/// </summary>
public enum RiskLevel
{
    /// <summary>Low risk — small positions, stable instruments.</summary>
    Low,

    /// <summary>Medium risk — moderate exposure or volatility.</summary>
    Medium,

    /// <summary>High risk — large positions or volatile instruments.</summary>
    High,

    /// <summary>Critical risk — requires immediate attention.</summary>
    Critical
}
