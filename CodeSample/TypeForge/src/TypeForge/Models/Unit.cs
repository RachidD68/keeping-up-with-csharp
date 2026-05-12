namespace TypeForge.Models;

// ╔══════════════════════════════════════════════════════════╗
// ║  Unit — measurement unit conversions using static       ║
// ║  virtual members in interfaces (generic math).          ║
// ╚══════════════════════════════════════════════════════════╝

/// <summary>
/// A unit of measurement that can convert to/from a base unit.
/// Static abstract members (C# 11) enforce conversion factors.
/// </summary>
public interface IUnit<TSelf> where TSelf : IUnit<TSelf>
{
    /// <summary>The unit symbol (e.g., "m", "ft", "kg").</summary>
    static abstract string Symbol { get; }

    /// <summary>The full name (e.g., "Meters", "Feet", "Kilograms").</summary>
    static abstract string FullName { get; }

    /// <summary>Factor to multiply by to convert to the base unit.</summary>
    static abstract double ToBaseFactor { get; }
}

/// <summary>Length base unit: Meters.</summary>
public readonly struct Meters : IUnit<Meters>
{
    public static string Symbol => "m";
    public static string FullName => "Meters";
    public static double ToBaseFactor => 1.0;
}

/// <summary>Length unit: Feet.</summary>
public readonly struct Feet : IUnit<Feet>
{
    public static string Symbol => "ft";
    public static string FullName => "Feet";
    public static double ToBaseFactor => 0.3048;
}

/// <summary>Length unit: Inches.</summary>
public readonly struct Inches : IUnit<Inches>
{
    public static string Symbol => "in";
    public static string FullName => "Inches";
    public static double ToBaseFactor => 0.0254;
}

/// <summary>Weight base unit: Kilograms.</summary>
public readonly struct Kilograms : IUnit<Kilograms>
{
    public static string Symbol => "kg";
    public static string FullName => "Kilograms";
    public static double ToBaseFactor => 1.0;
}

/// <summary>Weight unit: Pounds.</summary>
public readonly struct Pounds : IUnit<Pounds>
{
    public static string Symbol => "lb";
    public static string FullName => "Pounds";
    public static double ToBaseFactor => 0.453592;
}

/// <summary>
/// Converts between units using static abstract interface members.
/// </summary>
public static class UnitConverter
{
    /// <summary>
    /// Converts a value from one unit to another.
    /// Both units must share the same base (e.g., both length, both weight).
    /// </summary>
    public static double Convert<TFrom, TTo>(double value)
        where TFrom : IUnit<TFrom>
        where TTo : IUnit<TTo>
    {
        var baseValue = value * TFrom.ToBaseFactor;
        return baseValue / TTo.ToBaseFactor;
    }

    /// <summary>Formats a conversion for display.</summary>
    public static string FormatConversion<TFrom, TTo>(double value)
        where TFrom : IUnit<TFrom>
        where TTo : IUnit<TTo>
    {
        var converted = Convert<TFrom, TTo>(value);
        return $"{value:F2} {TFrom.Symbol} = {converted:F2} {TTo.Symbol}";
    }
}
