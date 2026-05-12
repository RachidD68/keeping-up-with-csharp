// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: String Interpolation Evolution                ║
// ║  Introduced: C# 6.0 → C# 10 → C# 11                   ║
// ║  Theme: 1 — Expressiveness & Boilerplate Reduction      ║
// ╚══════════════════════════════════════════════════════════╝

namespace DevScripts.Theme1_Expressiveness.StringInterpolation;

/// <summary>
/// Demonstrates the evolution of string interpolation from C# 6
/// through C# 11: basic interpolation, raw string literals,
/// and interpolated string handlers.
/// </summary>
public static class TemplateEngineDemo
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — How we did it before (C# 5.0)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Before string interpolation, formatting strings required
    /// <c>string.Format</c> with positional placeholders — error-prone
    /// and hard to read.
    /// </summary>
    public static void BeforeInterpolation()
    {
        Console.WriteLine("  BEFORE (C# 5.0 — string.Format):");
        Console.WriteLine();

        var name = "DevScripts";
        var version = 3;
        var date = DateTime.Now;

        // Positional placeholders — which is {0}, {1}, {2}?
        var message = string.Format(
            "Project: {0}, Version: {1}, Built: {2:yyyy-MM-dd}", name, version, date);
        Console.WriteLine($"    {message}");
        Console.WriteLine("    ⚠ Positional {0}, {1} — easy to swap accidentally");

        // Multi-line strings — concatenation mess
        var config = "{\n" +
                     "  \"name\": \"" + name + "\",\n" +
                     "  \"version\": " + version + "\n" +
                     "}";
        Console.WriteLine($"    JSON config:\n    {config.Replace("\n", "\n    ")}");
        Console.WriteLine("    ⚠ Concatenation with \\n — unreadable");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — The modern way (C# 6.0 → 11)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// String interpolation evolved across multiple C# versions,
    /// each adding new capabilities.
    /// </summary>
    public static void WithInterpolation()
    {
        Console.WriteLine("  AFTER (C# 6–11 — String interpolation evolution):");
        Console.WriteLine();

        var name = "DevScripts";
        var version = 3;
        var date = DateTime.Now;

        // C# 6: Basic interpolation
        var basic = $"Project: {name}, Version: {version}, Built: {date:yyyy-MM-dd}";
        Console.WriteLine($"    C# 6 basic: {basic}");

        // C# 6: Expressions inside interpolation
        var expr = $"Next version: {version + 1}, Name length: {name.Length}";
        Console.WriteLine($"    C# 6 expressions: {expr}");

        // C# 10: Const interpolated strings (when all parts are const)
        // See also: ConstInterpolation demo
        Console.WriteLine($"    C# 10: const interpolation (see ConstantStrings.cs)");

        // C# 11: Raw string literals — no escaping needed
        var json = $$"""
            {
                "name": "{{name}}",
                "version": {{version}},
                "built": "{{date:yyyy-MM-dd}}"
            }
            """;
        Console.WriteLine($"    C# 11 raw string literal:");
        Console.WriteLine($"    {json.Replace("\n", "\n    ")}");

        Console.WriteLine();

        // C# 11: Raw strings with single $ — use {{ for braces
        var html = $"""
            <div class="project">
                <h1>{name}</h1>
                <span>v{version}</span>
            </div>
            """;
        Console.WriteLine($"    C# 11 raw HTML template:");
        Console.WriteLine($"    {html.Replace("\n", "\n    ")}");

        Console.WriteLine();

        // C# 11: Multi-dollar raw strings for JSON with real braces
        var jsonTemplate = $$"""
            {
                "project": "{{name}}",
                "config": {
                    "debug": true,
                    "version": {{version}}
                }
            }
            """;
        Console.WriteLine($"    C# 11 multi-dollar JSON (real braces preserved):");
        Console.WriteLine($"    {jsonTemplate.Replace("\n", "\n    ")}");

        Console.WriteLine();
        Console.WriteLine("    ✓ C# 6: $\"...{expr}...\" — readable inline expressions");
        Console.WriteLine("    ✓ C# 11: \"\"\"raw strings\"\"\" — no escaping, preserves formatting");
        Console.WriteLine("    ✓ C# 11: $$\"\"\"...{{expr}}...\"\"\" — raw + interpolation");
    }

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // String manipulation is arguably the most common operation in
    // programming. Raw string literals eliminated the biggest pain
    // point: embedding JSON, XML, HTML, SQL, or regex patterns that
    // are full of quotes and backslashes. No more escape character
    // archaeology.

    // GOING DEEPER:
    // C# 10 introduced interpolated string handlers — a low-level
    // feature that lets methods like StringBuilder.Append($"...")
    // avoid the allocation that $"..." normally creates. The
    // compiler generates code that writes directly to the builder.
    // This is how Debug.Assert($"...") became zero-cost when the
    // condition is true — the string is never even constructed.
    // See also: Theme 6 — Memory (allocation-free string handling).

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: Create a SQL query template using raw string literals
    // that includes real SQL brackets and interpolated table/column
    // names. Compare readability to the escaped version.

    /// <summary>Runs the complete string interpolation demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: String Interpolation Evolution (C# 6→11)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforeInterpolation();
        WithInterpolation();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
