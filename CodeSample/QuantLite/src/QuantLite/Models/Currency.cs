// ╔══════════════════════════════════════════════════════════╗
// ║  Model: Currency                                        ║
// ║  A record representing a currency with conversion rates ║
// ╚══════════════════════════════════════════════════════════╝

namespace QuantLite.Models;

/// <summary>
/// Represents a currency with its ISO code and conversion rate to USD.
/// Modeled as a <c>record</c> for value-based equality — two currencies
/// with the same code are considered equal regardless of reference identity.
/// </summary>
/// <param name="Code">ISO 4217 currency code (e.g., USD, EUR, GBP).</param>
/// <param name="Name">Human-readable currency name.</param>
/// <param name="ToUsdRate">Conversion rate: 1 unit of this currency = N USD.</param>
public record Currency(string Code, string Name, decimal ToUsdRate)
{
    // ──────────────────────────────────────────────────────────
    // Well-known currencies — static factory instances
    // ──────────────────────────────────────────────────────────

    /// <summary>United States Dollar.</summary>
    public static readonly Currency USD = new("USD", "US Dollar", 1.0m);

    /// <summary>Euro.</summary>
    public static readonly Currency EUR = new("EUR", "Euro", 1.08m);

    /// <summary>British Pound Sterling.</summary>
    public static readonly Currency GBP = new("GBP", "British Pound", 1.27m);

    /// <summary>Japanese Yen.</summary>
    public static readonly Currency JPY = new("JPY", "Japanese Yen", 0.0067m);

    /// <summary>
    /// Returns all well-known currencies for iteration and lookup.
    /// </summary>
    public static IReadOnlyList<Currency> All { get; } =
        [USD, EUR, GBP, JPY];

    /// <summary>
    /// Converts an amount in this currency to the target currency.
    /// </summary>
    public decimal ConvertTo(decimal amount, Currency target)
    {
        ArgumentNullException.ThrowIfNull(target);

        if (this == target) return amount;

        // Convert to USD first, then to target
        var usdAmount = amount * ToUsdRate;
        return usdAmount / target.ToUsdRate;
    }

    /// <inheritdoc />
    public override string ToString() => Code;
}
