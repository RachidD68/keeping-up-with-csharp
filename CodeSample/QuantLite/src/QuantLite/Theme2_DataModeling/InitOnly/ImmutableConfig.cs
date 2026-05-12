// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: Init-Only Properties                          ║
// ║  Introduced: C# 9.0                                     ║
// ║  Theme: 2 — Data Modeling                               ║
// ╚══════════════════════════════════════════════════════════╝

namespace QuantLite.Theme2_DataModeling.InitOnly;

/// <summary>
/// Demonstrates init-only properties for immutable configuration
/// objects that are set once during initialization and never modified.
/// </summary>
public static class ImmutableConfigDemo
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — How we did it before (C# 8.0)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Before init-only, you had to choose between: mutable properties
    /// (settable anytime, unsafe) or constructor-only (can't use object
    /// initializer syntax).
    /// </summary>
    public static void BeforeInitOnly()
    {
        Console.WriteLine("  BEFORE (C# 8.0 — Mutable or constructor-only):");
        Console.WriteLine();

        // Option A: Mutable properties — convenient but unsafe
        var configA = new MutableConfig
        {
            MaxTradesPerSession = 1000,
            DefaultCurrency = "USD",
            RiskThreshold = 0.05m
        };
        configA.MaxTradesPerSession = 0; // BUG: mutated after creation!
        Console.WriteLine($"    Mutable config: MaxTrades changed to {configA.MaxTradesPerSession}");
        Console.WriteLine("    ⚠ Anyone can modify properties after construction");

        Console.WriteLine();

        // Option B: Constructor-only — safe but verbose
        var configB = new ImmutableViaCtorConfig(1000, "USD", 0.05m);
        // configB.MaxTradesPerSession = 0; // Won't compile ✓
        Console.WriteLine($"    Constructor config: MaxTrades={configB.MaxTradesPerSession}");
        Console.WriteLine("    ⚠ No object initializer syntax — positional args only");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — The modern way (C# 9.0)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Init-only properties give you the best of both worlds:
    /// object initializer syntax AND immutability after construction.
    /// </summary>
    public static void WithInitOnly()
    {
        Console.WriteLine("  AFTER (C# 9.0 — Init-only properties):");
        Console.WriteLine();

        // Object initializer syntax — readable, named properties
        var config = new TradingConfig
        {
            MaxTradesPerSession = 1000,
            DefaultCurrency = "USD",
            RiskThreshold = 0.05m,
            EnableAuditLog = true
        };

        Console.WriteLine($"    Created with object initializer:");
        Console.WriteLine($"      MaxTrades: {config.MaxTradesPerSession}");
        Console.WriteLine($"      Currency: {config.DefaultCurrency}");
        Console.WriteLine($"      Risk: {config.RiskThreshold}");
        Console.WriteLine($"      Audit: {config.EnableAuditLog}");

        // config.MaxTradesPerSession = 0; // ✅ Compile error: init-only!

        // Works with records too (positional record params are init-only)
        var riskConfig = new RiskConfig(0.05m, 0.10m, 0.25m);
        Console.WriteLine($"\n    Record with init-only: {riskConfig}");

        // with-expression still works — creates a NEW copy
        var relaxedConfig = riskConfig with { HighThreshold = 0.15m };
        Console.WriteLine($"    Modified copy: {relaxedConfig}");
        Console.WriteLine($"    Original unchanged: {riskConfig}");

        Console.WriteLine();
        Console.WriteLine("    ✓ Object initializer syntax for readability");
        Console.WriteLine("    ✓ Immutable after construction");
        Console.WriteLine("    ✓ Compatible with with-expressions");
    }

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // Init-only properties close the gap between "convenient to create"
    // and "safe to use." Configuration objects, DTOs, and value objects
    // should be immutable after construction. Init-only makes this the
    // natural path without sacrificing the readable object initializer
    // syntax that C# developers love.

    // GOING DEEPER:
    // The compiler emits an `init` accessor backed by a .initonly IL
    // flag. The CLR enforces this at the IL level — not just a compiler
    // trick. This means init-only is respected even by reflection
    // (with some caveats). The `init` accessor is actually a `set`
    // with a special `modreq(IsExternalInit)` modifier.

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: Create a MarketSessionConfig with init-only properties
    // for SessionStart (TimeOnly), SessionEnd (TimeOnly), and
    // TimeZone (string). Verify you can't modify them after creation.

    private sealed class MutableConfig
    {
        public int MaxTradesPerSession { get; set; }
        public string DefaultCurrency { get; set; } = "USD";
        public decimal RiskThreshold { get; set; }
    }

    private sealed class ImmutableViaCtorConfig
    {
        public int MaxTradesPerSession { get; }
        public string DefaultCurrency { get; }
        public decimal RiskThreshold { get; }

        public ImmutableViaCtorConfig(int maxTrades, string currency, decimal risk)
        {
            MaxTradesPerSession = maxTrades;
            DefaultCurrency = currency;
            RiskThreshold = risk;
        }
    }

    /// <summary>C# 9 init-only properties — settable in initializer only.</summary>
    private sealed class TradingConfig
    {
        public int MaxTradesPerSession { get; init; }
        public string DefaultCurrency { get; init; } = "USD";
        public decimal RiskThreshold { get; init; }
        public bool EnableAuditLog { get; init; }
    }

    /// <summary>Record with named parameters (all init-only by default).</summary>
    private record RiskConfig(decimal LowThreshold, decimal HighThreshold, decimal CriticalThreshold);

    /// <summary>Runs the complete init-only properties demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: Init-Only Properties (C# 9.0)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforeInitOnly();
        WithInitOnly();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
