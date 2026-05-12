// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: Dynamic Binding                               ║
// ║  Introduced: C# 4.0                                     ║
// ║  Theme: 0 — The Foundation                              ║
// ╚══════════════════════════════════════════════════════════╝

namespace QuantLite.Theme0_Foundation.DynamicBinding;

/// <summary>
/// Demonstrates the <c>dynamic</c> keyword for flexible pricing
/// engines that dispatch at runtime rather than compile time.
/// </summary>
public static class DynamicDispatchDemo
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — How we did it before
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Before <c>dynamic</c>, working with objects whose types
    /// are only known at runtime required verbose reflection.
    /// </summary>
    public static void BeforeDynamic()
    {
        Console.WriteLine("  BEFORE (C# 3.0 — Reflection for runtime dispatch):");
        Console.WriteLine();

        object pricingEngine = CreatePricingEngine("spot");

        // Must use reflection to call methods on unknown types
        var method = pricingEngine.GetType().GetMethod("CalculatePrice");
        var result = method?.Invoke(pricingEngine, [100.0m, 50]);

        Console.WriteLine($"    Reflection result: {result}");
        Console.WriteLine("    ⚠ No IntelliSense, no compile-time checking");
        Console.WriteLine("    ⚠ Verbose: GetType().GetMethod().Invoke()");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — The modern way (C# 4.0)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// <c>dynamic</c> defers type resolution to runtime, providing
    /// clean syntax for late-bound calls.
    /// </summary>
    public static void WithDynamic()
    {
        Console.WriteLine("  AFTER (C# 4.0 — dynamic keyword):");
        Console.WriteLine();

        // dynamic defers all member resolution to runtime
        dynamic pricingEngine = CreatePricingEngine("spot");
        decimal price = pricingEngine.CalculatePrice(100.0m, 50);
        Console.WriteLine($"    Spot price: {price}");

        dynamic optionEngine = CreatePricingEngine("option");
        decimal optionPrice = optionEngine.CalculatePrice(100.0m, 50);
        Console.WriteLine($"    Option price: {optionPrice}");

        // Dynamic dispatch — method selection at runtime
        Console.WriteLine();
        Console.WriteLine("    Dynamic dispatch (visitor-like pattern):");
        object[] instruments = [new SpotInstrument(), new OptionInstrument()];
        foreach (dynamic instrument in instruments)
        {
            // The correct Describe overload is selected at runtime
            var description = Describe(instrument);
            Console.WriteLine($"      {description}");
        }

        Console.WriteLine();
        Console.WriteLine("    ✓ Clean syntax for late-bound calls");
        Console.WriteLine("    ✓ No reflection boilerplate");
        Console.WriteLine("    ✓ Runtime dispatch enables visitor-like patterns");
    }

    private static string Describe(SpotInstrument _) => "Spot: immediate delivery";
    private static string Describe(OptionInstrument _) => "Option: right to buy/sell";

    private static object CreatePricingEngine(string type) => type switch
    {
        "spot" => new SpotPricingEngine(),
        "option" => new OptionPricingEngine(),
        _ => throw new ArgumentException($"Unknown engine type: {type}")
    };

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // dynamic is a niche feature but invaluable for: COM interop,
    // JSON parsing (without deserialization), plugin architectures,
    // and the visitor pattern without double dispatch. It was
    // originally created for Office COM interop in C# 4.0.

    // TRADE-OFF:
    // dynamic bypasses all compile-time type checking. Typos in
    // method names become runtime exceptions. Use it sparingly
    // and only when you genuinely don't know the type at compile
    // time. For most late-binding scenarios, prefer:
    // - Interfaces (compile-time safety)
    // - Pattern matching (Theme 3)
    // - Source generators (Theme 9)

    // GOING DEEPER:
    // Under the hood, dynamic uses the DLR (Dynamic Language Runtime).
    // The first call to a dynamic member is slow (it compiles a call
    // site), but subsequent calls with the same types are cached and
    // nearly as fast as static dispatch.

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: Create a dynamic "configuration" object using
    // System.Dynamic.ExpandoObject and add properties at runtime:
    // config.MaxTrades = 100, config.Currency = "USD".

    private sealed class SpotInstrument;
    private sealed class OptionInstrument;

    private sealed class SpotPricingEngine
    {
        public decimal CalculatePrice(decimal basePrice, int quantity) =>
            basePrice * quantity;
    }

    private sealed class OptionPricingEngine
    {
        public decimal CalculatePrice(decimal basePrice, int quantity) =>
            basePrice * quantity * 0.1m; // simplified option premium
    }

    /// <summary>Runs the complete dynamic binding demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: Dynamic Binding (C# 4.0)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforeDynamic();
        WithDynamic();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
