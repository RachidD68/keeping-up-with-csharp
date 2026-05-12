// ╔══════════════════════════════════════════════════════════╗
// ║  Theme 0 — The Foundation (C# 2–4)                     ║
// ║  "Before we run, let's remember how we learned to walk" ║
// ╚══════════════════════════════════════════════════════════╝
//
// Theme 0 covers the foundational features that transformed C# from
// a simple object-oriented language into the expressive, multi-paradigm
// language we know today. These features — generics, LINQ, lambdas,
// extension methods, and more — are the bedrock upon which every
// modern C# feature is built.
//
// In QuantLite, we use these foundations to build a simplified
// financial calculator: generic repositories for trades, LINQ
// queries over portfolios, lambda predicates for filtering,
// and extension methods for fluent trade analysis.
//
// Even if you know these features well, pay attention to the
// GOING DEEPER comments — they reveal nuances that trip up
// even experienced developers.

namespace QuantLite.Theme0_Foundation;

/// <summary>
/// Provides descriptions and runner methods for Theme 0: The Foundation.
/// </summary>
public static class Theme0Intro
{
    /// <summary>The theme title for display.</summary>
    public const string Title = "Theme 0 — The Foundation (C# 2–4)";

    /// <summary>The theme tagline.</summary>
    public const string Tagline = "Before we run, let's remember how we learned to walk";

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
