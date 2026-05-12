namespace TypeForge.Theme4_TypeSystem;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: Operator Overloading & Conversions (evolved)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  PROBLEM
//  -------
//  Custom numeric types need arithmetic operators to feel
//  natural.  Without overloading, users write
//  `a.Add(b).Multiply(c)` instead of `a + b * c`.
//
//  SOLUTION
//  --------
//  C# allows operator overloading for +, -, *, /, ==, <, >,
//  implicit/explicit conversions, and more.  C# 11 added
//  checked operators.  C# 11's generic math interfaces make
//  your types work with generic numeric code.
//
//  WHY IT MATTERS
//  ──────────────
//  Domain types like Money, Dimension, and Angle become
//  first-class numeric citizens.  They work with +-*/,
//  comparison operators, and generic math algorithms.
//
//  TRY IT
//  ──────
//  1. Add implicit conversion from int to Dimension<int>.
//  2. Implement IComparisonOperators on Angle.
//  3. Make Percentage work with checked arithmetic.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Demonstrates operator overloading and conversions for domain types.
/// </summary>
public static class UnitArithmeticDemo
{
    /// <summary>
    /// An angle type with full operator support.
    /// Demonstrates: arithmetic, comparison, implicit/explicit conversion.
    /// </summary>
    private readonly record struct Angle :
        IComparable<Angle>,
        IFormattable
    {
        public double Degrees { get; }

        public Angle(double degrees) =>
            Degrees = Normalize(degrees);

        // ── Arithmetic operators ─────────────────────────────
        public static Angle operator +(Angle a, Angle b) => new(a.Degrees + b.Degrees);
        public static Angle operator -(Angle a, Angle b) => new(a.Degrees - b.Degrees);
        public static Angle operator *(Angle a, double scalar) => new(a.Degrees * scalar);
        public static Angle operator /(Angle a, double scalar) => new(a.Degrees / scalar);
        public static Angle operator -(Angle a) => new(-a.Degrees);

        // ── Comparison operators ─────────────────────────────
        public static bool operator <(Angle a, Angle b) => a.Degrees < b.Degrees;
        public static bool operator >(Angle a, Angle b) => a.Degrees > b.Degrees;
        public static bool operator <=(Angle a, Angle b) => a.Degrees <= b.Degrees;
        public static bool operator >=(Angle a, Angle b) => a.Degrees >= b.Degrees;

        // ── Implicit conversion from double ──────────────────
        // "Every double is a valid Angle" — no data loss.
        public static implicit operator Angle(double degrees) => new(degrees);

        // ── Explicit conversion to double ────────────────────
        // "Extracting degrees is intentional" — might lose context.
        public static explicit operator double(Angle angle) => angle.Degrees;

        // ── Conversion to radians ────────────────────────────
        public double Radians => Degrees * Math.PI / 180.0;

        // ── Factory methods ──────────────────────────────────
        public static Angle FromRadians(double radians) =>
            new(radians * 180.0 / Math.PI);

        public static readonly Angle Zero = new(0);
        public static readonly Angle Right = new(90);
        public static readonly Angle Straight = new(180);
        public static readonly Angle Full = new(360);

        public int CompareTo(Angle other) =>
            Degrees.CompareTo(other.Degrees);

        public string ToString(string? format, IFormatProvider? provider) =>
            $"{Degrees.ToString(format, provider)}°";

        public override string ToString() => $"{Degrees:F1}°";

        private static double Normalize(double degrees)
        {
            degrees %= 360;
            return degrees < 0 ? degrees + 360 : degrees;
        }
    }

    /// <summary>
    /// A percentage type with safe arithmetic.
    /// </summary>
    private readonly record struct Percentage
    {
        public double Value { get; }

        public Percentage(double value) =>
            Value = Math.Clamp(value, 0, 100);

        public static Percentage operator +(Percentage a, Percentage b) =>
            new(a.Value + b.Value); // Clamped by constructor

        public static Percentage operator -(Percentage a, Percentage b) =>
            new(a.Value - b.Value);

        /// <summary>
        /// Applies this percentage to a value.
        /// 50% of 200 = 100.
        /// </summary>
        public double Of(double value) => value * Value / 100.0;

        public static implicit operator Percentage(double value) => new(value);

        public override string ToString() => $"{Value:F1}%";
    }

    public static void Run()
    {
        Console.WriteLine("╔═══ Operator Overloading & Conversions ═══╗\n");

        // ── 1. Angle arithmetic ──────────────────────────────
        Console.WriteLine("── Angle Arithmetic ──");
        Angle a = 45;   // implicit conversion from double
        Angle b = 90;
        Console.WriteLine($"  a = {a}, b = {b}");
        Console.WriteLine($"  a + b     = {a + b}");
        Console.WriteLine($"  a - b     = {a - b}");
        Console.WriteLine($"  a * 3     = {a * 3}");
        Console.WriteLine($"  a / 2     = {a / 2}");
        Console.WriteLine($"  -a        = {-a}");

        // ── 2. Angle normalization ───────────────────────────
        Console.WriteLine("\n── Angle Normalization ──");
        Angle big = 450;
        Angle negative = -90;
        Console.WriteLine($"  450° normalizes to: {big}");
        Console.WriteLine($"  -90° normalizes to: {negative}");

        // ── 3. Angle comparison ──────────────────────────────
        Console.WriteLine("\n── Angle Comparison ──");
        Console.WriteLine($"  45° < 90°:  {(Angle)45 < (Angle)90}");
        Console.WriteLine($"  90° > 180°: {(Angle)90 > (Angle)180}");
        Console.WriteLine($"  90° == 90°: {(Angle)90 == (Angle)90}");

        // ── 4. Implicit and explicit conversions ─────────────
        Console.WriteLine("\n── Conversions ──");
        Angle fromDouble = 30.0;  // Implicit: double → Angle
        double toDouble = (double)fromDouble;  // Explicit: Angle → double
        Console.WriteLine($"  30.0 → Angle: {fromDouble}");
        Console.WriteLine($"  Angle → double: {toDouble}");
        Console.WriteLine($"  Radians: {fromDouble.Radians:F4}");
        Console.WriteLine($"  FromRadians(π/4): {Angle.FromRadians(Math.PI / 4)}");

        // ── 5. Named constants ───────────────────────────────
        Console.WriteLine("\n── Named Constants ──");
        Console.WriteLine($"  Zero:     {Angle.Zero}");
        Console.WriteLine($"  Right:    {Angle.Right}");
        Console.WriteLine($"  Straight: {Angle.Straight}");
        Console.WriteLine($"  Full:     {Angle.Full}");

        // ── 6. Percentage type ───────────────────────────────
        Console.WriteLine("\n── Percentage Type ──");
        Percentage p1 = 25;
        Percentage p2 = 50;
        Console.WriteLine($"  {p1} + {p2} = {p1 + p2}");
        Console.WriteLine($"  {p1} of 200 = {p1.Of(200)}");
        Console.WriteLine($"  {p2} of 300 = {p2.Of(300)}");

        Percentage clamped = 150;  // Clamped to 100%
        Console.WriteLine($"  150 → {clamped} (clamped)");

        // ── 7. Dimension<T> operators ────────────────────────
        Console.WriteLine("\n── Dimension<T> Operators ──");
        var d1 = new Dimension<double>(10.0, "m");
        var d2 = new Dimension<double>(5.0, "m");
        Console.WriteLine($"  {d1} + {d2} = {d1 + d2}");
        Console.WriteLine($"  {d1} - {d2} = {d1 - d2}");
        Console.WriteLine($"  {d1} * 3 = {d1 * 3.0}");
        Console.WriteLine($"  {d1} / 2 = {d1 / 2.0}");

        try
        {
            var bad = new Dimension<double>(1.0, "m") + new Dimension<double>(1.0, "ft");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"  Mixed units: {ex.Message}");
        }

        // ── 8. Unit conversions ──────────────────────────────
        Console.WriteLine("\n── Unit Conversions ──");
        Console.WriteLine($"  {UnitConverter.FormatConversion<Meters, Feet>(1.0)}");
        Console.WriteLine($"  {UnitConverter.FormatConversion<Feet, Meters>(3.28)}");
        Console.WriteLine($"  {UnitConverter.FormatConversion<Kilograms, Pounds>(1.0)}");
    }
}
