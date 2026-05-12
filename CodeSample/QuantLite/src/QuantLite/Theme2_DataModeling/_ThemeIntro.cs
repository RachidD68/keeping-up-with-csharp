// ╔══════════════════════════════════════════════════════════╗
// ║  Theme 2 — Data Modeling Revolution (C# 7–11)           ║
// ║  "Your types should describe your domain, not your      ║
// ║   plumbing"                                             ║
// ╚══════════════════════════════════════════════════════════╝
//
// Theme 2 traces C#'s evolution from verbose class-based data
// modeling to the concise, immutable-by-default style enabled
// by tuples, records, init-only properties, and required members.
//
// In QuantLite, these features transform our trade domain from
// mutable classes with manual equality to immutable records with
// built-in value semantics — safer, shorter, and more expressive.
//
// Key insight: every feature in this theme moves C# closer to
// "pit of success" immutability — making the safe thing the easy
// thing to do.

namespace QuantLite.Theme2_DataModeling;

/// <summary>
/// Provides descriptions and runner methods for Theme 2: Data Modeling.
/// </summary>
public static class Theme2Intro
{
    /// <summary>The theme title for display.</summary>
    public const string Title = "Theme 2 — Data Modeling Revolution (C# 7–11)";

    /// <summary>The theme tagline.</summary>
    public const string Tagline = "Your types should describe your domain, not your plumbing";

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
