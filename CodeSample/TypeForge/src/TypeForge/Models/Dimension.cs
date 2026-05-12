namespace TypeForge.Models;

// ╔══════════════════════════════════════════════════════════╗
// ║  Dimension — strongly-typed dimensions with generic     ║
// ║  math, demonstrating INumber<T> and operator overloads. ║
// ╚══════════════════════════════════════════════════════════╝

/// <summary>
/// A strongly-typed measurement with a numeric value.
/// Uses generic math constraints (C# 11) to work with any numeric type.
/// </summary>
public readonly record struct Dimension<T>(T Value, string Unit)
    where T : INumber<T>
{
    public static Dimension<T> Zero(string unit) => new(T.Zero, unit);

    public static Dimension<T> operator +(Dimension<T> a, Dimension<T> b)
    {
        EnsureSameUnit(a, b);
        return new(a.Value + b.Value, a.Unit);
    }

    public static Dimension<T> operator -(Dimension<T> a, Dimension<T> b)
    {
        EnsureSameUnit(a, b);
        return new(a.Value - b.Value, a.Unit);
    }

    public static Dimension<T> operator *(Dimension<T> a, T scalar) =>
        new(a.Value * scalar, a.Unit);

    public static Dimension<T> operator /(Dimension<T> a, T scalar) =>
        new(a.Value / scalar, a.Unit);

    private static void EnsureSameUnit(Dimension<T> a, Dimension<T> b)
    {
        if (a.Unit != b.Unit)
            throw new InvalidOperationException(
                $"Cannot combine dimensions with different units: '{a.Unit}' and '{b.Unit}'.");
    }

    public override string ToString() => $"{Value} {Unit}";
}
