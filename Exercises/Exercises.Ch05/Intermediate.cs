// Chapter 5 — Type System & OOP Flexibility — INTERMEDIATE
// ----------------------------------------------------------------
// Exercise: Write a generic Median<T>(ReadOnlySpan<T> values)
//   method with where T : INumber<T>, IComparable<T>. Sort the
//   values, return the middle one for an odd count, and the
//   average of the two middle values for an even count. Test
//   with int, double, and decimal arrays.
//
// Hint: values.ToArray() gets you a mutable copy to sort; division
//   on generic T works because INumber<T> provides /.
// ----------------------------------------------------------------

using System.Numerics;

namespace KeepUpCs.Exercises.Ch05;

public static class Stats
{
    /// <summary>
    /// Returns the median of <paramref name="values"/>. For an even
    /// count, returns the average of the two middle elements.
    /// Throws if the span is empty.
    /// </summary>
    public static T Median<T>(ReadOnlySpan<T> values)
        where T : INumber<T>, IComparable<T>
    {
        if (values.IsEmpty)
            throw new ArgumentException("Cannot take the median of an empty span.", nameof(values));

        // Make a mutable copy and sort it.
        var sorted = values.ToArray();
        Array.Sort(sorted);

        int mid = sorted.Length / 2;
        if (sorted.Length % 2 == 1)
            return sorted[mid];

        // Even count — average the two middle elements.
        // INumber<T> guarantees both + and / for the generic T.
        T two = T.CreateChecked(2);
        return (sorted[mid - 1] + sorted[mid]) / two;
    }
}

public static class IntermediateDemo
{
    public static void Run()
    {
        Console.WriteLine("Ch05 Intermediate — Generic Median<T> with INumber<T>");
        Console.WriteLine(new string('─', 60));

        ReadOnlySpan<int>     ints     = [4, 1, 9, 2, 7];
        ReadOnlySpan<int>     evenInts = [1, 2, 3, 4];
        ReadOnlySpan<double>  doubles  = [1.5, 2.0, 0.5, 4.0];
        ReadOnlySpan<decimal> decs     = [10m, 20m, 30m, 40m, 50m];

        Console.WriteLine($"  median(int[5])     = {Stats.Median(ints)}");        // 4
        Console.WriteLine($"  median(int[4])     = {Stats.Median(evenInts)}");    // (2+3)/2 = 2 (integer div)
        Console.WriteLine($"  median(double[4])  = {Stats.Median(doubles)}");     // (1.5+2)/2 = 1.75
        Console.WriteLine($"  median(decimal[5]) = {Stats.Median(decs)}");        // 30
    }
}
