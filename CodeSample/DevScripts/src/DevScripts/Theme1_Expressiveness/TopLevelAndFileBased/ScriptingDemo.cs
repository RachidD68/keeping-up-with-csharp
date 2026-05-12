// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: Top-Level Statements & File-Based Apps        ║
// ║  Introduced: C# 9.0 (top-level) / C# 14 (file-based)  ║
// ║  Theme: 1 — Expressiveness & Boilerplate Reduction      ║
// ╚══════════════════════════════════════════════════════════╝

namespace DevScripts.Theme1_Expressiveness.TopLevelAndFileBased;

/// <summary>
/// Demonstrates top-level statements (C# 9) and file-based apps
/// (C# 14) — the journey from 15 lines of ceremony to 1 line
/// of code.
/// </summary>
public static class ScriptingDemoClass
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — How we did it before (C# 8.0)
    // ──────────────────────────────────────────────────────────

    public static void BeforeTopLevel()
    {
        Console.WriteLine("  BEFORE (C# 8.0 — Full ceremony for a simple app):");
        Console.WriteLine();

        // Every C# program needed this scaffolding:
        Console.WriteLine("""
                // using System;                      ← required
                //
                // namespace MyApp                     ← required
                // {                                   ← required
                //     class Program                   ← required
                //     {                               ← required
                //         static void Main(string[] args) ← required
                //         {                           ← required
                //             Console.WriteLine("Hello");  ← your code
                //         }                           ← required
                //     }                               ← required
                // }                                   ← required
            """);
        Console.WriteLine("    ⚠ 10 lines of ceremony for 1 line of actual code");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — The modern way (C# 9/14)
    // ──────────────────────────────────────────────────────────

    public static void WithTopLevel()
    {
        Console.WriteLine("  AFTER (C# 9 → C# 14 — Minimal ceremony):");
        Console.WriteLine();

        Console.WriteLine("    C# 9 — Top-level statements:");
        Console.WriteLine("""
                // Program.cs:
                // Console.WriteLine("Hello");
                //
                // That's it. No namespace, no class, no Main method.
                // The compiler generates all the scaffolding for you.
            """);

        Console.WriteLine();
        Console.WriteLine("    C# 14 — File-based apps (dotnet run file.cs):");
        Console.WriteLine("""
                // hello.cs:
                // Console.WriteLine("Hello from a single file!");
                //
                // Run directly: dotnet run hello.cs
                // No .csproj needed. No project structure.
                // See: scripts/hello.cs in this project.
            """);

        Console.WriteLine();
        Console.WriteLine("    Top-level statements can still use:");
        Console.WriteLine("      • args — the command-line arguments");
        Console.WriteLine("      • await — top-level async");
        Console.WriteLine("      • return — exit code");
        Console.WriteLine("      • Local functions and types at the end of the file");

        Console.WriteLine();
        Console.WriteLine("    ✓ C# 9: One file, no ceremony — ideal for small programs");
        Console.WriteLine("    ✓ C# 14: No project needed — true scripting experience");
        Console.WriteLine("    ✓ Still full C# — not a subset or scripting language");
    }

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // Top-level statements made C# competitive with Python and
    // JavaScript for quick scripts. File-based apps (C# 14) take
    // it further — no .csproj, no solution, just a .cs file.
    // This lowers the barrier for beginners (no "why do I need
    // all this scaffolding?") and for experienced devs writing
    // quick utilities.

    // GOING DEEPER:
    // Top-level statements compile to a Program class with an
    // async Task<int> Main(string[] args) method. You can verify
    // this with typeof(Program) — it exists, even though you
    // didn't write it. Only one file per project can have top-level
    // statements (the compiler chooses it as the entry point).
    // File-based apps (C# 14) are even simpler — the compiler
    // creates a temporary project behind the scenes.

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: Try running the scripts/hello.cs file directly:
    //   dotnet run scripts/hello.cs
    // Then create your own file-based app that takes a filename
    // as an argument and counts its lines.

    /// <summary>Runs the complete top-level statements demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: Top-Level Statements / File-Based Apps (C# 9/14)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforeTopLevel();
        WithTopLevel();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
