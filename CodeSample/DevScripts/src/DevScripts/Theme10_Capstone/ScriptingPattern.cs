// ╔══════════════════════════════════════════════════════════╗
// ║  Theme 10 — Capstone: Scripting Pattern                 ║
// ║  "Combine top-level statements, global usings, primary  ║
// ║   constructors, and collection expressions into a       ║
// ║   minimal-ceremony developer utility"                   ║
// ╚══════════════════════════════════════════════════════════╝
//
// Features Combined:
// ┌─────────────────────────────┬──────────┬─────────────────┐
// │ Feature                     │ C# Ver.  │ Theme           │
// ├─────────────────────────────┼──────────┼─────────────────┤
// │ Top-Level Statements        │ C# 9     │ Theme 1         │
// │ File-Scoped Namespaces      │ C# 10    │ Theme 1         │
// │ Global Usings               │ C# 10    │ Theme 1         │
// │ Primary Constructors        │ C# 12    │ Theme 1         │
// │ Collection Expressions      │ C# 12    │ Theme 1         │
// │ Raw String Literals         │ C# 11    │ Theme 1         │
// │ Expression-Bodied Members   │ C# 6/7   │ Theme 1         │
// └─────────────────────────────┴──────────┴─────────────────┘
//
// Scenario: A source code analyzer that reads .cs files and
// produces a metrics report — demonstrating how modern C# makes
// utility scripts as concise as Python.

namespace DevScripts.Theme10_Capstone;

/// <summary>
/// Capstone demonstration: a complete source code analyzer built
/// with maximum boilerplate reduction — primary constructors,
/// collection expressions, raw strings, expression bodies.
/// </summary>
public static class ScriptingPatternDemo
{
    public static void Run()
    {
        Console.WriteLine("═══════════════════════════════════════════════════");
        Console.WriteLine("  Theme 10 Capstone: Scripting Pattern");
        Console.WriteLine("  Combining: top-level, primary ctors, collections,");
        Console.WriteLine("  raw strings, expression bodies, global usings");
        Console.WriteLine("═══════════════════════════════════════════════════");
        Console.WriteLine();

        // Simulate analyzing source files
        List<SourceFile> files =
        [
            new("Program.cs", GenerateSourceLines(45, 38, 3)),
            new("Models.cs", GenerateSourceLines(120, 95, 8)),
            new("Utils.cs", GenerateSourceLines(200, 160, 12)),
            new("Config.cs", GenerateSourceLines(30, 25, 2)),
        ];

        // Analyze with primary constructor types + collection expressions
        var analyzer = new CodeAnalyzer(files, minComplexity: 5);
        var report = analyzer.GenerateReport();

        Console.WriteLine(report);

        // Show the code that built this
        Console.WriteLine("  How this was built (zero boilerplate):");
        Console.WriteLine("""
                  • SourceFile: record with primary constructor (1 line)
                  • CodeAnalyzer: class with primary constructor (captures deps)
                  • Collection expressions: [new(...), new(...)] inline
                  • Raw string literal: report template with {{interpolation}}
                  • Expression bodies: one-liner methods throughout
                  • No explicit namespace braces (file-scoped)
                  • No repeated using directives (global usings)
            """);

        // PRODUCTION NOTES:
        // 1. File-based apps (C# 14) would let you run this as
        //    `dotnet run analyze.cs -- *.cs` with zero project setup.
        //
        // 2. Primary constructors + records reduce the "type tax" —
        //    defining small helper types is so cheap that you create
        //    them freely instead of using anonymous types or tuples.
        //
        // 3. Collection expressions with spread (..) make data
        //    pipeline composition clean: [..header, ..body, ..footer].
        //
        // 4. The entire analyzer is ~60 lines of actual logic.
        //    In C# 5, this would be ~150+ lines.

        Console.WriteLine();
        Console.WriteLine("───────────────────────────────────────────────────────");
    }

    /// <summary>Simulates source file content.</summary>
    private record SourceFile(string Name, string[] Lines);

    /// <summary>Analysis result for a single file.</summary>
    private record FileAnalysis(
        string Name,
        int TotalLines,
        int CodeLines,
        int BlankLines,
        int CommentLines,
        int Complexity);

    /// <summary>
    /// Analyzer using primary constructor — dependencies captured directly.
    /// </summary>
    private sealed class CodeAnalyzer(List<ScriptingPatternDemo.SourceFile> files, int minComplexity)
    {
        public string GenerateReport()
        {
            // Analyze all files
            List<FileAnalysis> analyses = [..files.Select(Analyze)];

            // Filter complex files
            var complexFiles = analyses
                .Where(a => a.Complexity >= minComplexity)
                .OrderByDescending(a => a.Complexity);

            // Build report with raw string literal
            var sb = new StringBuilder();
            sb.AppendLine($$"""
                  ┌──────────────────────────────────────────────┐
                  │  Source Code Analysis Report                 │
                  │  Files: {{analyses.Count}}, Threshold: complexity >= {{minComplexity}}  │
                  └──────────────────────────────────────────────┘
                """);

            sb.AppendLine("  All files:");
            foreach (var a in analyses)
                sb.AppendLine($"    {a.Name,-20} {a.TotalLines,5} total  {a.CodeLines,5} code  C={a.Complexity}");

            sb.AppendLine();
            sb.AppendLine($"  Complex files (>= {minComplexity}):");
            foreach (var a in complexFiles)
                sb.AppendLine($"    ⚠ {a.Name,-20} complexity={a.Complexity}");

            // Summary using collection expression aggregation
            int[] allComplexities = [..analyses.Select(a => a.Complexity)];
            var totalLines = analyses.Sum(a => a.TotalLines);
            var totalCode = analyses.Sum(a => a.CodeLines);
            var avgComplexity = allComplexities.Length > 0 ? allComplexities.Average() : 0;

            sb.AppendLine();
            sb.AppendLine($"  Summary: {totalLines} total lines, {totalCode} code lines");
            sb.AppendLine($"  Average complexity: {avgComplexity:F1}");

            return sb.ToString();
        }

        // Expression-bodied analyzer
        private static FileAnalysis Analyze(SourceFile file)
        {
            var total = file.Lines.Length;
            var blank = file.Lines.Count(l => string.IsNullOrWhiteSpace(l));
            var comments = file.Lines.Count(l => l.TrimStart().StartsWith("//"));
            var code = total - blank - comments;
            var complexity = file.Lines.Count(l =>
                l.Contains("if") || l.Contains("for") || l.Contains("while") ||
                l.Contains("switch") || l.Contains("catch"));

            return new FileAnalysis(file.Name, total, code, blank, comments, complexity);
        }
    }

    private static string[] GenerateSourceLines(int total, int code, int complexity)
    {
        var lines = new string[total];
        var rand = new Random(total); // deterministic seed
        for (var i = 0; i < total; i++)
        {
            if (i < complexity)
                lines[i] = "    if (condition) { /* logic */ }";
            else if (rand.NextDouble() < 0.1)
                lines[i] = "";
            else if (rand.NextDouble() < 0.05)
                lines[i] = "    // comment";
            else
                lines[i] = "    var x = SomeCode();";
        }
        return lines;
    }
}
