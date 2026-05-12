namespace ChatStream.Theme5_Safety;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: System.Threading.Lock  (C# 13 / .NET 9)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  PROBLEM
//  -------
//  The traditional `lock(object)` pattern is error-prone:
//  • Locking on `this` or `typeof()` exposes the lock to
//    external code that can cause deadlocks.
//  • The lock object is untyped — any object reference can
//    be accidentally used as a lock.
//  • No way to enforce that a specific object is ONLY used
//    for locking.
//
//  SOLUTION
//  --------
//  .NET 9 introduces `System.Threading.Lock` — a dedicated,
//  type-safe lock object.  The compiler recognizes it and
//  generates optimized locking code using Lock.EnterScope()
//  instead of Monitor.Enter/Exit.
//
//  WHY IT MATTERS
//  ──────────────
//  The Lock type prevents common threading mistakes at the
//  type level.  You can't accidentally lock on the wrong
//  object, and the compiler can optimize the lock/unlock
//  sequence.
//
//  TRY IT
//  ──────
//  1. Replace `private readonly object _lock = new();` with
//     `private readonly Lock _lock = new();` — note the
//     compiler recognizes the type.
//  2. Try locking on `this` — compare the safety guarantees.
//  3. Use Lock.EnterScope() explicitly for advanced scenarios.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Demonstrates the new System.Threading.Lock type for thread-safe chat rooms.
/// </summary>
public static class ThreadSafeRoomDemo
{
    // ── Before: lock(object) — functional but fragile ────────

    /// <summary>
    /// Old-style thread-safe message store using lock(object).
    /// </summary>
    private sealed class MessageStoreOld
    {
        // Problem 1: 'object' gives no hint that this is a lock.
        // Problem 2: Nothing prevents someone from locking on a
        //            different object, or exposing this one.
        private readonly object _lock = new();
        private readonly List<ChatMessage> _messages = [];

        public void Add(ChatMessage message)
        {
            lock (_lock)
            {
                _messages.Add(message);
            }
        }

        public int Count
        {
            get
            {
                lock (_lock) { return _messages.Count; }
            }
        }
    }

    // ── After: System.Threading.Lock — type-safe ─────────────

    /// <summary>
    /// Modern thread-safe message store using System.Threading.Lock.
    /// </summary>
    private sealed class MessageStoreNew
    {
        // The Lock type makes intent crystal clear.
        // The compiler generates optimized code using
        // Lock.EnterScope() instead of Monitor.Enter/Exit.
        private readonly Lock _lock = new();
        private readonly List<ChatMessage> _messages = [];
        private int _totalBytes;

        public void Add(ChatMessage message)
        {
            // The `lock` statement recognizes System.Threading.Lock
            // and emits a Scope-based pattern under the hood:
            //   using (Lock.Scope scope = _lock.EnterScope())
            //   {
            //       _messages.Add(message);
            //   }
            lock (_lock)
            {
                _messages.Add(message);
                _totalBytes += message.Content.Length * 2; // UTF-16
            }
        }

        public (int Count, int TotalBytes) GetStats()
        {
            lock (_lock)
            {
                return (_messages.Count, _totalBytes);
            }
        }

        /// <summary>
        /// Demonstrates explicit EnterScope() for advanced control.
        /// </summary>
        public ChatMessage? FindLatest(string sender)
        {
            // EnterScope() returns a ref struct that calls
            // _lock.Exit() when disposed — guaranteed cleanup.
            using Lock.Scope scope = _lock.EnterScope();
            for (int i = _messages.Count - 1; i >= 0; i--)
            {
                if (_messages[i].Sender == sender)
                    return _messages[i];
            }
            return null;
        }

        /// <summary>
        /// Gets a snapshot of messages under the lock.
        /// </summary>
        public IReadOnlyList<ChatMessage> GetSnapshot()
        {
            lock (_lock)
            {
                return [.. _messages]; // Collection expression creates a copy
            }
        }
    }

    /// <summary>
    /// A thread-safe chat room using Lock for all mutable state.
    /// </summary>
    private sealed class ConcurrentChatRoom
    {
        private readonly Lock _usersLock = new();
        private readonly Lock _messagesLock = new();
        private readonly List<string> _users = [];
        private readonly List<ChatMessage> _messages = [];

        /// <summary>
        /// Uses separate locks for users and messages to allow
        /// concurrent reads/writes to different collections.
        /// </summary>
        public void Join(string userName)
        {
            lock (_usersLock)
            {
                if (!_users.Contains(userName))
                    _users.Add(userName);
            }
        }

        public void Send(ChatMessage message)
        {
            lock (_messagesLock)
            {
                _messages.Add(message);
            }
        }

        public (int UserCount, int MessageCount) GetStats()
        {
            // Need both locks for a consistent snapshot
            lock (_usersLock)
            {
                lock (_messagesLock)
                {
                    return (_users.Count, _messages.Count);
                }
            }
        }
    }

    public static void Run()
    {
        Console.WriteLine("╔═══ System.Threading.Lock ═══╗\n");

        // ── 1. Before vs After ───────────────────────────────
        Console.WriteLine("── Before vs After ──");
        Console.WriteLine("  Old: private readonly object _lock = new();");
        Console.WriteLine("  New: private readonly Lock _lock = new();");
        Console.WriteLine("  Both use `lock (_lock) { ... }` — same syntax!");
        Console.WriteLine("  But the compiler generates different, optimized IL.");

        // ── 2. Basic usage ───────────────────────────────────
        Console.WriteLine("\n── Basic Lock Usage ──");
        var store = new MessageStoreNew();
        store.Add(ChatMessage.Create("Alice", "Hello!", "general"));
        store.Add(ChatMessage.Create("Bob", "Hi there!", "general"));
        store.Add(ChatMessage.Create("Alice", "How are you?", "general"));

        var stats = store.GetStats();
        Console.WriteLine($"  Messages: {stats.Count}, Bytes: {stats.TotalBytes}");

        // ── 3. Explicit EnterScope() ─────────────────────────
        Console.WriteLine("\n── Explicit EnterScope() ──");
        var latest = store.FindLatest("Alice");
        Console.WriteLine($"  Latest from Alice: {latest}");
        Console.WriteLine($"  Latest from Eve:   {store.FindLatest("Eve")?.ToString() ?? "(none)"}");

        // ── 4. Concurrent access simulation ──────────────────
        Console.WriteLine("\n── Concurrent Access ──");
        var concurrentStore = new MessageStoreNew();

        // Simulate concurrent writes from multiple threads
        var tasks = new List<Task>();
        var senders = new[] { "Alice", "Bob", "Charlie", "Diana" };
        int messagesPerSender = 25;

        foreach (var sender in senders)
        {
            tasks.Add(Task.Run(() =>
            {
                for (int i = 0; i < messagesPerSender; i++)
                {
                    concurrentStore.Add(
                        ChatMessage.Create(sender, $"Message {i}", "general"));
                }
            }));
        }

        Task.WaitAll([.. tasks]);
        var concurrentStats = concurrentStore.GetStats();
        Console.WriteLine($"  Expected: {senders.Length * messagesPerSender} messages");
        Console.WriteLine($"  Actual:   {concurrentStats.Count} messages");
        Console.WriteLine($"  Thread-safe: {concurrentStats.Count == senders.Length * messagesPerSender}");

        // ── 5. Snapshot under lock ───────────────────────────
        Console.WriteLine("\n── Snapshot Under Lock ──");
        var snapshot = concurrentStore.GetSnapshot();
        Console.WriteLine($"  Snapshot size: {snapshot.Count}");
        Console.WriteLine($"  First: {snapshot[0]}");
        Console.WriteLine($"  Last:  {snapshot[^1]}");

        // ── 6. Separate locks pattern ────────────────────────
        Console.WriteLine("\n── Separate Locks for Fine-Grained Control ──");
        var room = new ConcurrentChatRoom();
        room.Join("Alice");
        room.Join("Bob");
        room.Send(ChatMessage.Create("Alice", "Hello!", "general"));

        var roomStats = room.GetStats();
        Console.WriteLine($"  Users: {roomStats.UserCount}, Messages: {roomStats.MessageCount}");

        // ── 7. Key differences ───────────────────────────────
        Console.WriteLine("\n── Key Differences ──");
        Console.WriteLine("  ┌────────────────────┬──────────────┬──────────────┐");
        Console.WriteLine("  │ Feature            │ lock(object) │ Lock         │");
        Console.WriteLine("  ├────────────────────┼──────────────┼──────────────┤");
        Console.WriteLine("  │ Type safety        │ Any object   │ Dedicated    │");
        Console.WriteLine("  │ Compiler optimized │ Monitor.*    │ EnterScope() │");
        Console.WriteLine("  │ Lock exposure risk │ High         │ Low          │");
        Console.WriteLine("  │ Intent clarity     │ Ambiguous    │ Explicit     │");
        Console.WriteLine("  │ Scope-based exit   │ No           │ Yes (ref)    │");
        Console.WriteLine("  └────────────────────┴──────────────┴──────────────┘");
    }
}
