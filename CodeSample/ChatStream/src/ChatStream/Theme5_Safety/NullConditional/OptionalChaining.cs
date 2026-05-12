namespace ChatStream.Theme5_Safety;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: Null-Conditional Operators (?. and ?[])  (C# 6)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  PROBLEM
//  -------
//  Deep null checks produced verbose, deeply-nested code.
//  Accessing obj.Prop1.Prop2.Method() required checking
//  each step for null, or risking NullReferenceException.
//
//  SOLUTION
//  --------
//  The ?. operator short-circuits to null if the left side
//  is null.  ?[] does the same for indexers.  Combined with
//  the null-coalescing operator (??), you get concise,
//  null-safe access chains.
//
//  WHY IT MATTERS
//  ──────────────
//  Code reads left-to-right, just like the happy path.
//  Null propagation is implicit and zero-overhead — the
//  compiler generates the same IL as manual null checks.
//
//  TRY IT
//  ──────
//  1. Chain ?. through 3 levels of nesting.
//  2. Combine ?. with ?? to provide fallback values.
//  3. Use ?[] with an array that might be null.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Demonstrates null-conditional operators in chat message processing.
/// </summary>
public static class OptionalChainingDemo
{
    // ── Helper types to create deep object graphs ────────────

    /// <summary>A user profile with optional metadata.</summary>
    private record UserProfile(
        string Name,
        UserPreferences? Preferences = null,
        string[]? Badges = null);

    /// <summary>User preferences — all fields optional.</summary>
    private record UserPreferences(
        string? Theme = null,
        NotificationSettings? Notifications = null);

    /// <summary>Notification settings — deeply nested optional.</summary>
    private record NotificationSettings(
        bool Enabled = true,
        string[]? MutedChannels = null);

    // ── Before: verbose null-check pyramids ──────────────────

    private static string GetThemeOldWay(UserProfile? profile)
    {
        // The "pyramid of doom" — each level needs its own check.
        if (profile != null)
        {
            if (profile.Preferences != null)
            {
                if (profile.Preferences.Theme != null)
                {
                    return profile.Preferences.Theme;
                }
            }
        }
        return "default";
    }

    // ── After: null-conditional chains ───────────────────────

    private static string GetThemeNewWay(UserProfile? profile)
    {
        // ?. short-circuits the entire chain to null if any
        // part is null, then ?? provides the fallback.
        return profile?.Preferences?.Theme ?? "default";
    }

    /// <summary>
    /// Checks if a channel is muted — demonstrates ?. with method calls.
    /// </summary>
    private static bool IsChannelMuted(UserProfile? profile, string channel)
    {
        // ?. works with method calls too. If any part is null,
        // Contains() is never called and the result is false.
        return profile?.Preferences?.Notifications?.MutedChannels?
            .Contains(channel) ?? false;
    }

    /// <summary>
    /// Gets the first badge — demonstrates ?[] indexer access.
    /// </summary>
    private static string GetFirstBadge(UserProfile? profile)
    {
        // ?[] returns null if the array is null; we also need
        // to check length to avoid IndexOutOfRangeException.
        // Note: ?[] protects against null, not empty arrays.
        return (profile?.Badges?.Length > 0)
            ? profile.Badges[0]
            : "(no badges)";
    }

    /// <summary>
    /// Gets message content length — demonstrates ?. with value type results.
    /// </summary>
    private static int GetContentLength(ChatMessage? message)
    {
        // When ?. accesses a value-type property, the result
        // becomes a nullable value type (int? here).
        int? length = message?.Content?.Length;
        return length ?? 0;
    }

    public static void Run()
    {
        Console.WriteLine("╔═══ Null-Conditional Operators ═══╗\n");

        // ── 1. Deep null-safe access ─────────────────────────
        Console.WriteLine("── Deep Null-Safe Access ──");

        var fullProfile = new UserProfile(
            "Alice",
            new UserPreferences("dark", new NotificationSettings(true, ["announcements"])),
            ["admin", "early-adopter"]);

        var minimalProfile = new UserProfile("Bob");
        UserProfile? nullProfile = null;

        Console.WriteLine($"  Alice's theme (old): {GetThemeOldWay(fullProfile)}");
        Console.WriteLine($"  Alice's theme (new): {GetThemeNewWay(fullProfile)}");
        Console.WriteLine($"  Bob's theme (new):   {GetThemeNewWay(minimalProfile)}");
        Console.WriteLine($"  Null profile theme:  {GetThemeNewWay(nullProfile)}");

        // ── 2. Null-conditional with method calls ────────────
        Console.WriteLine("\n── Null-Conditional + Methods ──");
        Console.WriteLine($"  Alice muted 'announcements': {IsChannelMuted(fullProfile, "announcements")}");
        Console.WriteLine($"  Alice muted 'general':       {IsChannelMuted(fullProfile, "general")}");
        Console.WriteLine($"  Bob muted 'general':         {IsChannelMuted(minimalProfile, "general")}");
        Console.WriteLine($"  Null muted 'general':        {IsChannelMuted(nullProfile, "general")}");

        // ── 3. Null-conditional indexer ?[] ───────────────────
        Console.WriteLine("\n── Null-Conditional Indexer ──");
        Console.WriteLine($"  Alice's first badge: {GetFirstBadge(fullProfile)}");
        Console.WriteLine($"  Bob's first badge:   {GetFirstBadge(minimalProfile)}");
        Console.WriteLine($"  Null's first badge:  {GetFirstBadge(nullProfile)}");

        // ── 4. Null-conditional with value types ─────────────
        Console.WriteLine("\n── Null-Conditional + Value Types ──");
        var message = ChatMessage.Create("Alice", "Hello, ChatStream!", "general");
        ChatMessage? nullMessage = null;

        Console.WriteLine($"  Message length:      {GetContentLength(message)}");
        Console.WriteLine($"  Null message length:  {GetContentLength(nullMessage)}");

        // ── 5. Chaining ?. with string methods ───────────────
        Console.WriteLine("\n── Chaining with String Methods ──");
        string? channelName = message?.Channel?.ToUpperInvariant();
        string? nullChannel = nullMessage?.Channel?.ToUpperInvariant();
        Console.WriteLine($"  Channel uppercased: {channelName ?? "(null)"}");
        Console.WriteLine($"  Null uppercased:    {nullChannel ?? "(null)"}");

        // ── 6. Event-raising pattern ─────────────────────────
        Console.WriteLine("\n── Event-Raising Pattern ──");
        Console.WriteLine("  In older C#: if (handler != null) handler(args);");
        Console.WriteLine("  With ?.:     handler?.Invoke(args);");
        Console.WriteLine("  This is thread-safe — no race between null check and invoke.");

        Action<string>? onMessageReceived = msg =>
            Console.WriteLine($"  Event received: {msg}");
        Action<string>? nullHandler = null;

        onMessageReceived?.Invoke("Hello!");
        nullHandler?.Invoke("This never prints");
        Console.WriteLine("  Null handler invoked safely (no output, no crash).");
    }
}
