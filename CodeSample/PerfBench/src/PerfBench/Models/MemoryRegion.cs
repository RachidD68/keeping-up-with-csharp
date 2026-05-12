namespace PerfBench.Models;

// ╔══════════════════════════════════════════════════════════╗
// ║  MemoryRegion — tracks pooled memory for diagnostics    ║
// ║  and demonstrates IMemoryOwner<T> patterns.             ║
// ╚══════════════════════════════════════════════════════════╝

/// <summary>
/// Tracks memory allocation for diagnostics.
/// </summary>
public record MemoryRegion(
    string Name,
    int RequestedBytes,
    int AllocatedBytes,
    bool IsPooled,
    DateTimeOffset AllocatedAt)
{
    /// <summary>Wasted bytes due to pool over-allocation.</summary>
    public int WastedBytes => AllocatedBytes - RequestedBytes;

    /// <summary>Utilization percentage.</summary>
    public double Utilization =>
        AllocatedBytes > 0
            ? (double)RequestedBytes / AllocatedBytes * 100.0
            : 0;

    public override string ToString() =>
        $"{Name}: {RequestedBytes:N0}/{AllocatedBytes:N0} bytes " +
        $"({Utilization:F1}% util, {(IsPooled ? "pooled" : "heap")})";
}

/// <summary>
/// Simple allocation tracker for demonstration.
/// </summary>
public sealed class AllocationTracker
{
    private readonly List<MemoryRegion> _regions = [];
    private int _totalAllocated;
    private int _totalRequested;

    public void Track(string name, int requested, int allocated, bool pooled)
    {
        _regions.Add(new MemoryRegion(
            name, requested, allocated, pooled, DateTimeOffset.UtcNow));
        _totalAllocated += allocated;
        _totalRequested += requested;
    }

    public int TotalAllocated => _totalAllocated;
    public int TotalRequested => _totalRequested;
    public int TotalWasted => _totalAllocated - _totalRequested;
    public IReadOnlyList<MemoryRegion> Regions => _regions;

    public void PrintSummary()
    {
        Console.WriteLine($"  Requested: {_totalRequested:N0} bytes");
        Console.WriteLine($"  Allocated: {_totalAllocated:N0} bytes");
        Console.WriteLine($"  Wasted:    {TotalWasted:N0} bytes");
        Console.WriteLine($"  Regions:   {_regions.Count}");
    }
}
