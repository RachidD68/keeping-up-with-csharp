// ╔══════════════════════════════════════════════════════════╗
// ║  Theme 9 — Compiler & Tooling Innovations (C# 5–14)    ║
// ║  "The compiler is your most powerful collaborator"      ║
// ╚══════════════════════════════════════════════════════════╝
//
// Theme 9 explores features that blur the line between your code
// and the compiler's code: caller information attributes that
// inject debugging context, module initializers for library setup,
// source generators that write code at compile time, and
// interceptors that redirect method calls.
//
// These features share a common philosophy: the compiler knows
// things at compile time that are expensive to discover at runtime.
// Let it do the work for you.

namespace DevScripts.Theme9_CompilerTooling;

/// <summary>
/// Provides descriptions and runner methods for Theme 9: Compiler & Tooling.
/// </summary>
public static class Theme9Intro
{
    /// <summary>The theme title for display.</summary>
    public const string Title = "Theme 9 — Compiler & Tooling Innovations (C# 5–14)";

    /// <summary>The theme tagline.</summary>
    public const string Tagline = "The compiler is your most powerful collaborator";

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
