namespace ChatStream.Theme5_Safety;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: Nullable Reference Types  (C# 8)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  PROBLEM
//  -------
//  Before C# 8, every reference type was implicitly nullable.
//  NullReferenceException was the single most common runtime
//  error — Tony Hoare's "billion-dollar mistake."
//
//  SOLUTION
//  --------
//  Enable <Nullable>enable</Nullable> in the project.  The
//  compiler now tracks nullability flow, warns on unsafe
//  dereferences, and distinguishes `string` (never null)
//  from `string?` (might be null).
//
//  WHY IT MATTERS
//  ──────────────
//  Null bugs are caught at compile time, not at 3 AM in
//  production.  APIs become self-documenting: callers can
//  see whether null is a valid input or return value.
//
//  TRY IT
//  ──────
//  1. Remove a null check — observe the compiler warning.
//  2. Add [NotNullWhen(true)] to TryFindMessage.
//  3. Create a MessageStore that returns string? vs string.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Demonstrates nullable reference types in a chat message API.
/// </summary>
public static class SafeMessageApiDemo
{
    // ── Before: everything is implicitly nullable ─────────────

    /// <summary>
    /// Old-style API — caller has no idea whether null is valid.
    /// </summary>
    private static string FormatMessageUnsafe(string sender, string content)
    {
        // Any parameter could be null at runtime despite the
        // non-nullable signature. The compiler gives no warnings
        // because nullable analysis didn't exist yet.
        return $"[{sender}]: {content}";
    }

    // ── After: nullable annotations make intent explicit ──────

    /// <summary>
    /// Finds a message by ID, returning null if not found.
    /// The return type <c>ChatMessage?</c> tells callers to expect null.
    /// </summary>
    private static ChatMessage? FindMessage(
        IReadOnlyList<ChatMessage> messages,
        Guid id)
    {
        // The compiler knows the return type is nullable, so
        // callers must handle the null case.
        foreach (var msg in messages)
        {
            if (msg.Id == id)
                return msg;
        }
        return null;
    }

    /// <summary>
    /// Try-pattern with nullable analysis — compiler tracks
    /// the relationship between return value and out parameter.
    /// </summary>
    private static bool TryFindMessage(
        IReadOnlyList<ChatMessage> messages,
        string sender,
        out ChatMessage? result)
    {
        foreach (var msg in messages)
        {
            if (msg.Sender == sender)
            {
                result = msg;
                return true;
            }
        }
        result = null;
        return false;
    }

    /// <summary>
    /// Formats a message safely — all null paths handled.
    /// </summary>
    private static string FormatMessageSafe(
        ChatMessage? message,
        string? fallbackSender = null)
    {
        // The compiler enforces null checks before dereference.
        if (message is null)
            return $"[{fallbackSender ?? "System"}]: (no message)";

        // After the null check, 'message' is narrowed to non-null.
        // No further checks needed — the compiler knows.
        return message.ToString();
    }

    /// <summary>
    /// Demonstrates nullable-aware collection filtering.
    /// </summary>
    private static IReadOnlyList<ChatMessage> FilterNonNull(
        IEnumerable<ChatMessage?> messages)
    {
        // OfType<T> naturally filters out nulls and returns
        // a non-nullable sequence — a neat trick.
        return messages.OfType<ChatMessage>().ToList();
    }

    public static void Run()
    {
        Console.WriteLine("╔═══ Nullable Reference Types ═══╗\n");

        // Build some test data
        var messages = new List<ChatMessage>
        {
            ChatMessage.Create("Alice", "Hello, world!", "general"),
            ChatMessage.Create("Bob", "Hi there!", "general"),
            ChatMessage.Create("Charlie", "Anyone here?", "random")
        };

        // ── 1. Non-nullable parameter enforcement ────────────
        Console.WriteLine("── Non-Nullable Parameters ──");
        Console.WriteLine(FormatMessageUnsafe("Alice", "Test"));
        // FormatMessageUnsafe(null, "Test"); // ← Would compile in old C#,
        //   now gives CS8625 warning: "Cannot convert null literal to
        //   non-nullable reference type."

        // ── 2. Nullable return type — find by ID ─────────────
        Console.WriteLine("\n── Nullable Return Types ──");
        var existing = FindMessage(messages, messages[0].Id);
        var missing = FindMessage(messages, Guid.NewGuid());

        // The compiler forces us to check for null before using
        // 'existing' — if we skip the check, we get a warning.
        Console.WriteLine($"  Found:   {existing?.ToString() ?? "(not found)"}");
        Console.WriteLine($"  Missing: {missing?.ToString() ?? "(not found)"}");

        // ── 3. Try-pattern with out parameter ────────────────
        Console.WriteLine("\n── Try-Pattern ──");
        if (TryFindMessage(messages, "Bob", out var found))
        {
            // 'found' is narrowed to non-null inside this block
            // because the method returned true.
            Console.WriteLine($"  Found Bob's message: {found}");
        }
        if (!TryFindMessage(messages, "Unknown", out _))
        {
            Console.WriteLine("  Unknown sender not found (as expected).");
        }

        // ── 4. Safe formatting with fallback ─────────────────
        Console.WriteLine("\n── Safe Formatting ──");
        Console.WriteLine($"  With message:  {FormatMessageSafe(messages[0])}");
        Console.WriteLine($"  Null message:  {FormatMessageSafe(null, "Fallback")}");
        Console.WriteLine($"  Null both:     {FormatMessageSafe(null)}");

        // ── 5. Filtering nulls from mixed sequences ──────────
        Console.WriteLine("\n── Null Filtering ──");
        ChatMessage?[] mixed = [messages[0], null, messages[2], null];
        var clean = FilterNonNull(mixed);
        Console.WriteLine($"  Input: {mixed.Length} items ({mixed.Count(m => m is null)} nulls)");
        Console.WriteLine($"  After filtering: {clean.Count} non-null messages");
        foreach (var msg in clean)
            Console.WriteLine($"    • {msg}");
    }
}
