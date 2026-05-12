namespace ChatStream.Theme8_Async;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: async / await  (C# 5)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  PROBLEM
//  -------
//  Before C# 5, asynchronous code required callbacks,
//  IAsyncResult, or event-based patterns.  Code was split
//  across methods, error handling was fragile, and
//  composing async operations was a nightmare.
//
//  SOLUTION
//  --------
//  The async/await pattern lets you write asynchronous code
//  that READS like synchronous code.  The compiler transforms
//  it into a state machine that suspends and resumes at each
//  await point.
//
//  WHY IT MATTERS
//  ──────────────
//  Async/await is the foundation of modern C#.  Every web
//  server, every UI app, every cloud service uses it.
//  Understanding the basics is essential before exploring
//  ValueTask, IAsyncEnumerable, and Channels.
//
//  TRY IT
//  ──────
//  1. Add a 3rd async step that validates the message content.
//  2. Make SendMessageAsync return the message ID for tracking.
//  3. Add a Task.WhenAll to send to multiple channels.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Demonstrates async/await fundamentals in a chat messaging context.
/// </summary>
public static class BasicMessagingDemo
{
    // ── Before: callback-based async ─────────────────────────

    /// <summary>
    /// Old-style async with callbacks — hard to read, hard to debug.
    /// </summary>
    private static void SendMessageCallback(
        string sender, string content, string channel,
        Action<ChatMessage> onSuccess,
        Action<Exception> onError)
    {
        // Simulate async work with ThreadPool
        ThreadPool.QueueUserWorkItem(_ =>
        {
            try
            {
                Thread.Sleep(50); // Simulate network delay
                var message = ChatMessage.Create(sender, content, channel);
                onSuccess(message);
            }
            catch (Exception ex)
            {
                onError(ex);
            }
        });
    }

    // ── After: async/await — reads like synchronous code ─────

    /// <summary>
    /// Simulates connecting to a chat server asynchronously.
    /// </summary>
    private static async Task<ConnectionInfo> ConnectAsync(
        string endpoint,
        CancellationToken ct = default)
    {
        Console.WriteLine($"    Connecting to {endpoint}...");

        // await suspends here, freeing the thread for other work.
        // When the delay completes, execution resumes.
        await Task.Delay(100, ct);

        Console.WriteLine($"    Connected to {endpoint}!");
        return new ConnectionInfo(endpoint, TimeSpan.FromSeconds(30));
    }

    /// <summary>
    /// Sends a message asynchronously — each await is a suspension point.
    /// </summary>
    private static async Task<ChatMessage> SendMessageAsync(
        string sender, string content, string channel,
        CancellationToken ct = default)
    {
        // Step 1: Validate (simulate async validation)
        await Task.Delay(50, ct);
        ArgumentException.ThrowIfNullOrEmpty(sender);
        ArgumentException.ThrowIfNullOrEmpty(content);

        // Step 2: Create the message
        var message = ChatMessage.Create(sender, content, channel);

        // Step 3: Simulate delivery
        await Task.Delay(50, ct);

        return message;
    }

    /// <summary>
    /// Demonstrates async composition — calling async from async.
    /// </summary>
    private static async Task<IReadOnlyList<ChatMessage>> SendBatchAsync(
        string sender, string channel, string[] contents,
        CancellationToken ct = default)
    {
        var messages = new List<ChatMessage>();

        foreach (var content in contents)
        {
            // Each send is awaited sequentially — one at a time.
            var msg = await SendMessageAsync(sender, content, channel, ct);
            messages.Add(msg);
        }

        return messages;
    }

    /// <summary>
    /// Demonstrates async error handling — try/catch works naturally.
    /// </summary>
    private static async Task<string> SafeSendAsync(
        string sender, string content, string channel)
    {
        try
        {
            var msg = await SendMessageAsync(sender, content, channel);
            return $"Sent: {msg}";
        }
        catch (ArgumentException ex)
        {
            // Exception handling works exactly like sync code!
            return $"Failed: {ex.Message}";
        }
    }

    public static async Task RunAsync()
    {
        Console.WriteLine("╔═══ async / await ═══╗\n");

        // ── 1. Basic async/await ─────────────────────────────
        Console.WriteLine("── Basic async/await ──");
        var conn = await ConnectAsync("chat.example.com");
        Console.WriteLine($"  Connection: {conn.Endpoint} (timeout: {conn.Timeout})");

        // ── 2. Sending a message ─────────────────────────────
        Console.WriteLine("\n── Send Message ──");
        var message = await SendMessageAsync("Alice", "Hello, async world!", "general");
        Console.WriteLine($"  {message}");

        // ── 3. Sequential batch ──────────────────────────────
        Console.WriteLine("\n── Sequential Batch ──");
        var batch = await SendBatchAsync(
            "Bob", "general",
            ["First message", "Second message", "Third message"]);
        foreach (var msg in batch)
            Console.WriteLine($"  {msg}");

        // ── 4. Parallel sends with Task.WhenAll ──────────────
        Console.WriteLine("\n── Parallel Sends (Task.WhenAll) ──");
        var parallelTasks = new[]
        {
            SendMessageAsync("Alice", "Parallel 1", "general"),
            SendMessageAsync("Bob", "Parallel 2", "random"),
            SendMessageAsync("Charlie", "Parallel 3", "dev"),
        };

        // All three sends happen concurrently!
        var results = await Task.WhenAll(parallelTasks);
        foreach (var msg in results)
            Console.WriteLine($"  {msg}");

        // ── 5. Error handling ────────────────────────────────
        Console.WriteLine("\n── Error Handling ──");
        Console.WriteLine($"  Valid:   {await SafeSendAsync("Alice", "Hi!", "general")}");
        Console.WriteLine($"  Invalid: {await SafeSendAsync("", "Hi!", "general")}");

        // ── 6. Task.WhenAny — first responder ────────────────
        Console.WriteLine("\n── First Responder (Task.WhenAny) ──");
        var fast = SendMessageAsync("Flash", "I'm fastest!", "speed");
        var slow = Task.Delay(500).ContinueWith(_ =>
            ChatMessage.Create("Turtle", "...eventually", "speed"));

        var winner = await Task.WhenAny(fast, slow);
        Console.WriteLine($"  Winner: {await winner}");

        // ── 7. Key insight ───────────────────────────────────
        Console.WriteLine("\n── Key Insight ──");
        Console.WriteLine("  async/await transforms your code into a state machine.");
        Console.WriteLine("  Each 'await' is a potential suspension point where the");
        Console.WriteLine("  thread is freed for other work. When the awaited task");
        Console.WriteLine("  completes, execution resumes where it left off.");
    }
}
