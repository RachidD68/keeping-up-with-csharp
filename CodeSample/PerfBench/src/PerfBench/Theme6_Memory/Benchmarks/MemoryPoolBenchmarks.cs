namespace PerfBench.Theme6_Memory;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Benchmarks: MemoryPool<T> vs ArrayPool<T> vs raw allocation
//  + async Memory<T> vs sync Span<T> overhead
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  Run:  dotnet run -c Release -- --bench --filter *MemoryPool*
//
//  Three-way comparison of buffer acquisition strategies plus
//  the cost of async-compatible Memory<T> vs stack-only Span<T>.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[MemoryDiagnoser]
public class MemoryPoolBenchmarks
{
    private byte[] sourceData = null!;

    [Params(256, 4096)]
    public int BufferSize { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        sourceData = new byte[BufferSize];
        Random.Shared.NextBytes(sourceData);
    }

    // ── Three-way pool comparison ────────────────────────────

    [Benchmark(Baseline = true, Description = "new byte[] (heap)")]
    public byte RawAllocation()
    {
        var buffer = new byte[BufferSize];
        sourceData.AsSpan().CopyTo(buffer);
        return buffer[0];
    }

    [Benchmark(Description = "ArrayPool<T>.Rent")]
    public byte ArrayPoolRent()
    {
        byte[] buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
        try
        {
            sourceData.AsSpan().CopyTo(buffer);
            return buffer[0];
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    [Benchmark(Description = "MemoryPool<T>.Rent")]
    public byte MemoryPoolRent()
    {
        using IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent(BufferSize);
        sourceData.AsSpan().CopyTo(owner.Memory.Span);
        return owner.Memory.Span[0];
    }

    // ── Async overhead: Memory<T> vs sync Span<T> ────────────

    [Benchmark(Description = "Sync Span<T> sum")]
    public long SyncSpanSum()
    {
        ReadOnlySpan<byte> span = sourceData;
        long sum = 0;
        foreach (byte b in span)
            sum += b;
        return sum;
    }

    [Benchmark(Description = "Async Memory<T> sum")]
    public Task<long> AsyncMemorySum()
    {
        return SumAsync(sourceData.AsMemory());
    }

    private static async Task<long> SumAsync(ReadOnlyMemory<byte> memory)
    {
        await Task.Yield(); // force async state machine
        ReadOnlySpan<byte> span = memory.Span;
        long sum = 0;
        foreach (byte b in span)
            sum += b;
        return sum;
    }
}
