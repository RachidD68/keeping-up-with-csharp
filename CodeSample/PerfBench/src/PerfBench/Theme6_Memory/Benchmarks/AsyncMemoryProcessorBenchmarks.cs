using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;

namespace PerfBench.Theme6_Memory;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Benchmarks: Async Memory<T> pipelines & pooling trade-offs
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  Run:  dotnet run -c Release -- --bench --filter *AsyncMemoryProcessor*
//  Category filters:
//         --anyCategories MemoryVsSpan
//         --anyCategories Pooling
//         --anyCategories Async
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[MemoryDiagnoser]
[CategoriesColumn]
[SimpleJob(RuntimeMoniker.HostProcess, launchCount: 1, warmupCount: 5, iterationCount: 10)]
public class AsyncMemoryProcessorBenchmarks
{
    private Pixel[] image = Array.Empty<Pixel>();
    private Memory<Pixel> imageMemory = Memory<Pixel>.Empty;
    private SensorReading[] sensorReadings = Array.Empty<SensorReading>();
    private byte[] payload = Array.Empty<byte>();

    [Params(4, 64, 512)]
    public int ImageWidth { get; set; }

    [Params(10, 1_000)]
    public int SensorBatchSize { get; set; }

    [Params(256, 4_096)]
    public int PoolBufferSize { get; set; }

    [Params(1_000)]
    public int AllocationIterations { get; set; }

    private int PixelCount => ImageWidth * ImageWidth;

    [GlobalSetup]
    public void Setup()
    {
        image = new Pixel[PixelCount];
        for (int i = 0; i < image.Length; i++)
        {
            byte v = (byte)((i * 17) & 0xFF);
            image[i] = new Pixel(v, v, v);
        }
        imageMemory = image.AsMemory();

        sensorReadings = new SensorReading[SensorBatchSize];
        for (int i = 0; i < sensorReadings.Length; i++)
        {
            sensorReadings[i] = SensorReading.Create(
                sensorId: i % 16,
                value: 20.0 + Random.Shared.NextDouble() * 80.0,
                status: SensorStatus.Normal);
        }

        payload = new byte[PoolBufferSize];
        Random.Shared.NextBytes(payload);
    }

    // ── 1. Span<T> vs Memory<T> across awaits ──────────────

    [Benchmark(Baseline = true, Description = "Span<T> sync brightness")]
    [BenchmarkCategory("MemoryVsSpan")]
    public float SpanPixelAverage()
    {
        ReadOnlySpan<Pixel> span = image;
        return ComputeAverageBrightness(span);
    }

    [Benchmark(Description = "Memory<T> async brightness")]
    [BenchmarkCategory("MemoryVsSpan", "Async")]
    public async Task<float> MemoryPixelAverageAsync()
    {
        await Task.Yield();
        float avg = ComputeAverageBrightness(imageMemory.Span);
        await Task.Yield();
        return avg;
    }

    // ── 2. Pooled async sensor processing ──────────────────

    [Benchmark(Description = "ArrayPool<double> batch (async)")]
    [BenchmarkCategory("Pooling", "Async")]
    public async Task<double> SensorBatchArrayPoolAsync()
    {
        double[] buffer = ArrayPool<double>.Shared.Rent(SensorBatchSize);
        try
        {
            LoadSensorValues(buffer.AsSpan(0, SensorBatchSize));
            await Task.Yield();
            return Average(buffer.AsSpan(0, SensorBatchSize));
        }
        finally
        {
            ArrayPool<double>.Shared.Return(buffer);
        }
    }

    [Benchmark(Description = "MemoryPool<double> batch (async)")]
    [BenchmarkCategory("Pooling", "Async")]
    public async Task<double> SensorBatchMemoryPoolAsync()
    {
        using IMemoryOwner<double> owner = MemoryPool<double>.Shared.Rent(SensorBatchSize);
        Memory<double> batch = owner.Memory;
        LoadSensorValues(batch.Span[..SensorBatchSize]);
        await Task.Yield();
        return Average(batch.Span[..SensorBatchSize]);
    }

    // ── 3. Allocation loops: heap vs pools ─────────────────

    [Benchmark(Baseline = true, Description = "new byte[] allocation loop")]
    [BenchmarkCategory("Pooling")]
    public long RawAllocationLoop()
    {
        long checksum = 0;
        for (int i = 0; i < AllocationIterations; i++)
        {
            var buffer = new byte[PoolBufferSize];
            checksum += CopyPayload(buffer);
        }
        return checksum;
    }

    [Benchmark(Description = "ArrayPool<byte>.Shared loop")]
    [BenchmarkCategory("Pooling")]
    public long ArrayPoolLoop()
    {
        long checksum = 0;
        for (int i = 0; i < AllocationIterations; i++)
        {
            byte[] buffer = ArrayPool<byte>.Shared.Rent(PoolBufferSize);
            try
            {
                Span<byte> span = buffer.AsSpan(0, PoolBufferSize);
                checksum += CopyPayload(span);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
        return checksum;
    }

    [Benchmark(Description = "MemoryPool<byte>.Shared loop")]
    [BenchmarkCategory("Pooling")]
    public long MemoryPoolLoop()
    {
        long checksum = 0;
        for (int i = 0; i < AllocationIterations; i++)
        {
            using IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent(PoolBufferSize);
            Span<byte> span = owner.Memory.Span[..PoolBufferSize];
            checksum += CopyPayload(span);
        }
        return checksum;
    }

    // ── Helpers ─────────────────────────────────────────────

    private static float ComputeAverageBrightness(ReadOnlySpan<Pixel> span)
    {
        if (span.IsEmpty)
            return 0f;

        float sum = 0f;
        foreach (ref readonly var pixel in span)
            sum += pixel.Brightness;
        return sum / span.Length;
    }

    private void LoadSensorValues(Span<double> destination)
    {
        for (int i = 0; i < destination.Length; i++)
            destination[i] = sensorReadings[i % sensorReadings.Length].Value;
    }

    private static double Average(ReadOnlySpan<double> values)
    {
        double sum = 0d;
        for (int i = 0; i < values.Length; i++)
            sum += values[i];
        return values.Length == 0 ? 0d : sum / values.Length;
    }

    private long CopyPayload(Span<byte> destination)
    {
        payload.AsSpan().CopyTo(destination);
        return destination[0];
    }
}
