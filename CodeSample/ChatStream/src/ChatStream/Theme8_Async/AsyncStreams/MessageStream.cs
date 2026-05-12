namespace ChatStream.Theme8_Async;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: IAsyncEnumerable<T>  (C# 8)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  PROBLEM
//  -------
//  Before C# 8, you had two options for producing data:
//  • IEnumerable<T> — synchronous, blocks the thread.
//  • Task<List<T>> — returns everything at once, no streaming.
//  Neither worked for live data feeds where items arrive
//  asynchronously over time.
//
//  SOLUTION
//  --------
//  IAsyncEnumerable<T> combines the pull-based laziness of
//  IEnumerable with the non-blocking nature of async/await.
//  Use `await foreach` to consume, and `yield return` with
//  async to produce.
//
//  WHY IT MATTERS
//  ──────────────
//  Chat messages arrive one at a time.  IAsyncEnumerable
//  is the natural abstraction: each await foreach iteration
//  suspends until the next message is available, without
//  blocking a thread.
//
//  TRY IT
//  ──────
//  1. Add a filter parameter to stream only certain senders.
//  2. Compose two async streams with SelectAwait.
//  3. Use .WithCancellation() to add timeout behavior.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Demonstrates IAsyncEnumerable for streaming chat messages.
/// </summary>
public static class MessageStreamDemo
{
    // ── Before: return everything at once ─────────────────────

    /// <summary>
    /// Old-style: fetch all messages, then return.
    /// Caller waits for ALL messages before processing any.
    /// </summary>
    private static async Task<List<ChatMessage>> GetAllMessagesAsync(
        int count, CancellationToken ct = default)
    {
        var messages = new List<ChatMessage>();
        for (int i = 0; i < count; i++)
        {
            await Task.Delay(50, ct);
            messages.Add(ChatMessage.Create("System", $"Batch message {i + 1}", "general"));
        }
        return messages; // All or nothing — no streaming
    }

    // ── After: stream messages one at a time ─────────────────

    /// <summary>
    /// Produces messages as an async stream — caller gets each
    /// message as soon as it's available.
    /// </summary>
    private static async IAsyncEnumerable<ChatMessage> StreamMessagesAsync(
        string channel,
        int count,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        for (int i = 0; i < count; i++)
        {
            // Simulate messages arriving over time
            await Task.Delay(80, ct);

            var sender = (i % 3) switch
            {
                0 => "Alice",
                1 => "Bob",
                _ => "Charlie"
            };

            // yield return produces one item at a time.
            // The caller receives it immediately via await foreach.
            yield return ChatMessage.Create(
                sender,
                $"Stream message {i + 1} of {count}",
                channel);
        }
    }

    /// <summary>
    /// Demonstrates composable async stream operations.
    /// Filters messages from a specific sender.
    /// </summary>
    private static async IAsyncEnumerable<ChatMessage> FilterBySenderAsync(
        IAsyncEnumerable<ChatMessage> source,
        string sender,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var message in source.WithCancellation(ct))
        {
            if (message.Sender == sender)
                yield return message;
        }
    }

    /// <summary>
    /// Transforms an async stream — adds a prefix to each message.
    /// </summary>
    private static async IAsyncEnumerable<string> FormatStreamAsync(
        IAsyncEnumerable<ChatMessage> source,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        int index = 0;
        await foreach (var message in source.WithCancellation(ct))
        {
            index++;
            yield return $"  [{index}] {message}";
        }
    }

    /// <summary>
    /// Takes the first N items from an async stream.
    /// </summary>
    private static async IAsyncEnumerable<T> TakeAsync<T>(
        IAsyncEnumerable<T> source,
        int count,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        int taken = 0;
        await foreach (var item in source.WithCancellation(ct))
        {
            yield return item;
            if (++taken >= count)
                yield break; // Stop producing after N items
        }
    }

    public static async Task RunAsync()
    {
        Console.WriteLine("╔═══ IAsyncEnumerable<T> ═══╗\n");

        // ── 1. Before: batch fetch ───────────────────────────
        Console.WriteLine("── Before: Batch Fetch ──");
        Console.Write("  Fetching all messages...");
        var batch = await GetAllMessagesAsync(3);
        Console.WriteLine(" done!");
        foreach (var msg in batch)
            Console.WriteLine($"  {msg}");

        // ── 2. After: async stream ───────────────────────────
        Console.WriteLine("\n── After: Async Stream ──");
        Console.WriteLine("  Messages arrive one at a time:");
        await foreach (var message in StreamMessagesAsync("general", 5))
        {
            // Each iteration awaits the next message.
            // The thread is free between messages.
            Console.WriteLine($"  ← {message}");
        }

        // ── 3. Composable: filter by sender ──────────────────
        Console.WriteLine("\n── Filtered Stream (Alice only) ──");
        var allMessages = StreamMessagesAsync("general", 6);
        var aliceOnly = FilterBySenderAsync(allMessages, "Alice");
        await foreach (var message in aliceOnly)
        {
            Console.WriteLine($"  ← {message}");
        }

        // ── 4. Composable: format + take ─────────────────────
        Console.WriteLine("\n── Formatted + Take(3) ──");
        var stream = StreamMessagesAsync("dev", 10);
        var formatted = FormatStreamAsync(stream);
        var limited = TakeAsync(formatted, 3);
        await foreach (var line in limited)
        {
            Console.WriteLine(line);
        }
        Console.WriteLine("  (stopped after 3 — remaining items not produced)");

        // ── 5. Cancellation with async streams ───────────────
        Console.WriteLine("\n── Cancellation ──");
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(250));
        int received = 0;
        try
        {
            await foreach (var message in StreamMessagesAsync("general", 100)
                .WithCancellation(cts.Token))
            {
                received++;
                Console.WriteLine($"  ← {message}");
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"  Cancelled after receiving {received} messages.");
        }

        // ── 6. Key insight ───────────────────────────────────
        Console.WriteLine("\n── Key Insight ──");
        Console.WriteLine("  IAsyncEnumerable = IEnumerable + async/await");
        Console.WriteLine("  • Producer: async method with yield return");
        Console.WriteLine("  • Consumer: await foreach (var item in source)");
        Console.WriteLine("  • Lazy: items produced on-demand, not upfront");
        Console.WriteLine("  • Composable: filter, transform, take, skip...");
        Console.WriteLine("  • Cancellable: .WithCancellation(token)");
    }
}
