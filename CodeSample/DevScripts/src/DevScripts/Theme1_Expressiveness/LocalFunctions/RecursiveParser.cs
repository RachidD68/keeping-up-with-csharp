// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: Local Functions & Static Local Functions      ║
// ║  Introduced: C# 7.0 (local) / C# 8.0 (static local)   ║
// ║  Theme: 1 — Expressiveness & Boilerplate Reduction      ║
// ╚══════════════════════════════════════════════════════════╝

namespace DevScripts.Theme1_Expressiveness.LocalFunctions;

/// <summary>
/// Demonstrates local functions and static local functions for
/// encapsulating helper logic within a method — particularly
/// useful for recursive parsing and validation.
/// </summary>
public static class RecursiveParserDemo
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — How we did it before (C# 6.0)
    // ──────────────────────────────────────────────────────────

    public static void BeforeLocalFunctions()
    {
        Console.WriteLine("  BEFORE (C# 6.0 — Private helper methods or lambdas):");
        Console.WriteLine();

        var json = """{"name": "DevScripts", "version": 3}""";
        var depth = CountNestingDepth_Old(json);
        Console.WriteLine($"    Nesting depth of '{json}': {depth}");
        Console.WriteLine("    ⚠ Helper method pollutes the class scope");
        Console.WriteLine("    ⚠ Lambda alternative: no recursion, delegate allocation");
        Console.WriteLine();
    }

    // Old approach: private method that belongs to the class
    private static int CountNestingDepth_Old(string text)
    {
        var maxDepth = 0;
        var currentDepth = 0;
        foreach (var ch in text)
        {
            if (ch is '{' or '[') { currentDepth++; maxDepth = Math.Max(maxDepth, currentDepth); }
            else if (ch is '}' or ']') currentDepth--;
        }
        return maxDepth;
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — The modern way (C# 7.0/8.0)
    // ──────────────────────────────────────────────────────────

    public static void WithLocalFunctions()
    {
        Console.WriteLine("  AFTER (C# 7.0/8.0 — Local & static local functions):");
        Console.WriteLine();

        // Local function: scoped to this method, can be recursive
        var input = "((a + b) * (c - (d / e)))";
        var (depth, balanced) = AnalyzeParentheses(input);
        Console.WriteLine($"    Input: {input}");
        Console.WriteLine($"    Max depth: {depth}, Balanced: {balanced}");

        Console.WriteLine();

        // Static local function: can't capture variables
        var items = new[] { "alpha", "beta", "gamma", "delta" };
        var csv = FormatAsCsv(items);
        Console.WriteLine($"    CSV: {csv}");

        // Demonstration: recursive local function
        Console.WriteLine();
        var fibonacci = ComputeFibonacci(10);
        Console.WriteLine($"    Fibonacci(10): {fibonacci}");

        Console.WriteLine();
        Console.WriteLine("    ✓ Local functions: scoped to the enclosing method");
        Console.WriteLine("    ✓ Support recursion (unlike lambdas)");
        Console.WriteLine("    ✓ static local: compile error if you capture variables");
        Console.WriteLine("    ✓ No delegate allocation (unlike Func<> lambdas)");
    }

    /// <summary>Analyzes parentheses using a local function.</summary>
    private static (int MaxDepth, bool Balanced) AnalyzeParentheses(string input)
    {
        var maxDepth = 0;
        var currentDepth = 0;
        var balanced = true;

        // Local function — scoped here, can access outer variables
        void ProcessChar(char ch)
        {
            if (ch == '(')
            {
                currentDepth++;
                maxDepth = Math.Max(maxDepth, currentDepth);
            }
            else if (ch == ')')
            {
                currentDepth--;
                if (currentDepth < 0) balanced = false;
            }
        }

        foreach (var ch in input)
            ProcessChar(ch);

        if (currentDepth != 0) balanced = false;
        return (maxDepth, balanced);
    }

    /// <summary>Formats items as CSV using a static local function.</summary>
    private static string FormatAsCsv(string[] items)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < items.Length; i++)
        {
            if (i > 0) sb.Append(',');
            sb.Append(EscapeValue(items[i]));
        }
        return sb.ToString();

        // Static local function — guaranteed no captures, no closure allocation
        static string EscapeValue(string value) =>
            value.Contains(',') ? $"\"{value}\"" : value;
    }

    /// <summary>Computes Fibonacci using a recursive local function.</summary>
    private static long ComputeFibonacci(int n)
    {
        return Fib(n);

        // Local function with recursion — lambdas can't do this cleanly
        static long Fib(int n) => n switch
        {
            <= 0 => 0,
            1 => 1,
            _ => Fib(n - 1) + Fib(n - 2)
        };
    }

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // Local functions solve three problems:
    // 1. Scoping: helper logic stays near its only caller
    // 2. Recursion: unlike lambdas, local functions can recurse
    //    without needing a variable declaration first
    // 3. Performance: no delegate allocation (static local = guaranteed)

    // GOING DEEPER:
    // The compiler transforms local functions differently from lambdas:
    // - Lambda: generates a delegate + possible closure class (heap alloc)
    // - Local function: generates a regular method (no delegate, no alloc)
    // - Static local function: additionally guarantees no captures
    // For iterator methods and async methods, local functions enable
    // "eager validation" — validate parameters in the outer method,
    // then yield/await in the local function.

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: Write a method ParseNestedTags(string html) that uses
    // a recursive local function to count the depth of nested
    // <div> tags. Use a static local function for tag detection.

    /// <summary>Runs the complete local functions demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: Local Functions (C# 7.0 / 8.0)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforeLocalFunctions();
        WithLocalFunctions();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
