// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: Global & Implicit Usings                      ║
// ║  Introduced: C# 10 / .NET 6                            ║
// ║  Theme: 1 — Expressiveness & Boilerplate Reduction      ║
// ╚══════════════════════════════════════════════════════════╝

namespace DevScripts.Theme1_Expressiveness.GlobalUsings;

/// <summary>
/// Demonstrates global usings and implicit usings — eliminating
/// the repetitive using directives at the top of every file.
/// </summary>
public static class UsingsDemoClass
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — How we did it before (C# 9.0)
    // ──────────────────────────────────────────────────────────

    public static void BeforeGlobalUsings()
    {
        Console.WriteLine("  BEFORE (C# 9.0 — Repeated usings in every file):");
        Console.WriteLine();
        Console.WriteLine("""
                // File1.cs:
                // using System;
                // using System.Collections.Generic;
                // using System.Linq;
                // using System.Text;
                // using System.Threading.Tasks;
                //
                // File2.cs:
                // using System;              ← same 5 lines again!
                // using System.Collections.Generic;
                // using System.Linq;
                // ...
            """);
        Console.WriteLine("    ⚠ The same ~5 usings repeated in every file");
        Console.WriteLine("    ⚠ 100 files × 5 usings = 500 lines of boilerplate");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — The modern way (C# 10 / .NET 6+)
    // ──────────────────────────────────────────────────────────

    public static void WithGlobalUsings()
    {
        Console.WriteLine("  AFTER (C# 10 — Global & implicit usings):");
        Console.WriteLine();

        Console.WriteLine("    1. Implicit usings (<ImplicitUsings>enable</ImplicitUsings>):");
        Console.WriteLine("       .NET SDK auto-adds common usings based on project type:");
        Console.WriteLine("       • Console apps: System, System.Collections.Generic, System.IO,");
        Console.WriteLine("         System.Linq, System.Threading, System.Threading.Tasks");
        Console.WriteLine("       • ASP.NET: adds Microsoft.AspNetCore.* namespaces");
        Console.WriteLine();

        Console.WriteLine("    2. Global usings (global using directive):");
        Console.WriteLine("""
                // GlobalUsings.cs (one file for the whole project):
                // global using System.Text.Json;
                // global using DevScripts.Models;
                //
                // Now every file in the project has access to these
                // without any per-file using directives.
            """);

        Console.WriteLine();

        // Proof: we can use types without per-file usings
        // System.Text.Json is in our GlobalUsings.cs
        var json = JsonSerializer.Serialize(new { feature = "global usings", version = 10 });
        Console.WriteLine($"    Using JsonSerializer (from global using): {json}");

        // StringBuilder from System.Text — also global
        var sb = new StringBuilder("Built with global usings");
        Console.WriteLine($"    Using StringBuilder (from global using): {sb}");

        Console.WriteLine();
        Console.WriteLine("    ✓ Implicit usings: zero-config common namespaces");
        Console.WriteLine("    ✓ Global usings: declare once, available everywhere");
        Console.WriteLine("    ✓ Centralized in GlobalUsings.cs for discoverability");
    }

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // Using directives are necessary but never interesting. They're
    // the "boilerplate tax" you pay in every file. Global usings
    // pay that tax once per project. Combined with implicit usings,
    // most files need zero using directives — they just start with
    // the namespace and type declarations.

    // GOING DEEPER:
    // You can also add global usings in your .csproj:
    //   <Using Include="System.Text.Json" />
    // And remove implicit usings for specific namespaces:
    //   <Using Remove="System.Net.Http" />
    // This gives fine-grained control over what's globally available.
    // Convention: put global usings in a single GlobalUsings.cs file
    // at the project root for easy discovery.

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: Open the GlobalUsings.cs file in this project. Add
    // a global using for System.Diagnostics, then use Stopwatch
    // in any file without a per-file using directive.

    /// <summary>Runs the complete global usings demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: Global & Implicit Usings (C# 10)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforeGlobalUsings();
        WithGlobalUsings();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
