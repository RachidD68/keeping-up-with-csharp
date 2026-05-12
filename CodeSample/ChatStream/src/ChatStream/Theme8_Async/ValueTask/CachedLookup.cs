namespace ChatStream.Theme8_Async;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: ValueTask<T>  (C# 7 / .NET Core 2.1)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  PROBLEM
//  -------
//  Task<T> allocates on the heap every time — even when the
//  result is available synchronously (cache hit, buffered
//  read, pre-computed value).  In hot paths, these
//  allocations add up to significant GC pressure.
//
//  SOLUTION
//  --------
//  ValueTask<T> is a struct that wraps either a T value
//  (synchronous completion) or a Task<T> (async completion).
//  When the fast path completes synchronously, there's zero
//  allocation.
//
//  WHY IT MATTERS
//  ──────────────
//  Use ValueTask when:
//  • The method often completes synchronously (cache hit)
//  • You're in a hot loop or high-throughput path
//  • Allocation pressure from Task<T> is measurable
//
//  ⚠️ Rules for ValueTask:
//  • Never await a ValueTask more than once
//  • Never use .Result/.GetAwaiter().GetResult() concurrently
//  • If you need to store it, convert to Task with .AsTask()
//
//  TRY IT
//  ──────
//  1. Add a ValueTask-returning method for batch lookups.
//  2. Compare Task<T> vs ValueTask<T> with BenchmarkDotNet.
//  3. Use IValueTaskSource for advanced pooling.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Demonstrates ValueTask for allocation-free cache lookups in chat.
/// </summary>
public static class CachedLookupDemo
{
    /// <summary>
    /// Message cache that returns cached results synchronously
    /// and fetches asynchronously only on cache miss.
    /// </summary>
    private sealed class MessageCache
    {
        private readonly Dictionary<Guid, ChatMessage> _cache = new();
        private int _cacheHits;
        private int _cacheMisses;

        // ── Before: Task<T> always allocates ─────────────────

        /// <summary>
        /// Task version — allocates even on cache hit.
        /// Every call creates a Task object on the heap.
        /// </summary>
        public async Task<ChatMessage?> GetMessageTaskAsync(Guid id)
        {
            if (_cache.TryGetValue(id, out var cached))
            {
                _cacheHits++;
                return cached; // Allocates a Task<ChatMessage> just to wrap the value!
            }

            _cacheMisses++;
            await Task.Delay(100); // Simulate fetch
            var message = ChatMessage.Create("System", $"Fetched {id}", "cache");
            _cache[id] = message;
            return message;
        }

        // ── After: ValueTask<T> — zero allocation on cache hit ─

        /// <summary>
        /// ValueTask version — no allocation on cache hit.
        /// Returns the cached value directly as a struct.
        /// </summary>
        public ValueTask<ChatMessage?> GetMessageAsync(Guid id)
        {
            if (_cache.TryGetValue(id, out var cached))
            {
                _cacheHits++;
                // Returns a ValueTask wrapping the value directly.
                // No heap allocation — it's a struct!
                return ValueTask.FromResult<ChatMessage?>(cached);
            }

            // Cache miss: delegate to async method.
            // Only allocates when we actually need to go async.
            return FetchAndCacheAsync(id);
        }

        private async ValueTask<ChatMessage?> FetchAndCacheAsync(Guid id)
        {
            _cacheMisses++;
            await Task.Delay(50); // Simulate network fetch
            var message = ChatMessage.Create("System", $"Fetched {id}", "cache");
            _cache[id] = message;
            return message;
        }

        /// <summary>Pre-populates the cache for demonstration.</summary>
        public void Seed(ChatMessage message) =>
            _cache[message.Id] = message;

        public (int Hits, int Misses) Stats => (_cacheHits, _cacheMisses);
        public void ResetStats() { _cacheHits = 0; _cacheMisses = 0; }
    }

    /// <summary>
    /// Demonstrates ValueTask for non-nullable fast paths.
    /// </summary>
    private static ValueTask<string> GetChannelNameAsync(string? channel)
    {
        // Synchronous fast path — no allocation.
        if (channel is not null)
            return ValueTask.FromResult(channel);

        // Async slow path — only when needed.
        return ResolveChannelAsync();

        static async ValueTask<string> ResolveChannelAsync()
        {
            await Task.Delay(30);
            return "general"; // Default channel
        }
    }

    /// <summary>
    /// Non-generic ValueTask for fire-and-forget style operations.
    /// </summary>
    private static ValueTask LogMessageAsync(ChatMessage message, bool synchronousMode)
    {
        if (synchronousMode)
        {
            // Synchronous path — returns completed ValueTask.
            Console.WriteLine($"    [Sync Log] {message}");
            return ValueTask.CompletedTask;
        }

        // Async path
        return SlowLogAsync(message);

        static async ValueTask SlowLogAsync(ChatMessage msg)
        {
            await Task.Delay(20);
            Console.WriteLine($"    [Async Log] {msg}");
        }
    }

    public static async Task RunAsync()
    {
        Console.WriteLine("╔═══ ValueTask<T> ═══╗\n");

        var cache = new MessageCache();

        // ── 1. Seed the cache ────────────────────────────────
        Console.WriteLine("── Seeding Cache ──");
        var seeded = new[]
        {
            ChatMessage.Create("Alice", "Cached message 1", "general"),
            ChatMessage.Create("Bob", "Cached message 2", "general"),
            ChatMessage.Create("Charlie", "Cached message 3", "random"),
        };
        foreach (var msg in seeded)
        {
            cache.Seed(msg);
            Console.WriteLine($"  Seeded: {msg.Id} → {msg.Sender}");
        }

        // ── 2. Cache hits (synchronous — zero allocation) ────
        Console.WriteLine("\n── Cache Hits (ValueTask — Zero Allocation) ──");
        cache.ResetStats();
        for (int i = 0; i < 3; i++)
        {
            // These calls complete synchronously — no Task allocated!
            var hit = await cache.GetMessageAsync(seeded[i].Id);
            Console.WriteLine($"  Hit: {hit}");
        }
        Console.WriteLine($"  Stats: {cache.Stats.Hits} hits, {cache.Stats.Misses} misses");

        // ── 3. Cache miss (async — allocates only when needed) ─
        Console.WriteLine("\n── Cache Miss (Async Path) ──");
        cache.ResetStats();
        var missed = await cache.GetMessageAsync(Guid.NewGuid());
        Console.WriteLine($"  Fetched: {missed}");
        Console.WriteLine($"  Stats: {cache.Stats.Hits} hits, {cache.Stats.Misses} misses");

        // ── 4. Mixed workload ────────────────────────────────
        Console.WriteLine("\n── Mixed Workload ──");
        cache.ResetStats();
        var ids = seeded.Select(m => m.Id)
            .Concat(Enumerable.Range(0, 3).Select(_ => Guid.NewGuid()))
            .ToArray();

        foreach (var id in ids)
        {
            var result = await cache.GetMessageAsync(id);
            Console.WriteLine($"  {(cache.Stats.Misses > 0 ? "MISS" : " HIT")}: {result?.Sender ?? "(null)"}");
        }
        Console.WriteLine($"  Final stats: {cache.Stats.Hits} hits, {cache.Stats.Misses} misses");

        // ── 5. Channel name resolution ───────────────────────
        Console.WriteLine("\n── Fast Path Resolution ──");
        var ch1 = await GetChannelNameAsync("dev");
        var ch2 = await GetChannelNameAsync(null);
        Console.WriteLine($"  Known channel:   {ch1}");
        Console.WriteLine($"  Default channel: {ch2}");

        // ── 6. Non-generic ValueTask ─────────────────────────
        Console.WriteLine("\n── Non-Generic ValueTask ──");
        var logMsg = ChatMessage.Create("Alice", "Test log", "general");
        await LogMessageAsync(logMsg, synchronousMode: true);
        await LogMessageAsync(logMsg, synchronousMode: false);

        // ── 7. Rules and gotchas ─────────────────────────────
        Console.WriteLine("\n── ValueTask Rules ──");
        Console.WriteLine("  ✓ Await exactly once");
        Console.WriteLine("  ✓ Don't .Result while incomplete");
        Console.WriteLine("  ✓ Use .AsTask() if you need to store/await multiple times");
        Console.WriteLine("  ✓ Use for methods that often complete synchronously");
        Console.WriteLine("  ✗ Don't use for methods that always go async");
        Console.WriteLine("  ✗ Don't await the same ValueTask twice");
    }
}
