namespace TypeForge.Theme4_TypeSystem;

// ╔══════════════════════════════════════════════════════════════════╗
// ║  Theme 4 — Type System & OOP Flexibility                      ║
// ║                                                                ║
// ║  C# started as a classical OOP language, but each version      ║
// ║  has expanded how types can interact, compose, and constrain   ║
// ║  each other.  The type system is now one of the richest in     ║
// ║  any mainstream language:                                      ║
// ║                                                                ║
// ║  ● C# 8  — Default interface methods, nullable ref types      ║
// ║  ● C# 10 — Record structs, sealed ToString on records         ║
// ║  ● C# 11 — Static abstract/virtual members in interfaces      ║
// ║  ● C# 11 — Generic math (INumber<T>, IAdditionOperators)      ║
// ║  ● C# 11 — File-scoped types, ref fields                      ║
// ║  ● C# 12 — Inline arrays, collection expressions              ║
// ║  ● C# 13 — Partial properties, ref struct interfaces          ║
// ║  ● C# 14 — Extension members (extension everything)           ║
// ║                                                                ║
// ║  In TypeForge, every feature shapes, transforms, or validates  ║
// ║  types: shapes, plugins, type nodes, and dimensions.          ║
// ╚══════════════════════════════════════════════════════════════════╝

/// <summary>Theme 4 introduction and metadata.</summary>
public static class Theme4Intro
{
    public static string Title => "Theme 4 — Type System & OOP Flexibility";

    public static string Tagline =>
        "Forge types that are expressive, safe, and composable.";

    public static void PrintHeader()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(new string('═', 60));
        Console.WriteLine($"  {Title}");
        Console.WriteLine($"  {Tagline}");
        Console.WriteLine(new string('═', 60));
        Console.ResetColor();
    }
}
