// ╔══════════════════════════════════════════════════════════╗
// ║  Model: Money                                           ║
// ║  A readonly record struct for amount + currency pairs   ║
// ╚══════════════════════════════════════════════════════════╝

namespace QuantLite.Models;

/// <summary>
/// Represents a monetary value with its currency. Implemented as a
/// <c>readonly record struct</c> for zero-allocation value semantics
/// and value-based equality.
/// </summary>
/// <remarks>
/// As a record struct, <see cref="Money"/> lives on the stack when used
/// as a local variable, avoiding GC pressure in hot paths like trade
/// calculations. The <c>readonly</c> modifier ensures no mutation after
/// construction — critical for financial data integrity.
/// </remarks>
/// <param name="Amount">The monetary amount.</param>
/// <param name="Currency">The currency of this money value.</param>
public readonly record struct Money(decimal Amount, Currency Currency)
{
    /// <summary>Creates a zero-value money in the specified currency.</summary>
    public static Money Zero(Currency currency) => new(0m, currency);

    /// <summary>
    /// Converts this money value to a different currency.
    /// </summary>
    public Money ConvertTo(Currency target)
    {
        ArgumentNullException.ThrowIfNull(target);
        var converted = Currency.ConvertTo(Amount, target);
        return new Money(converted, target);
    }

    // ──────────────────────────────────────────────────────────
    // Arithmetic operators — same currency only
    // ──────────────────────────────────────────────────────────

    public static Money operator +(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return new Money(left.Amount + right.Amount, left.Currency);
    }

    public static Money operator -(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return new Money(left.Amount - right.Amount, left.Currency);
    }

    public static Money operator *(Money money, decimal factor) =>
        new(money.Amount * factor, money.Currency);

    public static Money operator *(decimal factor, Money money) =>
        money * factor;

    public static Money operator /(Money money, decimal divisor) =>
        divisor == 0 ? throw new DivideByZeroException("Cannot divide money by zero.")
                     : new(money.Amount / divisor, money.Currency);

    // ──────────────────────────────────────────────────────────
    // Comparison operators
    // ──────────────────────────────────────────────────────────

    public static bool operator >(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return left.Amount > right.Amount;
    }

    public static bool operator <(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return left.Amount < right.Amount;
    }

    public static bool operator >=(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return left.Amount >= right.Amount;
    }

    public static bool operator <=(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return left.Amount <= right.Amount;
    }

    /// <inheritdoc />
    public override string ToString() => $"{Amount:N2} {Currency.Code}";

    private static void EnsureSameCurrency(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException(
                $"Cannot perform arithmetic on different currencies: " +
                $"{left.Currency.Code} and {right.Currency.Code}. " +
                $"Convert to a common currency first.");
    }
}
