namespace PerfBench.Theme6_Memory;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Benchmarks: ArrayPool<T> vs heap allocation
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  Run:  dotnet run -c Release -- --bench --filter *ArrayPool*
//
//  Measures the allocation savings of ArrayPool<T>.Shared.Rent
//  versus allocating a fresh double[] for each batch of sensor
//  readings.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[MemoryDiagnoser]
public class ArrayPoolBenchmarks
{
    private SensorReading[] readings = null!;

    [Params(100, 1000)]
    public int BatchSize { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        readings = new SensorReading[BatchSize];
        for (int i = 0; i < BatchSize; i++)
        {
            readings[i] = SensorReading.Create(
                sensorId: i % 10,
                value: 20.0 + Random.Shared.NextDouble() * 80.0,
                status: SensorStatus.Normal);
        }
    }

    [Benchmark(Baseline = true, Description = "new double[] (heap)")]
    public double HeapAllocation()
    {
        var buffer = new double[readings.Length];
        for (int i = 0; i < readings.Length; i++)
            buffer[i] = readings[i].Value;

        double sum = 0;
        foreach (double v in buffer)
            sum += v;
        return sum / buffer.Length;
    }

    [Benchmark(Description = "ArrayPool<T>.Rent (pooled)")]
    public double PooledAllocation()
    {
        double[] buffer = ArrayPool<double>.Shared.Rent(readings.Length);
        try
        {
            for (int i = 0; i < readings.Length; i++)
                buffer[i] = readings[i].Value;

            double sum = 0;
            for (int i = 0; i < readings.Length; i++)
                sum += buffer[i];
            return sum / readings.Length;
        }
        finally
        {
            ArrayPool<double>.Shared.Return(buffer);
        }
    }
}
