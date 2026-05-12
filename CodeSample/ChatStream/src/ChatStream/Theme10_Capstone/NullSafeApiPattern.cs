namespace ChatStream.Theme10_Capstone;

// ╔══════════════════════════════════════════════════════════════════╗
// ║  Theme 10 Capstone — Null-Safe API Pattern                     ║
// ║                                                                ║
// ║  Combines every null-safety feature from Theme 5 into a        ║
// ║  cohesive, production-quality API:                             ║
// ║                                                                ║
// ║  • Nullable reference types (string? vs string)                ║
// ║  • Null-conditional operators (?. and ?[])                     ║
// ║  • Null-coalescing assignment (??=)                            ║
// ║  • CallerArgumentExpression guards                             ║
// ║  • Exception filters for targeted error handling               ║
// ║  • Pattern matching for null-safe branching                    ║
// ║                                                                ║
// ║  The result: a chat notification system where null             ║
// ║  is explicitly modeled, never crashes, and invalid states      ║
// ║  are caught at compile time.                                   ║
// ╚══════════════════════════════════════════════════════════════════╝

/// <summary>
/// Capstone: A null-safe notification pipeline for chat messages.
/// Every piece of data flows through typed-null gates.
/// </summary>
public static class NullSafeApiPatternDemo
{
    // ── Domain types with explicit nullability ───────────────

    /// <summary>
    /// Notification preference — every field has explicit null semantics.
    /// Null means "not configured" (use system default).
    /// </summary>
    private record NotificationPreference(
        bool? SoundEnabled = null,        // null = use system default
        string? CustomRingtone = null,    // null = default ringtone
        string[]? MutedChannels = null,   // null = nothing muted
        TimeSpan? QuietHoursStart = null, // null = no quiet hours
        TimeSpan? QuietHoursEnd = null);

    /// <summary>
    /// A notification target — may or may not have preferences.
    /// </summary>
    private record NotificationTarget(
        string UserId,
        string DisplayName,
        NotificationPreference? Preferences = null,
        string? DeviceToken = null);  // null = no push notifications

    /// <summary>Notification delivery result.</summary>
    private record DeliveryResult(
        string UserId,
        bool Delivered,
        string? FailureReason = null);

    /// <summary>Delivery channel for notifications.</summary>
    private enum DeliveryChannel { InApp, Push, Email, Sms }

    /// <summary>
    /// Chat-specific exception with error codes.
    /// </summary>
    private sealed class NotificationException(
        string message, int errorCode, string? userId = null)
        : Exception(message)
    {
        public int ErrorCode { get; } = errorCode;
        public string? UserId { get; } = userId;
    }

    // ── The Null-Safe Notification Engine ────────────────────

    /// <summary>
    /// The core notification engine — demonstrates all null-safety features
    /// working together in a production-quality API.
    /// </summary>
    private sealed class NotificationEngine
    {
        private Dictionary<string, NotificationTarget>? _targets;
        private List<DeliveryResult>? _deliveryLog;

        // ── ??= for lazy initialization ──────────────────────

        private Dictionary<string, NotificationTarget> Targets =>
            _targets ??= new Dictionary<string, NotificationTarget>();

        private List<DeliveryResult> DeliveryLog =>
            _deliveryLog ??= [];

        // ── Guards with CallerArgumentExpression ─────────────

        public void RegisterTarget(NotificationTarget? target)
        {
            // Guard.Against.Null captures "target" as the expression
            var valid = Guard.Against.Null(target);
            Guard.Against.NullOrWhiteSpace(valid.UserId);
            Guard.Against.NullOrWhiteSpace(valid.DisplayName);

            Targets[valid.UserId] = valid;
        }

        // ── Null-conditional chains for safe access ──────────

        /// <summary>
        /// Resolves the notification sound for a user.
        /// Uses ?. chains with ?? fallbacks at every level.
        /// </summary>
        public string ResolveSoundForUser(string userId)
        {
            // ?. chain: target?.Preferences?.CustomRingtone
            // ?? fallback at each level
            var target = Targets.GetValueOrDefault(userId);
            var ringtone = target?.Preferences?.CustomRingtone
                ?? "default-chime.wav";

            var soundEnabled = target?.Preferences?.SoundEnabled ?? true;

            return soundEnabled ? ringtone : "(muted)";
        }

        /// <summary>
        /// Checks if a channel is muted for a user.
        /// Demonstrates ?. with ?[] and ?? in one expression.
        /// </summary>
        public bool IsChannelMuted(string userId, string channel)
        {
            return Targets.GetValueOrDefault(userId)
                ?.Preferences
                ?.MutedChannels
                ?.Contains(channel) ?? false;
        }

        /// <summary>
        /// Determines the best delivery channel for a user.
        /// Pattern matching handles all null combinations.
        /// </summary>
        public DeliveryChannel ResolveDeliveryChannel(string userId)
        {
            var target = Targets.GetValueOrDefault(userId);

            // Pattern matching makes null handling explicit and exhaustive
            return target switch
            {
                null => DeliveryChannel.InApp,  // Unknown user → in-app only
                { DeviceToken: not null } => DeliveryChannel.Push,  // Has push token
                { Preferences.SoundEnabled: true } => DeliveryChannel.InApp,
                _ => DeliveryChannel.InApp
            };
        }

        /// <summary>
        /// Sends a notification with comprehensive null safety.
        /// Exception filters handle specific error categories.
        /// </summary>
        public DeliveryResult SendNotification(
            string userId, ChatMessage? message)
        {
            try
            {
                // Guard against null message
                var validMessage = Guard.Against.Null(message);
                Guard.Against.NullOrEmpty(validMessage.Content);

                var target = Targets.GetValueOrDefault(userId);
                if (target is null)
                {
                    return LogResult(new DeliveryResult(
                        userId, false, "User not registered"));
                }

                // Check quiet hours using null-safe comparison
                if (IsInQuietHours(target))
                {
                    return LogResult(new DeliveryResult(
                        userId, false, "User is in quiet hours"));
                }

                // Check muted channels
                if (IsChannelMuted(userId, validMessage.Channel))
                {
                    return LogResult(new DeliveryResult(
                        userId, false, $"Channel #{validMessage.Channel} is muted"));
                }

                // Simulate delivery
                var channel = ResolveDeliveryChannel(userId);
                Console.WriteLine(
                    $"    → [{channel}] {target.DisplayName}: {validMessage.Content}");

                return LogResult(new DeliveryResult(userId, true));
            }
            // Exception filters for targeted error handling
            catch (NotificationException ex) when (ex.ErrorCode == 429)
            {
                return LogResult(new DeliveryResult(
                    userId, false, "Rate limited"));
            }
            catch (NotificationException ex) when (ex.ErrorCode >= 500)
            {
                return LogResult(new DeliveryResult(
                    userId, false, $"Server error: {ex.Message}"));
            }
            catch (ArgumentException ex)
            {
                return LogResult(new DeliveryResult(
                    userId, false, $"Invalid input: {ex.Message}"));
            }
        }

        /// <summary>
        /// Checks if the user is in quiet hours.
        /// Demonstrates deeply nested null-safe access.
        /// </summary>
        private static bool IsInQuietHours(NotificationTarget target)
        {
            // Null at any level means "no quiet hours configured"
            var start = target.Preferences?.QuietHoursStart;
            var end = target.Preferences?.QuietHoursEnd;

            if (start is null || end is null)
                return false;

            var now = DateTimeOffset.UtcNow.TimeOfDay;
            return now >= start.Value && now <= end.Value;
        }

        private DeliveryResult LogResult(DeliveryResult result)
        {
            DeliveryLog.Add(result);
            return result;
        }

        /// <summary>Gets delivery statistics.</summary>
        public (int Total, int Delivered, int Failed) GetStats()
        {
            var total = DeliveryLog.Count;
            var delivered = DeliveryLog.Count(r => r.Delivered);
            return (total, delivered, total - delivered);
        }

        /// <summary>
        /// Gets the failure reason for a user, or null.
        /// Demonstrates null-safe LINQ.
        /// </summary>
        public string? GetLastFailureReason(string userId) =>
            DeliveryLog
                .Where(r => r.UserId == userId && !r.Delivered)
                .LastOrDefault()
                ?.FailureReason;
    }

    public static void Run()
    {
        Console.WriteLine("╔═══ Theme 10 Capstone: Null-Safe API Pattern ═══╗\n");

        var engine = new NotificationEngine();

        // ── 1. Register users with varying null configurations ─
        Console.WriteLine("── Registering Users ──");

        engine.RegisterTarget(new NotificationTarget(
            "alice", "Alice",
            new NotificationPreference(
                SoundEnabled: true,
                CustomRingtone: "bell.mp3",
                MutedChannels: ["announcements"]),
            DeviceToken: "apns-alice-token"));

        engine.RegisterTarget(new NotificationTarget(
            "bob", "Bob",
            new NotificationPreference(SoundEnabled: false)));
        // Bob: no device token, no custom ringtone, no muted channels

        engine.RegisterTarget(new NotificationTarget(
            "charlie", "Charlie"));
        // Charlie: no preferences at all, no device token

        Console.WriteLine("  Registered: Alice (full config), Bob (minimal), Charlie (bare)");

        // ── 2. Sound resolution through null chains ──────────
        Console.WriteLine("\n── Sound Resolution (?.  + ??) ──");
        Console.WriteLine($"  Alice:   {engine.ResolveSoundForUser("alice")}");
        Console.WriteLine($"  Bob:     {engine.ResolveSoundForUser("bob")}");
        Console.WriteLine($"  Charlie: {engine.ResolveSoundForUser("charlie")}");
        Console.WriteLine($"  Unknown: {engine.ResolveSoundForUser("nobody")}");

        // ── 3. Delivery channel resolution ───────────────────
        Console.WriteLine("\n── Delivery Channel (pattern matching) ──");
        Console.WriteLine($"  Alice:   {engine.ResolveDeliveryChannel("alice")}");
        Console.WriteLine($"  Bob:     {engine.ResolveDeliveryChannel("bob")}");
        Console.WriteLine($"  Charlie: {engine.ResolveDeliveryChannel("charlie")}");
        Console.WriteLine($"  Unknown: {engine.ResolveDeliveryChannel("nobody")}");

        // ── 4. Muted channel checks ──────────────────────────
        Console.WriteLine("\n── Muted Channel Check (?. + ?[] + ??) ──");
        Console.WriteLine($"  Alice #announcements: {engine.IsChannelMuted("alice", "announcements")}");
        Console.WriteLine($"  Alice #general:       {engine.IsChannelMuted("alice", "general")}");
        Console.WriteLine($"  Bob #anything:        {engine.IsChannelMuted("bob", "anything")}");
        Console.WriteLine($"  Unknown #general:     {engine.IsChannelMuted("nobody", "general")}");

        // ── 5. Full notification pipeline ────────────────────
        Console.WriteLine("\n── Notification Pipeline ──");

        var msg1 = ChatMessage.Create("System", "Server update at 3pm", "general");
        var msg2 = ChatMessage.Create("System", "New policy", "announcements");
        ChatMessage? nullMsg = null;

        var r1 = engine.SendNotification("alice", msg1);
        Console.WriteLine($"  Alice/general:       {(r1.Delivered ? "✓" : "✗")} {r1.FailureReason ?? ""}");

        var r2 = engine.SendNotification("alice", msg2);
        Console.WriteLine($"  Alice/announcements: {(r2.Delivered ? "✓" : "✗")} {r2.FailureReason ?? ""}");

        var r3 = engine.SendNotification("bob", msg1);
        Console.WriteLine($"  Bob/general:         {(r3.Delivered ? "✓" : "✗")} {r3.FailureReason ?? ""}");

        var r4 = engine.SendNotification("nobody", msg1);
        Console.WriteLine($"  Unknown:             {(r4.Delivered ? "✓" : "✗")} {r4.FailureReason ?? ""}");

        var r5 = engine.SendNotification("alice", nullMsg);
        Console.WriteLine($"  Alice/null msg:      {(r5.Delivered ? "✓" : "✗")} {r5.FailureReason ?? ""}");

        // ── 6. Statistics ────────────────────────────────────
        Console.WriteLine("\n── Delivery Statistics ──");
        var stats = engine.GetStats();
        Console.WriteLine($"  Total: {stats.Total}, Delivered: {stats.Delivered}, Failed: {stats.Failed}");

        // ── 7. Null-safe failure reason lookup ───────────────
        Console.WriteLine("\n── Last Failure Reason (null-safe LINQ) ──");
        Console.WriteLine($"  Alice:   {engine.GetLastFailureReason("alice") ?? "(no failures)"}");
        Console.WriteLine($"  Nobody:  {engine.GetLastFailureReason("nobody") ?? "(no failures)"}");
        Console.WriteLine($"  Bob:     {engine.GetLastFailureReason("bob") ?? "(no failures)"}");

        // ── 8. Features used ─────────────────────────────────
        Console.WriteLine("\n── C# Features Combined ──");
        Console.WriteLine("  ✓ Nullable reference types (string? vs string)");
        Console.WriteLine("  ✓ Null-conditional access (?. and ?[])");
        Console.WriteLine("  ✓ Null-coalescing assignment (??=)");
        Console.WriteLine("  ✓ Null-coalescing operator (??)");
        Console.WriteLine("  ✓ CallerArgumentExpression guards");
        Console.WriteLine("  ✓ Exception filters (when clause)");
        Console.WriteLine("  ✓ Pattern matching (property patterns)");
        Console.WriteLine("  ✓ Records with optional fields");
        Console.WriteLine("  → Every null path is explicit, documented, and safe.");
    }
}
