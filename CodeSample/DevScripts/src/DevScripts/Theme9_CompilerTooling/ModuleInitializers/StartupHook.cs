// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: Module Initializers                           ║
// ║  Introduced: C# 9.0                                     ║
// ║  Theme: 9 — Compiler & Tooling                          ║
// ╚══════════════════════════════════════════════════════════╝

namespace DevScripts.Theme9_CompilerTooling.ModuleInitializers;

/// <summary>
/// Demonstrates module initializers — methods that run automatically
/// when an assembly is loaded, before any other code executes.
/// </summary>
public static class StartupHookDemo
{
    // Track whether our module initializer has run
    internal static bool IsInitialized { get; private set; }
    internal static DateTimeOffset InitializedAt { get; private set; }

    /// <summary>
    /// A module initializer — runs automatically when the assembly loads.
    /// No explicit call needed. The runtime invokes this before any
    /// other code in the assembly executes.
    /// </summary>
    [ModuleInitializer]
    internal static void Initialize()
    {
        IsInitialized = true;
        InitializedAt = DateTimeOffset.UtcNow;
        // In production: register codecs, configure defaults, set up telemetry
    }

    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — How we did it before (C# 8.0)
    // ──────────────────────────────────────────────────────────

    public static void BeforeModuleInitializers()
    {
        Console.WriteLine("  BEFORE (C# 8.0 — Static constructors or manual init):");
        Console.WriteLine();
        Console.WriteLine("""
                // Option A: Static constructor — only runs when the type is first used
                // static MyLibrary()
                // {
                //     RegisterCodecs();
                //     SetupDefaults();
                // }
                // ⚠ Problem: runs lazily, timing is unpredictable
                //
                // Option B: Explicit Init() call — requires user action
                // MyLibrary.Initialize();
                // ⚠ Problem: users forget to call it
            """);
        Console.WriteLine("    ⚠ No way to guarantee code runs at assembly load time");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — The modern way (C# 9.0)
    // ──────────────────────────────────────────────────────────

    public static void WithModuleInitializers()
    {
        Console.WriteLine("  AFTER (C# 9.0 — [ModuleInitializer]):");
        Console.WriteLine();

        Console.WriteLine($"    Module initialized: {IsInitialized}");
        Console.WriteLine($"    Initialized at: {InitializedAt:O}");
        Console.WriteLine("    ✓ Ran automatically — no explicit call needed!");
        Console.WriteLine();

        Console.WriteLine("""
                // [ModuleInitializer]
                // internal static void Initialize()
                // {
                //     // Runs before ANY code in this assembly executes
                //     RegisterJsonConverters();
                //     SetupDefaultConfiguration();
                //     InitializeTelemetry();
                // }
            """);

        Console.WriteLine();
        Console.WriteLine("    Requirements:");
        Console.WriteLine("      • Must be static, parameterless, void (or async void)");
        Console.WriteLine("      • Must be accessible (internal or public)");
        Console.WriteLine("      • Multiple [ModuleInitializer] methods allowed");
        Console.WriteLine("      • Execution order is unspecified between them");

        Console.WriteLine();
        Console.WriteLine("    ✓ Guaranteed to run before any assembly code");
        Console.WriteLine("    ✓ No user action required — truly automatic");
        Console.WriteLine("    ✓ Ideal for source generator registration");
    }

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // Module initializers solve the "library setup" problem.
    // Serialization libraries need to register converters, logging
    // libraries need to configure sinks, source generators need to
    // register generated code. Before module initializers, users
    // had to remember to call Init() — and they forgot. Now it's
    // automatic.

    // GOING DEEPER:
    // Module initializers are the C# equivalent of .NET's
    // ModuleInitializer attribute (which has existed in IL since
    // .NET 1.0 but was never exposed to C#). The compiler generates
    // a .cctor (module constructor) that calls your method.
    // Multiple [ModuleInitializer] methods create a single .cctor
    // that calls all of them in IL order (which correlates with
    // source file order, but this is not guaranteed).

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: Add a second [ModuleInitializer] method that registers
    // a custom JSON converter. Verify both initializers run by
    // checking a flag from each.

    /// <summary>Runs the complete module initializers demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: Module Initializers (C# 9.0)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforeModuleInitializers();
        WithModuleInitializers();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
