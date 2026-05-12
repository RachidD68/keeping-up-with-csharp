// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: Interceptors                                   ║
// ║  Introduced: C# 12 (Experimental); Stable in C# 14      ║
// ║  Theme: 9 — Compiler & Tooling                          ║
// ╚══════════════════════════════════════════════════════════╝

namespace DevScripts.Theme9_CompilerTooling.Interceptors;

/// <summary>
/// Explains interceptors — a feature (stable since C# 14) that allows
/// source generators to redirect method calls at compile time.
/// This demo explains the concept with commentary and illustrative
/// examples showing how interceptors work.
/// </summary>
public static class InterceptorDemoClass
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — Why interceptors exist
    // ──────────────────────────────────────────────────────────

    public static void WhyInterceptors()
    {
        Console.WriteLine("  THE PROBLEM — Runtime dispatch overhead:");
        Console.WriteLine();
        Console.WriteLine("""
                // Consider a logging call:
                // logger.LogInformation("User {Name} logged in at {Time}", name, time);
                //
                // At runtime, this must:
                // 1. Parse the template string to find {Name} and {Time}
                // 2. Match them to the arguments
                // 3. Format the output
                //
                // All this parsing happens EVERY call, even though the
                // template never changes. An interceptor can pre-compute
                // the parsing at compile time.
            """);
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — How interceptors work
    // ──────────────────────────────────────────────────────────

    public static void HowInterceptorsWork()
    {
        Console.WriteLine("  HOW INTERCEPTORS WORK (C# 12 — Experimental):");
        Console.WriteLine();
        Console.WriteLine("""
                // A source generator can "intercept" a specific call site:
                //
                // Your code:
                //   logger.LogInformation("User {Name} logged in", name);
                //                         ↑ file: Program.cs, line: 42, column: 5
                //
                // Generator produces:
                //   [InterceptsLocation("Program.cs", 42, 5)]
                //   public static void LogInformation_Intercepted(
                //       this ILogger logger, string name)
                //   {
                //       // Pre-compiled template — no runtime parsing!
                //       logger.Write(LogLevel.Information,
                //           $"User {name} logged in");
                //   }
                //
                // At compile time, the call to LogInformation is redirected
                // to LogInformation_Intercepted — zero runtime cost.
            """);

        Console.WriteLine();
        Console.WriteLine("    Current status: EXPERIMENTAL");
        Console.WriteLine("    Requires: <InterceptorsPreviewNamespaces>");
        Console.WriteLine("    Used by: ASP.NET Minimal APIs, System.Text.Json");
        Console.WriteLine();

        // Demonstrate the concept with a simple example
        var result = SlowFormatMessage("Hello {0}, welcome to {1}!", "Alice", "DevScripts");
        Console.WriteLine($"    Runtime formatting: {result}");
        Console.WriteLine("    ↑ In production, an interceptor would pre-compile this template");

        Console.WriteLine();
        Console.WriteLine("    ✓ Redirects specific call sites at compile time");
        Console.WriteLine("    ✓ Enables pre-compilation of templates and patterns");
        Console.WriteLine("    ✓ Used internally by ASP.NET Core for Minimal APIs");
        Console.WriteLine("    ✓ Still experimental — API may change");
    }

    private static string SlowFormatMessage(string template, params object[] args) =>
        string.Format(template, args);

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // Interceptors complete the source generator story. Generators
    // can add new code, but they can't modify existing code.
    // Interceptors fill this gap — they redirect existing calls
    // to generated optimized versions. This is how ASP.NET Core's
    // Minimal API achieves near-zero overhead: the source generator
    // intercepts MapGet() calls and replaces them with specialized,
    // pre-compiled handlers.

    // GOING DEEPER:
    // Interceptors are controversial because they modify behavior
    // without changing source code — a method call does something
    // different from what the source suggests. This is why they're
    // restricted to source generators (not user code) and require
    // explicit opt-in via project properties. The team is carefully
    // evaluating whether to make them stable.

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: Read the ASP.NET Core Minimal API source to see how
    // interceptors are used in production. Search for
    // [InterceptsLocation] in the dotnet/aspnetcore repository.

    /// <summary>Runs the complete interceptors demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: Interceptors (C# 12 — Experimental)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        WhyInterceptors();
        HowInterceptorsWork();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
