namespace ChatStream.Theme8_Async;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: ConfigureAwait  (C# 5 / evolved through .NET 8)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  PROBLEM
//  -------
//  By default, `await` captures the current synchronization
//  context and resumes on it.  In UI apps, this means
//  resuming on the UI thread.  In ASP.NET (old), resuming
//  on the request thread.  In library code, this causes
//  deadlocks when the caller blocks with .Result or .Wait().
//
//  SOLUTION
//  --------
//  ConfigureAwait(false) tells the awaiter NOT to capture
//  the synchronization context.  The continuation runs on
//  any available thread pool thread — faster and deadlock-free.
//
//  WHY IT MATTERS
//  ──────────────
//  • Library code: ALWAYS use ConfigureAwait(false)
//  • App code (ASP.NET Core): context doesn't matter (no SyncCtx)
//  • UI code: use ConfigureAwait(true) for UI thread access
//
//  TRY IT
//  ──────
//  1. Run this in a WPF app to see the thread ID difference.
//  2. Try ConfigureAwait(ConfigureAwaitOptions.ForceYielding).
//  3. Compare behavior with and without a SynchronizationContext.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Explores ConfigureAwait behavior and best practices.
/// </summary>
public static class ContextExplorerDemo
{
    /// <summary>
    /// Simulates a library method that should use ConfigureAwait(false).
    /// </summary>
    private static async Task<ChatMessage> FetchMessageLibraryAsync(
        Guid id, CancellationToken ct = default)
    {
        Console.WriteLine($"    Before await: Thread {Environment.CurrentManagedThreadId}");

        // ConfigureAwait(false) means "I don't need to resume
        // on the captured context." This prevents deadlocks
        // when called from UI or old ASP.NET code.
        await Task.Delay(50, ct).ConfigureAwait(false);

        Console.WriteLine($"    After await:  Thread {Environment.CurrentManagedThreadId}");

        return ChatMessage.Create("System", $"Message {id}", "general");
    }

    /// <summary>
    /// Shows the default behavior (ConfigureAwait(true)) — context captured.
    /// </summary>
    private static async Task<ChatMessage> FetchMessageDefaultAsync(
        Guid id, CancellationToken ct = default)
    {
        Console.WriteLine($"    Before await: Thread {Environment.CurrentManagedThreadId}");

        // Default behavior (same as ConfigureAwait(true)):
        // Captures and resumes on the synchronization context.
        // In a console app with no SyncCtx, this is equivalent
        // to ConfigureAwait(false).
        await Task.Delay(50, ct);

        Console.WriteLine($"    After await:  Thread {Environment.CurrentManagedThreadId}");

        return ChatMessage.Create("System", $"Message {id}", "general");
    }

    /// <summary>
    /// Demonstrates ConfigureAwait with async streams.
    /// </summary>
    private static async IAsyncEnumerable<ChatMessage> StreamWithConfigureAwait(
        int count,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        for (int i = 0; i < count; i++)
        {
            // ConfigureAwait(false) on each await in the stream
            await Task.Delay(30, ct).ConfigureAwait(false);
            yield return ChatMessage.Create(
                "Streamer",
                $"Config-awaited message {i + 1}",
                "general");
        }
    }

    /// <summary>
    /// .NET 8+ ConfigureAwaitOptions — more granular control.
    /// </summary>
    private static async Task<string> DemoConfigureAwaitOptions(
        CancellationToken ct = default)
    {
        Console.WriteLine($"    Before: Thread {Environment.CurrentManagedThreadId}");

        // .NET 8 introduced ConfigureAwaitOptions for richer control.
        // ForceYielding ensures the continuation never runs synchronously,
        // even if the task is already completed.
        await Task.Delay(10, ct)
            .ConfigureAwait(ConfigureAwaitOptions.ForceYielding);

        Console.WriteLine($"    After ForceYielding: Thread {Environment.CurrentManagedThreadId}");

        // ContinueOnCapturedContext = false is equivalent to
        // ConfigureAwait(false) but more readable.
        await Task.Delay(10, ct)
            .ConfigureAwait(ConfigureAwaitOptions.None);

        Console.WriteLine($"    After None: Thread {Environment.CurrentManagedThreadId}");

        return "Completed with ConfigureAwaitOptions";
    }

    public static async Task RunAsync()
    {
        Console.WriteLine("╔═══ ConfigureAwait ═══╗\n");

        // ── 1. Default behavior ──────────────────────────────
        Console.WriteLine("── Default (captures context) ──");
        var msg1 = await FetchMessageDefaultAsync(Guid.NewGuid());
        Console.WriteLine($"  Result: {msg1.Content}\n");

        // ── 2. ConfigureAwait(false) ─────────────────────────
        Console.WriteLine("── ConfigureAwait(false) ──");
        var msg2 = await FetchMessageLibraryAsync(Guid.NewGuid());
        Console.WriteLine($"  Result: {msg2.Content}\n");

        // ── 3. Multiple awaits with ConfigureAwait ───────────
        Console.WriteLine("── Multiple Awaits ──");
        Console.WriteLine("  Each await with ConfigureAwait(false):");
        Console.WriteLine($"    Start thread: {Environment.CurrentManagedThreadId}");

        await Task.Delay(20).ConfigureAwait(false);
        Console.WriteLine($"    After 1st:    {Environment.CurrentManagedThreadId}");

        await Task.Delay(20).ConfigureAwait(false);
        Console.WriteLine($"    After 2nd:    {Environment.CurrentManagedThreadId}");

        await Task.Delay(20).ConfigureAwait(false);
        Console.WriteLine($"    After 3rd:    {Environment.CurrentManagedThreadId}");
        Console.WriteLine();

        // ── 4. Async streams with ConfigureAwait ─────────────
        Console.WriteLine("── Async Streams + ConfigureAwait ──");
        await foreach (var message in StreamWithConfigureAwait(3)
            .ConfigureAwait(false))
        {
            Console.WriteLine($"  Thread {Environment.CurrentManagedThreadId}: {message.Content}");
        }
        Console.WriteLine();

        // ── 5. ConfigureAwaitOptions (.NET 8+) ───────────────
        Console.WriteLine("── ConfigureAwaitOptions (.NET 8+) ──");
        var optionsResult = await DemoConfigureAwaitOptions();
        Console.WriteLine($"  Result: {optionsResult}\n");

        // ── 6. Decision guide ────────────────────────────────
        Console.WriteLine("── When to Use What ──");
        Console.WriteLine("  ┌──────────────────────┬────────────────────────────┐");
        Console.WriteLine("  │ Context              │ Recommendation             │");
        Console.WriteLine("  ├──────────────────────┼────────────────────────────┤");
        Console.WriteLine("  │ Library code         │ ConfigureAwait(false)      │");
        Console.WriteLine("  │ ASP.NET Core         │ Doesn't matter (no ctx)    │");
        Console.WriteLine("  │ UI (WPF/WinForms)    │ Default (need UI thread)   │");
        Console.WriteLine("  │ Console apps         │ Doesn't matter (no ctx)    │");
        Console.WriteLine("  │ Force thread switch  │ ForceYielding              │");
        Console.WriteLine("  └──────────────────────┴────────────────────────────┘");
    }
}
