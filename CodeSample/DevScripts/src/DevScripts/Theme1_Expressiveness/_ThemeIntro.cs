// ╔══════════════════════════════════════════════════════════╗
// ║  Theme 1 — Expressiveness & Boilerplate Reduction       ║
// ║  (C# 6–14)                                             ║
// ║  "Write less ceremony, express more intent"             ║
// ╚══════════════════════════════════════════════════════════╝
//
// Theme 1 is the widest theme in the book — 16 features spanning
// nine C# versions. It traces the relentless war on boilerplate:
// from string interpolation (C# 6) to the field keyword (C# 14).
//
// Each feature removes a few lines of ceremony, and together they
// transform verbose, noisy code into clean, intent-revealing
// expressions. In DevScripts, we apply these features to developer
// utilities: template engines, code analyzers, configuration
// builders, and data pipelines.
//
// Key insight: most of these features are syntactic sugar — they
// don't add new capabilities, they make existing capabilities
// easier to express. But syntax matters enormously: it determines
// what patterns developers reach for naturally.

namespace DevScripts.Theme1_Expressiveness;

/// <summary>
/// Provides descriptions and runner methods for Theme 1: Expressiveness.
/// </summary>
public static class Theme1Intro
{
    /// <summary>The theme title for display.</summary>
    public const string Title = "Theme 1 — Expressiveness & Boilerplate Reduction (C# 6–14)";

    /// <summary>The theme tagline.</summary>
    public const string Tagline = "Write less ceremony, express more intent";

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
