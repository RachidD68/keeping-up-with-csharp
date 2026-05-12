namespace TypeForge.Theme4_TypeSystem;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: Generic Math (INumber<T> and friends)  (C# 11)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  PROBLEM
//  -------
//  Before C# 11, you couldn't write a generic Sum<T>()
//  method.  + was not available on unconstrained T.  You
//  had to write separate overloads for int, double, decimal,
//  etc. — or use dynamic (slow and unsafe).
//
//  SOLUTION
//  --------
//  .NET 7 introduced numeric interfaces: INumber<T>,
//  IAdditionOperators<T,T,T>, IComparisonOperators, etc.
//  Combined with static abstract members, generic code can
//  now perform arithmetic on any numeric type.
//
//  WHY IT MATTERS
//  ──────────────
//  Write once, work with int, double, decimal, Half, Int128,
//  BigInteger — even your own numeric types.  This is the
//  "generic math" revolution.
//
//  TRY IT
//  ──────
//  1. Write a generic Average<T>() method.
//  2. Create a generic Vector2D<T> with + and * operators.
//  3. Implement a generic Clamp<T>() using IComparable<T>.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Demonstrates generic math for type-safe numeric programming.
/// </summary>
public static class DimensionCalculatorDemo
{
    // ── Before: overloads for each numeric type ──────────────

    private static int SumInts(ReadOnlySpan<int> values)
    {
        int sum = 0;
        foreach (var v in values) sum += v;
        return sum;
    }

    private static double SumDoubles(ReadOnlySpan<double> values)
    {
        double sum = 0;
        foreach (var v in values) sum += v;
        return sum;
    }
    // And again for decimal, float, long, ...

    // ── After: one generic method for ALL numeric types ──────

    /// <summary>
    /// Generic sum — works with any INumber&lt;T&gt;.
    /// One method replaces N overloads.
    /// </summary>
    private static T Sum<T>(ReadOnlySpan<T> values) where T : INumber<T>
    {
        T sum = T.Zero; // T.Zero is a static abstract property!
        foreach (var v in values)
            sum += v; // += works because T : IAdditionOperators<T,T,T>
        return sum;
    }

    /// <summary>
    /// Generic average with proper type handling.
    /// </summary>
    private static T Average<T>(ReadOnlySpan<T> values) where T : INumber<T>
    {
        if (values.IsEmpty) return T.Zero;
        var sum = Sum(values);
        return sum / T.CreateChecked(values.Length);
    }

    /// <summary>
    /// Generic clamp — constrains a value to [min, max].
    /// </summary>
    private static T Clamp<T>(T value, T min, T max) where T : INumber<T> =>
        T.Clamp(value, min, max);

    /// <summary>
    /// Generic min/max finder.
    /// </summary>
    private static (T Min, T Max) MinMax<T>(ReadOnlySpan<T> values)
        where T : INumber<T>
    {
        if (values.IsEmpty) return (T.Zero, T.Zero);

        T min = values[0], max = values[0];
        foreach (var v in values[1..])
        {
            if (v < min) min = v;
            if (v > max) max = v;
        }
        return (min, max);
    }

    /// <summary>
    /// Demonstrates Dimension&lt;T&gt; arithmetic using generic math.
    /// </summary>
    private static Dimension<T> ScaleAndOffset<T>(
        Dimension<T> value, T scale, Dimension<T> offset)
        where T : INumber<T>
    {
        return (value * scale) + offset;
    }

    /// <summary>
    /// Demonstrates generic numeric conversion between types.
    /// </summary>
    private static TTo ConvertNumeric<TFrom, TTo>(TFrom value)
        where TFrom : INumber<TFrom>
        where TTo : INumber<TTo> =>
        TTo.CreateChecked(value);

    public static void Run()
    {
        Console.WriteLine("╔═══ Generic Math (INumber<T>) ═══╗\n");

        // ── 1. Generic Sum ───────────────────────────────────
        Console.WriteLine("── Generic Sum<T> ──");
        int[] ints = [1, 2, 3, 4, 5];
        double[] doubles = [1.5, 2.5, 3.5];
        decimal[] decimals = [100.10m, 200.20m, 300.30m];

        Console.WriteLine($"  Sum<int>:     {Sum<int>(ints)}");
        Console.WriteLine($"  Sum<double>:  {Sum<double>(doubles)}");
        Console.WriteLine($"  Sum<decimal>: {Sum<decimal>(decimals)}");

        // ── 2. Generic Average ───────────────────────────────
        Console.WriteLine("\n── Generic Average<T> ──");
        Console.WriteLine($"  Avg<int>:     {Average<int>(ints)}");
        Console.WriteLine($"  Avg<double>:  {Average<double>(doubles):F2}");
        Console.WriteLine($"  Avg<decimal>: {Average<decimal>(decimals)}");

        // ── 3. Generic Clamp ─────────────────────────────────
        Console.WriteLine("\n── Generic Clamp<T> ──");
        Console.WriteLine($"  Clamp(15, 0, 10):     {Clamp(15, 0, 10)}");
        Console.WriteLine($"  Clamp(-5.0, 0.0, 1.0): {Clamp(-5.0, 0.0, 1.0)}");
        Console.WriteLine($"  Clamp(50m, 0m, 100m):  {Clamp(50m, 0m, 100m)}");

        // ── 4. Generic MinMax ────────────────────────────────
        Console.WriteLine("\n── Generic MinMax<T> ──");
        var (minI, maxI) = MinMax<int>(ints);
        var (minD, maxD) = MinMax<double>(doubles);
        Console.WriteLine($"  int:    Min={minI}, Max={maxI}");
        Console.WriteLine($"  double: Min={minD}, Max={maxD}");

        // ── 5. Dimension<T> arithmetic ───────────────────────
        Console.WriteLine("\n── Dimension<T> with Generic Math ──");
        var length = new Dimension<double>(10.0, "m");
        var offset = new Dimension<double>(2.0, "m");
        var result = ScaleAndOffset(length, 3.0, offset);
        Console.WriteLine($"  {length} × 3 + {offset} = {result}");

        var intDim = new Dimension<int>(100, "px");
        var intOffset = new Dimension<int>(50, "px");
        var intResult = ScaleAndOffset(intDim, 2, intOffset);
        Console.WriteLine($"  {intDim} × 2 + {intOffset} = {intResult}");

        // ── 6. Numeric conversion ────────────────────────────
        Console.WriteLine("\n── Generic Numeric Conversion ──");
        int intVal = 42;
        double dblVal = ConvertNumeric<int, double>(intVal);
        decimal decVal = ConvertNumeric<int, decimal>(intVal);
        Console.WriteLine($"  int {intVal} → double {dblVal}");
        Console.WriteLine($"  int {intVal} → decimal {decVal}");

        // ── 7. T.Zero, T.One, T.CreateChecked ────────────────
        Console.WriteLine("\n── Static Abstract Members on INumber<T> ──");
        ShowNumericInfo<int>();
        ShowNumericInfo<double>();
        ShowNumericInfo<decimal>();

        static void ShowNumericInfo<T>() where T : INumber<T>
        {
            Console.WriteLine($"  {typeof(T).Name,-10} Zero={T.Zero} One={T.One}");
        }

        // ── 8. Key insight ───────────────────────────────────
        Console.WriteLine("\n── Key Insight ──");
        Console.WriteLine("  INumber<T> enables ONE generic method for ALL numeric types.");
        Console.WriteLine("  No more overloads. No more dynamic. No more code generation.");
        Console.WriteLine("  int, double, decimal, Half, Int128 — all just work.");
    }
}
