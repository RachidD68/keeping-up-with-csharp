// ╔══════════════════════════════════════════════════════════╗
// ║  Theme 10 — Capstone: Source Generator Pattern          ║
// ║  "Combine source generators, module initializers,       ║
// ║   partial members, and caller info into a compile-time  ║
// ║   code generation pipeline"                             ║
// ╚══════════════════════════════════════════════════════════╝
//
// Features Combined:
// ┌─────────────────────────────┬──────────┬─────────────────┐
// │ Feature                     │ C# Ver.  │ Theme           │
// ├─────────────────────────────┼──────────┼─────────────────┤
// │ Source Generators            │ C# 9     │ Theme 9         │
// │ Module Initializers          │ C# 9     │ Theme 9         │
// │ Partial Members              │ C# 13/14 │ Theme 9         │
// │ Caller Info Attributes       │ C# 5     │ Theme 9         │
// │ Primary Constructors         │ C# 12    │ Theme 1         │
// └─────────────────────────────┴──────────┴─────────────────┘
//
// Scenario: A diagnostic framework where source generators
// produce logging code, module initializers register components,
// partial members bridge user code and generated code, and
// caller info captures context automatically.

namespace DevScripts.Theme10_Capstone;

/// <summary>
/// Capstone demonstration: a complete diagnostic framework
/// combining source generators, module initializers, partial
/// members, and caller information attributes.
/// </summary>
public static class SourceGeneratorPatternDemo
{
    public static void Run()
    {
        Console.WriteLine("═══════════════════════════════════════════════════");
        Console.WriteLine("  Theme 10 Capstone: Source Generator Pattern");
        Console.WriteLine("  Combining: source generators, module initializers,");
        Console.WriteLine("  partial members, caller info attributes");
        Console.WriteLine("═══════════════════════════════════════════════════");
        Console.WriteLine();

        // 1. Module initializer already ran (Theme 9)
        Console.WriteLine("  Step 1: Module Initializer");
        Console.WriteLine($"    Module initialized: {Theme9_CompilerTooling.ModuleInitializers.StartupHookDemo.IsInitialized}");
        Console.WriteLine("    ✓ Ran before any code — registered components automatically");
        Console.WriteLine();

        // 2. Caller info in diagnostic logging
        Console.WriteLine("  Step 2: Caller Information in Diagnostics");
        var diagnostics = new DiagnosticService("DevScripts");
        diagnostics.Info("Service started");
        diagnostics.Warn("Config file missing, using defaults");
        diagnostics.Error("Failed to connect to remote service");
        Console.WriteLine();

        // 3. Partial member pattern (simulated generator output)
        Console.WriteLine("  Step 3: Partial Members (Generated Code Bridge)");
        var config = new GeneratedConfig
        {
            DatabaseHost = "localhost",
            DatabasePort = 5432,
            EnableCaching = true
        };
        Console.WriteLine($"    Config: {config}");
        Console.WriteLine($"    Connection: {config.ConnectionString}");
        Console.WriteLine($"    IsValid: {config.IsValid}");
        Console.WriteLine();

        // 4. Source generator output (AutoToString)
        Console.WriteLine("  Step 4: Source Generator Output");
        var serverInfo = new Theme9_CompilerTooling.SourceGenerators.GeneratorConsumerDemo.ServerInfo
        {
            Host = "api.devscripts.io",
            Port = 443,
            UseTls = true
        };
        Console.WriteLine($"    Generated ToString: {serverInfo}");
        Console.WriteLine();

        // 5. The complete pipeline
        Console.WriteLine("  Complete Pipeline:");
        Console.WriteLine("""
                    1. [ModuleInitializer] → Registers components at load time
                    2. Source Generator    → Generates ToString, serializers at compile time
                    3. Partial Members     → Bridges user declarations ↔ generated implementations
                    4. Caller Info         → Zero-cost diagnostic context
                    5. Everything runs at compile time — zero reflection at runtime
            """);

        // PRODUCTION NOTES:
        // 1. Source generators + module initializers are how modern
        //    .NET libraries achieve zero-reflection startup:
        //    System.Text.Json, Microsoft.Extensions.Logging, and
        //    ASP.NET Minimal APIs all use this pattern.
        //
        // 2. The partial member contract lets library authors define
        //    "what users declare" while generators handle "how it
        //    works." This is a clean separation of concerns.
        //
        // 3. Caller info is injected at compile time — the generated
        //    code sees the ORIGINAL call site, not the generator's
        //    file. This is crucial for accurate diagnostics.
        //
        // 4. The combined pattern eliminates entire categories of
        //    runtime overhead: no reflection, no dynamic dispatch,
        //    no string parsing at runtime.

        Console.WriteLine();
        Console.WriteLine("───────────────────────────────────────────────────────");
    }

    /// <summary>
    /// Diagnostic service using caller info for automatic context capture.
    /// </summary>
    private sealed class DiagnosticService(string component)
    {
        public void Info(string message,
            [CallerMemberName] string caller = "",
            [CallerLineNumber] int line = 0) =>
            Console.WriteLine($"    ℹ [{component}] {caller}:{line} — {message}");

        public void Warn(string message,
            [CallerMemberName] string caller = "",
            [CallerLineNumber] int line = 0) =>
            Console.WriteLine($"    ⚠ [{component}] {caller}:{line} — {message}");

        public void Error(string message,
            [CallerMemberName] string caller = "",
            [CallerLineNumber] int line = 0) =>
            Console.WriteLine($"    ❌ [{component}] {caller}:{line} — {message}");
    }

    /// <summary>
    /// Partial class simulating a source-generator scenario.
    /// User declares properties; "generator" implements computed members.
    /// </summary>
    public partial class GeneratedConfig
    {
        // User-declared properties
        public string DatabaseHost { get; set; } = "localhost";
        public int DatabasePort { get; set; } = 5432;
        public bool EnableCaching { get; set; }

        // Partial property — declaration side
        public partial string ConnectionString { get; }
        public partial bool IsValid { get; }
    }

    // "Generated" implementation (simulates generator output)
    public partial class GeneratedConfig
    {
        public partial string ConnectionString =>
            $"Host={DatabaseHost};Port={DatabasePort};Caching={EnableCaching}";

        public partial bool IsValid =>
            !string.IsNullOrEmpty(DatabaseHost) && DatabasePort > 0;

        public override string ToString() =>
            $"GeneratedConfig {{ Host = {DatabaseHost}, Port = {DatabasePort}, Caching = {EnableCaching} }}";
    }
}
