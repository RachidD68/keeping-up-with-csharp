// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: Constant Interpolated Strings                 ║
// ║  Introduced: C# 10                                      ║
// ║  Theme: 1 — Expressiveness & Boilerplate Reduction      ║
// ╚══════════════════════════════════════════════════════════╝

namespace DevScripts.Theme1_Expressiveness.ConstInterpolation;

/// <summary>
/// Demonstrates constant interpolated strings — <c>const string</c>
/// values that use string interpolation when all parts are constant.
/// </summary>
public static class ConstantStringsDemo
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — How we did it before (C# 9.0)
    // ──────────────────────────────────────────────────────────

    // Before: constants couldn't use interpolation
    private const string AppNameOld = "DevScripts";
    private const string VersionOld = "3.0";
    private const string FullNameOld = AppNameOld + " v" + VersionOld; // concatenation only

    public static void BeforeConstInterpolation()
    {
        Console.WriteLine("  BEFORE (C# 9.0 — String concatenation for const):");
        Console.WriteLine();
        Console.WriteLine($"    const string FullName = AppName + \" v\" + Version;");
        Console.WriteLine($"    Result: {FullNameOld}");
        Console.WriteLine("    ⚠ String concatenation less readable than interpolation");
        Console.WriteLine("    ⚠ $\"...\" was not allowed for const strings");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — The modern way (C# 10)
    // ──────────────────────────────────────────────────────────

    // After: interpolation works in const when all parts are const
    private const string AppName = "DevScripts";
    private const string Version = "3.0";
    private const string FullName = $"{AppName} v{Version}"; // ✅ C# 10!
    private const string LogPrefix = $"[{AppName}]";
    private const string ConfigKey = $"{AppName}.Settings.{Version}";

    public static void WithConstInterpolation()
    {
        Console.WriteLine("  AFTER (C# 10 — Constant interpolated strings):");
        Console.WriteLine();
        Console.WriteLine($"    const string FullName = $\"{{AppName}} v{{Version}}\";");
        Console.WriteLine($"    Result: {FullName}");
        Console.WriteLine($"    LogPrefix: {LogPrefix}");
        Console.WriteLine($"    ConfigKey: {ConfigKey}");
        Console.WriteLine();

        // Used in attributes (which require compile-time constants)
        Console.WriteLine("    Works in attributes (compile-time constants):");
        Console.WriteLine($"    [Display(Name = $\"{{AppName}} Dashboard\")]");

        // Used in switch cases
        var input = "DevScripts v3.0";
        var match = input switch
        {
            FullName => "Matched the const interpolated string!",
            _ => "No match"
        };
        Console.WriteLine($"    Switch case with const: {match}");

        Console.WriteLine();
        Console.WriteLine("    ✓ $\"{{const1}} {{const2}}\" works when all parts are const");
        Console.WriteLine("    ✓ More readable than string concatenation");
        Console.WriteLine("    ✓ Works in attributes, switch cases, and other const contexts");
    }

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // Const strings are used in attribute arguments, switch cases,
    // and default parameter values. Before C# 10, you had to use
    // concatenation in these contexts even though interpolation was
    // the natural choice everywhere else. Now interpolation works
    // consistently — const or not.

    // GOING DEEPER:
    // The restriction: all interpolation holes must contain other
    // const expressions. You can't use const string s = $"x={myVar}"
    // where myVar is a local — it must be another const. Format
    // specifiers like {value:F2} are NOT allowed because formatting
    // is a runtime operation.

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: Create a set of const strings for error message templates
    // using const interpolation. Use them in a switch expression.

    /// <summary>Runs the complete const interpolation demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: Constant Interpolated Strings (C# 10)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforeConstInterpolation();
        WithConstInterpolation();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
