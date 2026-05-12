// Chapter 4 — Evolution of Control Flow — INTERMEDIATE
// ----------------------------------------------------------------
// Exercise: Write a CalculateDiscount(int quantity) method using
//   relational and logical patterns: quantity 1–99 → 0%, 100–499
//   → 5%, 500–999 → 10%, 1000+ → 15%. Include 0 and negative
//   values as an explicit guard that throws.
//
// Hint: Five arms: a guard for <= 0, and four range patterns using
//   and.
// ----------------------------------------------------------------

namespace KeepUpCs.Exercises.Ch04;

public static class Discount
{
    public static decimal CalculateDiscount(int quantity) => quantity switch
    {
        <= 0                          => throw new ArgumentOutOfRangeException(
                                                nameof(quantity),
                                                "Quantity must be positive."),
        >= 1    and <= 99             => 0.00m,
        >= 100  and <= 499            => 0.05m,
        >= 500  and <= 999            => 0.10m,
        >= 1000                       => 0.15m,
    };
}

public static class IntermediateDemo
{
    public static void Run()
    {
        Console.WriteLine("Ch04 Intermediate — CalculateDiscount with relational patterns");
        Console.WriteLine(new string('─', 60));

        int[] samples = [1, 50, 99, 100, 250, 499, 500, 750, 999, 1_000, 5_000];
        foreach (var q in samples)
        {
            var d = Discount.CalculateDiscount(q);
            Console.WriteLine($"  quantity = {q,5}  →  {d,5:P0} discount");
        }

        Console.WriteLine();
        try
        {
            Discount.CalculateDiscount(0);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Console.WriteLine($"  quantity = 0 throws: {ex.Message.Split('\n')[0]}");
        }

        try
        {
            Discount.CalculateDiscount(-3);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Console.WriteLine($"  quantity = -3 throws: {ex.Message.Split('\n')[0]}");
        }
    }
}
