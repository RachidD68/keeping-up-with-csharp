// ╔══════════════════════════════════════════════════════════╗
// ║  Model: TradeType                                       ║
// ║  Enumeration of supported trade instruments             ║
// ╚══════════════════════════════════════════════════════════╝

namespace QuantLite.Models;

/// <summary>
/// Classifies a trade by its financial instrument type.
/// Used throughout the application for pattern matching
/// and risk classification.
/// </summary>
public enum TradeType
{
    /// <summary>Immediate delivery trade at current market price.</summary>
    Spot,

    /// <summary>Agreement to trade at a future date at an agreed price.</summary>
    Forward,

    /// <summary>Contract giving the right (not obligation) to trade.</summary>
    Option,

    /// <summary>Exchange of cash flows between two parties.</summary>
    Swap
}
