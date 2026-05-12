// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: Source Generators                             ║
// ║  Introduced: C# 9.0 / .NET 5                           ║
// ║  Theme: 9 — Compiler & Tooling                          ║
// ╚══════════════════════════════════════════════════════════╝

namespace DevScripts.Theme9_CompilerTooling.SourceGenerators;

/// <summary>
/// Demonstrates consuming a source generator — the AutoToString
/// generator defined in DevScripts.Generators automatically
/// creates ToString() methods for decorated classes.
/// </summary>
public static partial class GeneratorConsumerDemo
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — How we did it before
    // ──────────────────────────────────────────────────────────

    public static void BeforeSourceGenerators()
    {
        Console.WriteLine("  BEFORE — Manual ToString() or runtime reflection:");
        Console.WriteLine();

        var entry = new ManualLogEntry
        {
            Timestamp = DateTimeOffset.UtcNow,
            Level = "Info",
            Message = "Application started",
            Source = "Main"
        };

        Console.WriteLine($"    Manual ToString: {entry}");
        Console.WriteLine("    ⚠ Must write and maintain ToString for every class");
        Console.WriteLine("    ⚠ Reflection-based alternatives are slow (runtime cost)");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — The modern way (Source Generators)
    // ──────────────────────────────────────────────────────────

    public static void WithSourceGenerators()
    {
        Console.WriteLine("  AFTER (C# 9+ — Source Generators):");
        Console.WriteLine();

        Console.WriteLine("    Source generators write code AT COMPILE TIME:");
        Console.WriteLine("""
                // 1. You write:
                //    [AutoToString]
                //    public partial class ServerInfo
                //    {
                //        public string Host { get; set; }
                //        public int Port { get; set; }
                //        public bool UseTls { get; set; }
                //    }
                //
                // 2. The generator produces (at compile time):
                //    partial class ServerInfo
                //    {
                //        public override string ToString() =>
                //            $"ServerInfo {{ Host = {Host}, Port = {Port}, UseTls = {UseTls} }}";
                //    }
                //
                // 3. You use it normally:
                //    var server = new ServerInfo { Host = "api.example.com", Port = 443, UseTls = true };
                //    Console.WriteLine(server); // Generated ToString!
            """);

        Console.WriteLine();

        // Demonstrate with our actual decorated type
        var info = new ServerInfo { Host = "api.example.com", Port = 443, UseTls = true };
        Console.WriteLine($"    ServerInfo: {info}");

        Console.WriteLine();
        Console.WriteLine("    Key points:");
        Console.WriteLine("      • Generator project targets netstandard2.0");
        Console.WriteLine("      • Implements IIncrementalGenerator (not ISourceGenerator)");
        Console.WriteLine("      • Referenced as analyzer: OutputItemType=\"Analyzer\"");
        Console.WriteLine("      • Generated code is visible in IDE (navigate to definition)");
        Console.WriteLine("      • Zero runtime reflection — all work done at compile time");

        Console.WriteLine();
        Console.WriteLine("    ✓ Code generated at compile time — zero runtime cost");
        Console.WriteLine("    ✓ IntelliSense-aware — generated members are visible in IDE");
        Console.WriteLine("    ✓ Type-safe — compiler validates generated code");
        Console.WriteLine("    ✓ Replaces runtime reflection patterns with compile-time code");
    }

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // Source generators eliminate entire categories of runtime
    // reflection. JSON serialization, logging, validation, mapping —
    // all traditionally done via reflection — can now be generated
    // at compile time with zero runtime overhead. System.Text.Json,
    // Logging, and many libraries now use source generators.

    // GOING DEEPER:
    // Incremental generators (IIncrementalGenerator) replaced the
    // original ISourceGenerator API in .NET 6. The incremental API
    // uses a pipeline model that caches intermediate results,
    // making the generator fast enough for real-time IDE use.
    // The pipeline: RegisterPostInitializationOutput (attributes),
    // CreateSyntaxProvider (find candidates), Combine + Register
    // (generate code).
    // See also: DevScripts.Generators/AutoToStringGenerator.cs
    // for the full generator implementation.

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: Add a new class decorated with [AutoToString] and
    // verify the generator produces a ToString() method.
    // Then look at the generated code in:
    //   obj/Debug/net10.0/generated/DevScripts.Generators/...

    /// <summary>Manual ToString implementation (the "before").</summary>
    private sealed class ManualLogEntry
    {
        public DateTimeOffset Timestamp { get; set; }
        public string Level { get; set; } = "";
        public string Message { get; set; } = "";
        public string Source { get; set; } = "";

        // Must write and maintain manually
        public override string ToString() =>
            $"LogEntry {{ Timestamp = {Timestamp:O}, Level = {Level}, Message = {Message}, Source = {Source} }}";
    }

    /// <summary>
    /// Decorated with [AutoToString] — the source generator creates
    /// the ToString() method at compile time. Must be partial.
    /// </summary>
    [AutoToString]
    public partial class ServerInfo
    {
        public string Host { get; set; } = "";
        public int Port { get; set; }
        public bool UseTls { get; set; }

        // If generator is not running, provide a fallback ToString
        // The generator's partial method will override this only if generated
    }

    /// <summary>Runs the complete source generators demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: Source Generators (C# 9.0)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforeSourceGenerators();
        WithSourceGenerators();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
