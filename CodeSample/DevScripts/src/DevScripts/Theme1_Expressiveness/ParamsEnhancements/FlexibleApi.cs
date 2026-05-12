// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: params Enhancements                           ║
// ║  Introduced: C# 13                                      ║
// ║  Theme: 1 — Expressiveness & Boilerplate Reduction      ║
// ╚══════════════════════════════════════════════════════════╝

namespace DevScripts.Theme1_Expressiveness.ParamsEnhancements;

/// <summary>
/// Demonstrates enhanced <c>params</c> that works with
/// <c>Span&lt;T&gt;</c>, <c>ReadOnlySpan&lt;T&gt;</c>, and
/// <c>IEnumerable&lt;T&gt;</c> — not just arrays.
/// </summary>
public static class FlexibleApiDemo
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — How we did it before (C# 12)
    // ──────────────────────────────────────────────────────────

    public static void BeforeParamsEnhancements()
    {
        Console.WriteLine("  BEFORE (C# 12 — params only works with arrays):");
        Console.WriteLine();

        // params allocates an array on every call
        LogMessages_Old("Info", "Starting up", "Loading config", "Ready");

        Console.WriteLine("    ⚠ params string[] allocates a new array every call");
        Console.WriteLine("    ⚠ Can't use params with Span<T> or IEnumerable<T>");
        Console.WriteLine();
    }

    private static void LogMessages_Old(string level, params string[] messages)
    {
        Console.WriteLine($"    [{level}] {messages.Length} messages (array allocated)");
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — The modern way (C# 13)
    // ──────────────────────────────────────────────────────────

    public static void WithParamsEnhancements()
    {
        Console.WriteLine("  AFTER (C# 13 — params with Span, IEnumerable, etc.):");
        Console.WriteLine();

        // params ReadOnlySpan<T> — zero allocation!
        LogMessages("Info", "Starting up", "Loading config", "Ready");

        // params IEnumerable<T> — works with any collection
        LogFlexible("Debug", "Step 1", "Step 2", "Step 3");

        // You can still pass existing collections
        List<string> existingList = ["Error A", "Error B"];
        LogFlexible("Error", [..existingList, "Error C"]);

        Console.WriteLine();

        // params with collection expressions
        var report = BuildReport("header", "line1", "line2", "line3", "footer");
        Console.WriteLine($"    Report: {report}");

        Console.WriteLine();
        Console.WriteLine("    ✓ params ReadOnlySpan<T> — zero-allocation variadic args");
        Console.WriteLine("    ✓ params IEnumerable<T> — accepts any collection type");
        Console.WriteLine("    ✓ Backward compatible — existing params T[] still works");
    }

    /// <summary>
    /// params with ReadOnlySpan — stack-allocated, zero heap allocation.
    /// </summary>
    private static void LogMessages(string level, params ReadOnlySpan<string> messages)
    {
        Console.WriteLine($"    [{level}] {messages.Length} messages (stack-allocated span, zero heap alloc)");
        foreach (var msg in messages)
            Console.WriteLine($"      → {msg}");
    }

    /// <summary>
    /// params with IEnumerable — maximum flexibility.
    /// </summary>
    private static void LogFlexible(string level, params IEnumerable<string> messages)
    {
        var count = messages.Count();
        Console.WriteLine($"    [{level}] {count} messages (IEnumerable — any source)");
    }

    /// <summary>
    /// Building results from params span.
    /// </summary>
    private static string BuildReport(params ReadOnlySpan<string> sections)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < sections.Length; i++)
        {
            if (i > 0) sb.Append(" | ");
            sb.Append(sections[i]);
        }
        return sb.ToString();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // Every call to a params T[] method allocates an array on the
    // heap. In high-frequency code (logging, event handling), this
    // creates GC pressure. params ReadOnlySpan<T> eliminates this
    // allocation entirely — the compiler uses stackalloc or inline
    // storage. This is a "free performance win" for any API that
    // accepts variable arguments.

    // GOING DEEPER:
    // The compiler resolves params overloads with these priorities:
    // 1. params ReadOnlySpan<T> (preferred — zero allocation)
    // 2. params Span<T>
    // 3. params T[] (legacy — always allocates)
    // 4. params IEnumerable<T> (most flexible)
    // If you provide multiple overloads, the compiler picks the
    // best match. For new APIs, prefer params ReadOnlySpan<T>.
    // See also: Theme 6 — Span<T> and Memory (full Span deep dive).

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: Convert an existing method with params string[] to
    // params ReadOnlySpan<string>. Verify it still works with
    // the same call sites. Measure the allocation difference.

    /// <summary>Runs the complete params enhancements demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: params Enhancements (C# 13)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforeParamsEnhancements();
        WithParamsEnhancements();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
