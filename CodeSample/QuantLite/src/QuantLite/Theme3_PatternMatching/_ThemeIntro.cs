// ╔══════════════════════════════════════════════════════════╗
// ║  Theme 3 — Pattern Matching: The Swiss Army Knife       ║
// ║  (C# 7–11)                                             ║
// ║  "Replace chains of if-else with elegant, exhaustive    ║
// ║   pattern matching"                                     ║
// ╚══════════════════════════════════════════════════════════╝
//
// Pattern matching has evolved from simple type checks (C# 7)
// to a comprehensive pattern language (C# 11) that can match
// on types, properties, relationships, and sequences.
//
// In QuantLite, pattern matching transforms verbose risk
// classification logic from nested if/else chains into concise,
// exhaustive switch expressions — safer and more readable.
//
// Key insight: each new pattern type (property, relational,
// logical, list) adds another "dimension" of matching capability.
// Combined, they form an expressive DSL for decision logic.

namespace QuantLite.Theme3_PatternMatching;

/// <summary>
/// Provides descriptions and runner methods for Theme 3: Pattern Matching.
/// </summary>
public static class Theme3Intro
{
    /// <summary>The theme title for display.</summary>
    public const string Title = "Theme 3 — Pattern Matching: The Swiss Army Knife (C# 7–11)";

    /// <summary>The theme tagline.</summary>
    public const string Tagline = "Replace chains of if-else with elegant, exhaustive pattern matching";

    /// <summary>Prints the theme header to the console.</summary>
    public static void PrintHeader()
    {
        Console.WriteLine();
        Console.WriteLine("═══════════════════════════════════════════════════");
        Console.WriteLine($"  {Title}");
        Console.WriteLine($"  \"{Tagline}\"");
        Console.WriteLine("═══════════════════════════════════════════════════");
        Console.WriteLine();
    }
}
