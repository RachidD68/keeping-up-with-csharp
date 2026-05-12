namespace ChatStream.Theme5_Safety;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: Null-Conditional Assignment (?.=)  (C# 14)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  PROBLEM
//  -------
//  You often want to update a property on an object, but
//  only if the object itself isn't null:
//    if (user != null) user.Status = Online;
//  This is verbose and error-prone when deeply nested.
//
//  SOLUTION
//  --------
//  C# 14 introduces ?.= which assigns to a member only if
//  the receiver is non-null.  The assignment is skipped
//  entirely when the left side of ?. is null.
//
//  WHY IT MATTERS
//  ──────────────
//  Combined with ?. for reading, ?.= for writing completes
//  the null-safe access story.  Deeply nested optional
//  mutations become one-liners.
//
//  TRY IT
//  ──────
//  1. Use ?.= to update a deeply nested optional property.
//  2. Compare the generated IL with a manual null check.
//  3. Chain ?.= with ?. to conditionally update sub-objects.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Demonstrates null-conditional assignment for chat state updates.
/// </summary>
public static class ConditionalUpdateDemo
{
    // ── Mutable state types (needed to demonstrate assignment) ─

    /// <summary>Mutable user session for demonstrating ?.= assignment.</summary>
    private sealed class UserSession
    {
        public string Name { get; set; }
        public UserStatus Status { get; set; }
        public string? CurrentChannel { get; set; }
        public SessionMetrics? Metrics { get; set; }

        public UserSession(string name)
        {
            Name = name;
            Status = UserStatus.Online;
        }

        public override string ToString() =>
            $"{Name} ({Status}) in #{CurrentChannel ?? "(none)"}";
    }

    /// <summary>Optional session metrics.</summary>
    private sealed class SessionMetrics
    {
        public int MessagesSent { get; set; }
        public DateTimeOffset? LastActivity { get; set; }

        public override string ToString() =>
            $"Sent: {MessagesSent}, Last: {LastActivity?.ToString("HH:mm:ss") ?? "never"}";
    }

    /// <summary>Chat room state with optional active session.</summary>
    private sealed class RoomState
    {
        public string RoomName { get; set; } = "general";
        public UserSession? ActiveSession { get; set; }
        public string? Topic { get; set; }
    }

    // ── Before: verbose null-check-then-assign ──────────────

    private static void UpdateSessionOldWay(UserSession? session, UserStatus newStatus)
    {
        // Must explicitly check for null before every assignment.
        if (session != null)
        {
            session.Status = newStatus;
        }
    }

    private static void UpdateMetricsOldWay(UserSession? session)
    {
        // Deeply nested — each level needs its own check.
        if (session != null)
        {
            if (session.Metrics != null)
            {
                session.Metrics.MessagesSent++;
                session.Metrics.LastActivity = DateTimeOffset.UtcNow;
            }
        }
    }

    // ── After: ?.= null-conditional assignment (C# 14) ──────

    private static void UpdateSessionNewWay(UserSession? session, UserStatus newStatus)
    {
        // C# 14: assign only if session is non-null
        session?.Status = newStatus;
    }

    private static void UpdateMetricsNewWay(UserSession? session)
    {
        // C# 14: null-conditional on nested properties
        session?.Metrics?.LastActivity = DateTimeOffset.UtcNow;

        // Increment also works with ?.
        if (session?.Metrics is { } metrics)
        {
            metrics.MessagesSent++;
        }
    }

    /// <summary>
    /// Demonstrates the full ?.= pattern with deeply nested state.
    /// </summary>
    private static void ConditionallyUpdateRoom(RoomState? state, string? newTopic)
    {
        // C# 14: null-conditional assignment on nested objects
        state?.Topic = newTopic;

        // Deep chain: only assigns if both state and ActiveSession are non-null
        if (state?.ActiveSession is not null)
        {
            state.ActiveSession.CurrentChannel = state.RoomName;
        }
    }

    public static void Run()
    {
        Console.WriteLine("╔═══ Null-Conditional Assignment (?.=) ═══╗\n");

        // ── 1. Basic null-conditional assignment ─────────────
        Console.WriteLine("── Basic ?.= Pattern ──");

        var alice = new UserSession("Alice")
        {
            Metrics = new SessionMetrics()
        };
        UserSession? nobody = null;

        Console.WriteLine($"  Before: {alice}");
        UpdateSessionNewWay(alice, UserStatus.Away);
        Console.WriteLine($"  After:  {alice}");

        Console.Write("  Updating null session: ");
        UpdateSessionNewWay(nobody, UserStatus.Online); // No-op, no crash
        Console.WriteLine("(no crash — assignment skipped)");

        // ── 2. Deeply nested conditional update ──────────────
        Console.WriteLine("\n── Nested ?.= Updates ──");

        Console.WriteLine($"  Metrics before: {alice.Metrics}");
        UpdateMetricsNewWay(alice);
        UpdateMetricsNewWay(alice);
        UpdateMetricsNewWay(alice);
        Console.WriteLine($"  Metrics after:  {alice.Metrics}");

        // Null metrics — assignment skipped entirely
        var bob = new UserSession("Bob"); // No Metrics
        UpdateMetricsNewWay(bob);
        Console.WriteLine($"  Bob's metrics:  {bob.Metrics?.ToString() ?? "(null — update skipped)"}");

        // ── 3. Room state conditional update ─────────────────
        Console.WriteLine("\n── Room State Update ──");

        var room = new RoomState
        {
            RoomName = "design",
            ActiveSession = new UserSession("Charlie"),
            Topic = "UI redesign"
        };
        RoomState? emptyRoom = null;

        Console.WriteLine($"  Room topic:   {room.Topic}");
        Console.WriteLine($"  Active user:  {room.ActiveSession}");

        ConditionallyUpdateRoom(room, "New color palette");
        Console.WriteLine($"  After update: {room.Topic}");
        Console.WriteLine($"  User channel: {room.ActiveSession?.CurrentChannel}");

        ConditionallyUpdateRoom(emptyRoom, "Ghost topic");
        Console.WriteLine($"  Null room:    (no crash — update skipped)");

        // ── 4. Pattern comparison ────────────────────────────
        Console.WriteLine("\n── Pattern Comparison ──");
        Console.WriteLine("  Old C#:  if (x != null) x.Prop = value;");
        Console.WriteLine("  C# 14:   x?.Prop = value;");
        Console.WriteLine("  Old C#:  if (x?.Y != null) x.Y.Z = value;");
        Console.WriteLine("  C# 14:   x?.Y?.Z = value;");
        Console.WriteLine();
        Console.WriteLine("  ?.= completes the null-safety story:");
        Console.WriteLine("    ?.  = null-safe reading  (C# 6)");
        Console.WriteLine("    ??= = null-coalescing assign (C# 8)");
        Console.WriteLine("    ?.= = null-conditional assign (C# 14)");
    }
}
