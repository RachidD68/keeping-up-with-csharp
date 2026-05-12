namespace ChatStream.Theme8_Async;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: CancellationToken & Patterns  (C# 5+)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  PROBLEM
//  -------
//  Long-running operations need to be stoppable.  Without
//  a cancellation mechanism, you either abort threads
//  (dangerous!) or use boolean flags (error-prone, no
//  composition).
//
//  SOLUTION
//  --------
//  CancellationToken is a cooperative cancellation mechanism.
//  The caller creates a CancellationTokenSource, passes the
//  token to async methods, and signals cancellation when
//  needed.  The async method checks the token and throws
//  OperationCanceledException.
//
//  WHY IT MATTERS
//  ──────────────
//  Chat apps need graceful shutdown: drain pending messages,
//  close connections, notify users.  Linked tokens let you
//  compose timeout + user-cancellation.  Timeout patterns
//  prevent hung connections.
//
//  TRY IT
//  ──────
//  1. Use CreateLinkedTokenSource to combine timeout + user cancel.
//  2. Implement a retry loop that respects cancellation.
//  3. Add CancellationToken.Register() for cleanup callbacks.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Demonstrates cancellation patterns for graceful chat shutdown.
/// </summary>
public static class GracefulShutdownDemo
{
    /// <summary>
    /// Simulates a long-running message processing loop.
    /// </summary>
    private static async Task ProcessMessagesAsync(
        string channel,
        CancellationToken ct = default)
    {
        int count = 0;
        while (!ct.IsCancellationRequested)
        {
            count++;
            Console.WriteLine($"    Processing message {count} on #{channel}...");
            await Task.Delay(80, ct); // Throws if cancelled during delay
        }
    }

    /// <summary>
    /// Demonstrates CancellationToken.ThrowIfCancellationRequested().
    /// </summary>
    private static async Task ValidateAndSendAsync(
        IReadOnlyList<ChatMessage> messages,
        CancellationToken ct = default)
    {
        foreach (var msg in messages)
        {
            // Explicitly check before each expensive operation.
            // More control than waiting for the next await.
            ct.ThrowIfCancellationRequested();

            Console.WriteLine($"    Validating: {msg.Content}");
            await Task.Delay(50, ct);

            ct.ThrowIfCancellationRequested();

            Console.WriteLine($"    Sending:    {msg.Content}");
            await Task.Delay(50, ct);
        }
    }

    /// <summary>
    /// Timeout pattern using CancellationTokenSource.
    /// </summary>
    private static async Task<ChatMessage?> FetchWithTimeoutAsync(
        string sender,
        TimeSpan timeout)
    {
        // CancellationTokenSource with timeout — cancels automatically.
        using var cts = new CancellationTokenSource(timeout);

        try
        {
            Console.WriteLine($"    Fetching with {timeout.TotalMilliseconds}ms timeout...");
            await Task.Delay(200, cts.Token); // Simulates slow fetch
            return ChatMessage.Create(sender, "Fetched!", "general");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("    Timeout expired!");
            return null;
        }
    }

    /// <summary>
    /// Linked token source — combines user cancellation with timeout.
    /// This is a critical pattern for production code.
    /// </summary>
    private static async Task<ChatMessage?> FetchWithLinkedCancellationAsync(
        string sender,
        TimeSpan timeout,
        CancellationToken userToken)
    {
        // CreateLinkedTokenSource combines multiple cancellation sources.
        // The linked token fires when EITHER source is cancelled.
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(userToken);
        linkedCts.CancelAfter(timeout);

        try
        {
            Console.WriteLine($"    Fetching (linked: timeout + user cancel)...");
            await Task.Delay(150, linkedCts.Token);
            return ChatMessage.Create(sender, "Linked fetch!", "general");
        }
        catch (OperationCanceledException) when (!userToken.IsCancellationRequested)
        {
            Console.WriteLine("    Cancelled by timeout (not user).");
            return null;
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("    Cancelled by user.");
            return null;
        }
    }

    /// <summary>
    /// Register-based cleanup — runs when cancellation fires.
    /// </summary>
    private static async Task DemoRegistrationAsync(CancellationToken ct)
    {
        // Register a callback that runs when the token is cancelled.
        // Returns a CancellationTokenRegistration (disposable).
        using var registration = ct.Register(() =>
            Console.WriteLine("    [Callback] Cleanup registered on cancellation!"));

        Console.WriteLine("    Waiting for cancellation...");
        try
        {
            await Task.Delay(Timeout.Infinite, ct);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("    [Method] Cancelled — registration callback already ran.");
        }
    }

    /// <summary>
    /// Graceful shutdown: drain pending work before stopping.
    /// </summary>
    private static async Task GracefulDrainAsync(
        Channel<ChatMessage> channel,
        CancellationToken ct)
    {
        Console.WriteLine("    Starting graceful drain...");

        try
        {
            await foreach (var msg in channel.Reader.ReadAllAsync(ct))
            {
                Console.WriteLine($"    Drained: {msg.Content}");
                await Task.Delay(30, ct);
            }
        }
        catch (OperationCanceledException)
        {
            // Drain remaining items without cancellation
            Console.WriteLine("    Hard cancel — draining remaining items...");
            while (channel.Reader.TryRead(out var remaining))
            {
                Console.WriteLine($"    Final drain: {remaining.Content}");
            }
        }
    }

    public static async Task RunAsync()
    {
        Console.WriteLine("╔═══ Cancellation & Graceful Shutdown ═══╗\n");

        // ── 1. Basic cancellation ────────────────────────────
        Console.WriteLine("── Basic Cancellation ──");
        using var cts1 = new CancellationTokenSource();
        cts1.CancelAfter(TimeSpan.FromMilliseconds(300));
        try
        {
            await ProcessMessagesAsync("general", cts1.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("  Processing cancelled.\n");
        }

        // ── 2. ThrowIfCancellationRequested ──────────────────
        Console.WriteLine("── ThrowIfCancellationRequested ──");
        using var cts2 = new CancellationTokenSource();
        cts2.CancelAfter(TimeSpan.FromMilliseconds(180));
        var messages = new[]
        {
            ChatMessage.Create("Alice", "Msg 1", "general"),
            ChatMessage.Create("Bob", "Msg 2", "general"),
            ChatMessage.Create("Charlie", "Msg 3", "general"),
            ChatMessage.Create("Diana", "Msg 4", "general"),
        };
        try
        {
            await ValidateAndSendAsync(messages, cts2.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("  Batch cancelled mid-way.\n");
        }

        // ── 3. Timeout pattern ───────────────────────────────
        Console.WriteLine("── Timeout Pattern ──");
        var fast = await FetchWithTimeoutAsync("Alice", TimeSpan.FromSeconds(1));
        Console.WriteLine($"  Fast result: {fast}\n");

        var slow = await FetchWithTimeoutAsync("Bob", TimeSpan.FromMilliseconds(50));
        Console.WriteLine($"  Slow result: {slow?.ToString() ?? "(timed out)"}\n");

        // ── 4. Linked cancellation ───────────────────────────
        Console.WriteLine("── Linked Cancellation ──");
        using var userCts = new CancellationTokenSource();

        // Timeout fires first (100ms < user cancel)
        var linked1 = await FetchWithLinkedCancellationAsync(
            "Charlie", TimeSpan.FromMilliseconds(50), userCts.Token);
        Console.WriteLine($"  Result: {linked1?.ToString() ?? "(cancelled)"}\n");

        // ── 5. Registration callback ─────────────────────────
        Console.WriteLine("── Registration Callback ──");
        using var cts3 = new CancellationTokenSource();
        cts3.CancelAfter(TimeSpan.FromMilliseconds(100));
        await DemoRegistrationAsync(cts3.Token);
        Console.WriteLine();

        // ── 6. Graceful drain ────────────────────────────────
        Console.WriteLine("── Graceful Drain ──");
        var ch = Channel.CreateBounded<ChatMessage>(10);
        for (int i = 1; i <= 5; i++)
            await ch.Writer.WriteAsync(ChatMessage.Create("System", $"Item {i}", "drain"));
        ch.Writer.Complete();

        using var cts4 = new CancellationTokenSource();
        cts4.CancelAfter(TimeSpan.FromMilliseconds(100));
        await GracefulDrainAsync(ch, cts4.Token);

        // ── 7. Best practices ────────────────────────────────
        Console.WriteLine("\n── Best Practices ──");
        Console.WriteLine("  ✓ Always propagate CancellationToken through async chains");
        Console.WriteLine("  ✓ Use CreateLinkedTokenSource for timeout + user cancel");
        Console.WriteLine("  ✓ Check ct.IsCancellationRequested before expensive ops");
        Console.WriteLine("  ✓ Use ct.Register() for cleanup callbacks");
        Console.WriteLine("  ✓ Dispose CancellationTokenSource when done");
        Console.WriteLine("  ✗ Don't catch OperationCanceledException and swallow it");
    }
}
