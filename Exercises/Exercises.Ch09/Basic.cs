// Chapter 9 — Asynchronous Programming Evolution — BASIC
// ----------------------------------------------------------------
// Exercise: Take ChatStream's GetMessageTaskAsync (returns
//   Task<ChatMessage?>) and convert it to a version returning
//   ValueTask<ChatMessage?>, with no allocation on cache hit. Run
//   the cache-hit scenario with dotnet-counters monitor System.Runtime
//   and observe the Gen 0 GC Count before and after the change.
//
// Hint: Split into a sync method returning ValueTask.FromResult(...)
//   on hit, and a private async ValueTask helper on miss.
// ----------------------------------------------------------------

namespace KeepUpCs.Exercises.Ch09;

public record ChatMessage(Guid Id, string Author, string Body);

public sealed class MessageCache
{
    private readonly Dictionary<Guid, ChatMessage> _cache = new();
    public int CacheHits;
    public int CacheMisses;

    // ── BEFORE — Task<T> allocates on every call, even cache hits ─
    public async Task<ChatMessage?> GetMessageTaskAsync(Guid id)
    {
        if (_cache.TryGetValue(id, out var cached))
        {
            CacheHits++;
            return cached;  // allocates a Task<ChatMessage?> just to wrap the value
        }

        CacheMisses++;
        await Task.Delay(10);
        var msg = new ChatMessage(id, "system", $"fetched {id}");
        _cache[id] = msg;
        return msg;
    }

    // ── AFTER — ValueTask<T> avoids allocation on the synchronous path ─
    public ValueTask<ChatMessage?> GetMessageValueTaskAsync(Guid id)
    {
        if (_cache.TryGetValue(id, out var cached))
        {
            CacheHits++;
            return ValueTask.FromResult<ChatMessage?>(cached);
        }
        return FetchAndCacheAsync(id);
    }

    private async ValueTask<ChatMessage?> FetchAndCacheAsync(Guid id)
    {
        CacheMisses++;
        await Task.Delay(10);
        var msg = new ChatMessage(id, "system", $"fetched {id}");
        _cache[id] = msg;
        return msg;
    }
}

public static class BasicDemo
{
    public static async Task RunAsync()
    {
        Console.WriteLine("Ch09 Basic — Task<T> → ValueTask<T> on cache hit");
        Console.WriteLine(new string('─', 60));

        var cache = new MessageCache();
        var id = Guid.NewGuid();

        // Prime the cache via the async path.
        await cache.GetMessageValueTaskAsync(id);
        cache.CacheHits = 0;
        cache.CacheMisses = 0;

        const int Iter = 100_000;

        long beforeTask = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < Iter; i++) _ = await cache.GetMessageTaskAsync(id);
        long taskBytes = GC.GetAllocatedBytesForCurrentThread() - beforeTask;

        long beforeValue = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < Iter; i++) _ = await cache.GetMessageValueTaskAsync(id);
        long valueBytes = GC.GetAllocatedBytesForCurrentThread() - beforeValue;

        Console.WriteLine($"  Cache hits over {Iter:N0} cache-hit iterations:");
        Console.WriteLine($"    Task<T>:      {taskBytes,15:N0} bytes allocated");
        Console.WriteLine($"    ValueTask<T>: {valueBytes,15:N0} bytes allocated");
        var saved = taskBytes - valueBytes;
        if (taskBytes > 0)
            Console.WriteLine($"    reduction:    {saved,15:N0} bytes ({(double)saved / taskBytes:P1})");
    }
}
