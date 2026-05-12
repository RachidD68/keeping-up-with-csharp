namespace ChatStream.Theme8_Async;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: IAsyncDisposable  (C# 8)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  PROBLEM
//  -------
//  Some resources need async cleanup: flushing buffers,
//  closing network connections, completing channel writers.
//  IDisposable.Dispose() is synchronous — calling async
//  methods from Dispose() leads to deadlocks or fire-and-
//  forget bugs.
//
//  SOLUTION
//  --------
//  IAsyncDisposable provides DisposeAsync() which returns
//  a ValueTask.  Use `await using` instead of `using` to
//  ensure async cleanup runs to completion.
//
//  WHY IT MATTERS
//  ──────────────
//  Chat connections hold network resources that need graceful
//  shutdown.  IAsyncDisposable ensures cleanup happens
//  properly — no deadlocks, no forgotten flushes.
//
//  TRY IT
//  ──────
//  1. Add IAsyncDisposable to a class that wraps a Channel.
//  2. Implement both IDisposable and IAsyncDisposable.
//  3. Track disposal state and throw ObjectDisposedException.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Demonstrates IAsyncDisposable for managing chat connections.
/// </summary>
public static class ConnectionManagerDemo
{
    // ── Before: synchronous disposal of async resources ──────

    /// <summary>
    /// Old-style: IDisposable can't properly clean up async resources.
    /// </summary>
    private sealed class OldConnection : IDisposable
    {
        private bool _disposed;

        public void Connect() =>
            Console.WriteLine("    [Old] Connected");

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            // Can't await here! Must use .Wait() or .GetAwaiter().GetResult()
            // which can deadlock in UI/ASP.NET contexts.
            Console.WriteLine("    [Old] Disposed (synchronously — may miss async cleanup)");
        }
    }

    // ── After: IAsyncDisposable with await using ─────────────

    /// <summary>
    /// Modern: IAsyncDisposable allows proper async cleanup.
    /// </summary>
    private sealed class ChatConnection : IAsyncDisposable, IDisposable
    {
        private readonly string _endpoint;
        private readonly List<ChatMessage> _pendingFlush = [];
        private bool _disposed;

        public ChatConnection(string endpoint)
        {
            _endpoint = endpoint;
            Console.WriteLine($"    [New] Connecting to {endpoint}...");
        }

        public bool IsConnected => !_disposed;

        /// <summary>Queues a message for delivery.</summary>
        public void QueueMessage(ChatMessage message)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _pendingFlush.Add(message);
            Console.WriteLine($"    [New] Queued: {message.Content}");
        }

        /// <summary>Flushes pending messages asynchronously.</summary>
        private async Task FlushAsync()
        {
            if (_pendingFlush.Count > 0)
            {
                Console.WriteLine($"    [New] Flushing {_pendingFlush.Count} pending messages...");
                await Task.Delay(50); // Simulate network flush
                Console.WriteLine($"    [New] Flush complete.");
                _pendingFlush.Clear();
            }
        }

        /// <summary>Gracefully disconnects from the endpoint.</summary>
        private async Task DisconnectAsync()
        {
            Console.WriteLine($"    [New] Disconnecting from {_endpoint}...");
            await Task.Delay(30); // Simulate disconnect handshake
            Console.WriteLine($"    [New] Disconnected cleanly.");
        }

        /// <summary>
        /// Async disposal — flushes pending messages, then disconnects.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;

            await FlushAsync();
            await DisconnectAsync();

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Synchronous fallback — logs a warning if used instead of DisposeAsync.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Console.WriteLine($"    [New] WARNING: Synchronous dispose — async cleanup skipped!");
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// A chat session that owns multiple async-disposable resources.
    /// </summary>
    private sealed class ChatSession : IAsyncDisposable
    {
        private readonly ChatConnection _connection;
        private readonly ChatChannel _channel;
        private bool _disposed;

        public ChatSession(string endpoint, string channelName)
        {
            _connection = new ChatConnection(endpoint);
            _channel = new ChatChannel(channelName, capacity: 10);
        }

        public async ValueTask SendAsync(string sender, string content, CancellationToken ct = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            var message = ChatMessage.Create(sender, content, _channel.Name);
            _connection.QueueMessage(message);
            await _channel.Writer.WriteAsync(message, ct);
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;

            Console.WriteLine("    [Session] Cleaning up session...");

            // Complete the channel writer
            _channel.Writer.Complete();
            Console.WriteLine("    [Session] Channel writer completed.");

            // Dispose the connection (flushes pending messages)
            await _connection.DisposeAsync();

            Console.WriteLine("    [Session] Session cleaned up.");
        }
    }

    public static async Task RunAsync()
    {
        Console.WriteLine("╔═══ IAsyncDisposable ═══╗\n");

        // ── 1. Old-style synchronous disposal ────────────────
        Console.WriteLine("── Before: Synchronous Disposal ──");
        using (var oldConn = new OldConnection())
        {
            oldConn.Connect();
        } // Dispose() called — can't do async cleanup
        Console.WriteLine();

        // ── 2. Modern async disposal ─────────────────────────
        Console.WriteLine("── After: await using ──");
        await using (var conn = new ChatConnection("chat.example.com"))
        {
            conn.QueueMessage(ChatMessage.Create("Alice", "Hello!", "general"));
            conn.QueueMessage(ChatMessage.Create("Bob", "Hi!", "general"));
            conn.QueueMessage(ChatMessage.Create("Alice", "Goodbye!", "general"));
        } // DisposeAsync() called — flushes messages, disconnects
        Console.WriteLine();

        // ── 3. Declaration form of await using ───────────────
        Console.WriteLine("── Declaration Form ──");
        {
            await using var conn2 = new ChatConnection("backup.example.com");
            conn2.QueueMessage(ChatMessage.Create("System", "Heartbeat", "system"));
        } // DisposeAsync() at end of scope
        Console.WriteLine();

        // ── 4. Disposal on exception ─────────────────────────
        Console.WriteLine("── Disposal on Exception ──");
        try
        {
            await using var conn3 = new ChatConnection("fragile.example.com");
            conn3.QueueMessage(ChatMessage.Create("Eve", "Before error", "general"));
            throw new InvalidOperationException("Simulated error!");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"    Caught: {ex.Message}");
            Console.WriteLine("    (DisposeAsync still ran — resources cleaned up!)");
        }
        Console.WriteLine();

        // ── 5. Composite resource — ChatSession ──────────────
        Console.WriteLine("── Composite Resource ──");
        await using (var session = new ChatSession("prod.example.com", "general"))
        {
            await session.SendAsync("Alice", "Session message 1");
            await session.SendAsync("Bob", "Session message 2");
        } // Both channel and connection cleaned up
        Console.WriteLine();

        // ── 6. ObjectDisposedException after disposal ────────
        Console.WriteLine("── Post-Disposal Protection ──");
        var closedConn = new ChatConnection("temp.example.com");
        await closedConn.DisposeAsync();
        try
        {
            closedConn.QueueMessage(
                ChatMessage.Create("Hacker", "Should fail", "general"));
        }
        catch (ObjectDisposedException)
        {
            Console.WriteLine("    ObjectDisposedException caught — can't use after disposal!");
        }

        // ── 7. Key insight ───────────────────────────────────
        Console.WriteLine("\n── Key Insight ──");
        Console.WriteLine("  IDisposable.Dispose()       → synchronous cleanup");
        Console.WriteLine("  IAsyncDisposable.DisposeAsync() → async cleanup");
        Console.WriteLine("  using (var x = ...)         → calls Dispose()");
        Console.WriteLine("  await using (var x = ...)   → calls DisposeAsync()");
        Console.WriteLine("  Both guarantee cleanup, even on exceptions.");
    }
}
