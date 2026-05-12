// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: Digit Separators & Binary Literals            ║
// ║  Introduced: C# 7.0                                     ║
// ║  Theme: 1 — Expressiveness & Boilerplate Reduction      ║
// ╚══════════════════════════════════════════════════════════╝

namespace DevScripts.Theme1_Expressiveness.NumericLiterals;

/// <summary>
/// Demonstrates digit separators (<c>_</c>) and binary literals
/// (<c>0b</c>) for readable numeric constants in bitmask and
/// permission builders.
/// </summary>
public static class BitMaskBuilderDemo
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — How we did it before (C# 6.0)
    // ──────────────────────────────────────────────────────────

    public static void BeforeNumericLiterals()
    {
        Console.WriteLine("  BEFORE (C# 6.0 — Raw numeric literals):");
        Console.WriteLine();

        const int permissions = 0x1F; // What bits are set? Hard to see.
        const long budget = 1000000000; // How many zeros? Count carefully...
        const int flags = 255; // 0xFF? 0b11111111? Who knows from decimal.

        Console.WriteLine($"    Permissions: 0x1F = {permissions} (which bits?)");
        Console.WriteLine($"    Budget: {budget} (is that 1 billion?)");
        Console.WriteLine($"    Flags: {flags} (what's the bit pattern?)");
        Console.WriteLine("    ⚠ Hard to read large numbers and bit patterns");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — The modern way (C# 7.0)
    // ──────────────────────────────────────────────────────────

    public static void WithNumericLiterals()
    {
        Console.WriteLine("  AFTER (C# 7.0 — Digit separators & binary literals):");
        Console.WriteLine();

        // Digit separators — group digits for readability
        const long budget = 1_000_000_000; // Obviously 1 billion
        const double pi = 3.141_592_653_589;
        const int hexColor = 0xFF_80_20; // RGB components visible

        Console.WriteLine($"    Budget: {budget:N0} (1_000_000_000 — clearly 1 billion)");
        Console.WriteLine($"    Pi: {pi} (3.141_592_653_589)");
        Console.WriteLine($"    Color: #{hexColor:X6} (0xFF_80_20 — R,G,B groups visible)");
        Console.WriteLine();

        // Binary literals — essential for flags/permissions
        const int read = 0b0000_0001;
        const int write = 0b0000_0010;
        const int execute = 0b0000_0100;
        const int delete = 0b0000_1000;
        const int admin = 0b0001_0000;

        Console.WriteLine("    Permission flags (binary literals):");
        Console.WriteLine($"      Read:    0b{Convert.ToString(read, 2).PadLeft(8, '0')} = {read}");
        Console.WriteLine($"      Write:   0b{Convert.ToString(write, 2).PadLeft(8, '0')} = {write}");
        Console.WriteLine($"      Execute: 0b{Convert.ToString(execute, 2).PadLeft(8, '0')} = {execute}");
        Console.WriteLine($"      Delete:  0b{Convert.ToString(delete, 2).PadLeft(8, '0')} = {delete}");
        Console.WriteLine($"      Admin:   0b{Convert.ToString(admin, 2).PadLeft(8, '0')} = {admin}");

        // Combining flags
        var userPerms = read | write;
        var adminPerms = read | write | execute | delete | admin;
        Console.WriteLine($"\n    User (R|W):    0b{Convert.ToString(userPerms, 2).PadLeft(8, '0')} = {userPerms}");
        Console.WriteLine($"    Admin (all):   0b{Convert.ToString(adminPerms, 2).PadLeft(8, '0')} = {adminPerms}");

        Console.WriteLine();
        Console.WriteLine("    ✓ Digit separators: 1_000_000 is obviously 1 million");
        Console.WriteLine("    ✓ Binary literals: 0b0000_0001 — bit patterns are visible");
        Console.WriteLine("    ✓ Separators work in decimal, hex, and binary");
    }

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // Numeric literals are used in configuration, permissions,
    // protocol parsing, and hardware interfacing. Digit separators
    // prevent the "off by one zero" bug (is that 10 million or
    // 100 million?). Binary literals make bit manipulation code
    // self-documenting — the bit pattern IS the documentation.

    // GOING DEEPER:
    // Digit separators are purely cosmetic — they're stripped by
    // the compiler and have zero runtime impact. You can place
    // them anywhere within a number (not at start/end or adjacent
    // to the decimal point). Convention: group by 3 for decimal
    // (1_000_000), by 4 for binary (0b0000_0001), and by 2 for
    // hex (0xFF_80_20).

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: Create a [Flags] enum for FilePermissions using binary
    // literals: Read, Write, Execute, SetUid, SetGid, Sticky.

    /// <summary>Runs the complete numeric literals demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: Digit Separators & Binary Literals (C# 7.0)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforeNumericLiterals();
        WithNumericLiterals();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
