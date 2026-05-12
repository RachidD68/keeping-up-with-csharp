// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: Caller Information Attributes                 ║
// ║  Introduced: C# 5.0                                     ║
// ║  Theme: 9 — Compiler & Tooling                          ║
// ╚══════════════════════════════════════════════════════════╝

namespace DevScripts.Theme9_CompilerTooling.CallerInfo;

/// <summary>
/// Demonstrates <c>[CallerMemberName]</c>, <c>[CallerFilePath]</c>,
/// and <c>[CallerLineNumber]</c> for zero-effort diagnostic logging.
/// </summary>
public static class DiagnosticLoggerDemo
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — How we did it before
    // ──────────────────────────────────────────────────────────

    public static void BeforeCallerInfo()
    {
        Console.WriteLine("  BEFORE — Manual caller information:");
        Console.WriteLine();

        // Manually passing caller info — tedious and error-prone
        LogOld("Something happened", "BeforeCallerInfo", "DiagnosticLogger.cs", 28);
        Console.WriteLine("    ⚠ Must manually pass method name, file, and line");
        Console.WriteLine("    ⚠ Copy-paste errors when refactoring");
        Console.WriteLine();
    }

    private static void LogOld(string message, string member, string file, int line)
    {
        Console.WriteLine($"    [{Path.GetFileName(file)}:{line}] {member}: {message}");
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — The modern way (C# 5.0)
    // ──────────────────────────────────────────────────────────

    public static void WithCallerInfo()
    {
        Console.WriteLine("  AFTER (C# 5.0 — Caller information attributes):");
        Console.WriteLine();

        // Caller info injected automatically by the compiler
        Log("Application started");
        Log("Configuration loaded");
        SimulateWork();

        Console.WriteLine();

        // CallerArgumentExpression (C# 10) — captures the argument text
        // See also: ChatStream — Theme 5, Guard.cs for full demo
        var x = 42;
        ValidatePositive(x);

        Console.WriteLine();
        Console.WriteLine("    ✓ [CallerMemberName] — method name injected automatically");
        Console.WriteLine("    ✓ [CallerFilePath] — source file path");
        Console.WriteLine("    ✓ [CallerLineNumber] — line number");
        Console.WriteLine("    ✓ C# 10: [CallerArgumentExpression] — argument text");
    }

    private static void SimulateWork()
    {
        Log("Processing data...");
        Log("Work complete");
    }

    /// <summary>
    /// Logs with automatic caller information — no manual passing needed.
    /// The compiler fills in the default values at the call site.
    /// </summary>
    private static void Log(
        string message,
        [CallerMemberName] string member = "",
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
    {
        var fileName = Path.GetFileName(file);
        Console.WriteLine($"    [{fileName}:{line}] {member}: {message}");
    }

    /// <summary>
    /// CallerArgumentExpression (C# 10) captures the expression text.
    /// </summary>
    private static void ValidatePositive(
        int value,
        [CallerArgumentExpression(nameof(value))] string? expression = null)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(
                expression,
                $"Expected positive, got {value}");
        Console.WriteLine($"    Validate: {expression} = {value} ✓");
    }

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // Diagnostic information (who called what, from where) is
    // essential for debugging. Before caller info attributes,
    // you had two options: manual strings (error-prone) or
    // StackTrace (expensive at runtime). Caller info is injected
    // at compile time — zero runtime cost, always accurate.

    // GOING DEEPER:
    // Caller info attributes work by having the compiler replace
    // the default parameter value at the call site. This means:
    // 1. Zero runtime cost — it's string literals baked in
    // 2. Works across assemblies — the compiler at the call site
    //    fills in the values, not the callee's compiler
    // 3. CallerArgumentExpression (C# 10) captures the SOURCE TEXT
    //    of the argument — e.g., "items.Count" not "3"
    // See also: Theme 5 — CallerArgumentExpression (full guard
    // clause pattern in ChatStream).

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: Create a PerformanceTimer method that uses caller info
    // to log which method is being timed, without any parameters
    // from the caller.

    /// <summary>Runs the complete caller info demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: Caller Information Attributes (C# 5.0)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforeCallerInfo();
        WithCallerInfo();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
