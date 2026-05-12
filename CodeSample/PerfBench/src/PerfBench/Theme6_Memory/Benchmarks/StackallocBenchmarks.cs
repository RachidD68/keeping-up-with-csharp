namespace PerfBench.Theme6_Memory;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Benchmarks: stackalloc vs heap-allocated scratch buffers
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  Run:  dotnet run -c Release -- --bench --filter *Stackalloc*
//
//  Shows that stackalloc eliminates all GC pressure for small
//  temporary buffers.  Buffer sizes are kept small (≤128) to
//  avoid stack overflow under BenchmarkDotNet's iteration count.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[MemoryDiagnoser]
public class StackallocBenchmarks
{
    private Pixel[] pixels = null!;

    [Params(16, 64)]
    public int PixelCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        pixels = new Pixel[PixelCount];
        for (int i = 0; i < pixels.Length; i++)
        {
            byte v = (byte)(i * (256 / PixelCount));
            pixels[i] = new Pixel(v, v, v);
        }
    }

    [Benchmark(Baseline = true, Description = "new float[] (heap scratch)")]
    public float HeapScratchBuffer()
    {
        var scratch = new float[pixels.Length];
        for (int i = 0; i < pixels.Length; i++)
            scratch[i] = pixels[i].Brightness;

        float sum = 0f;
        foreach (float b in scratch)
            sum += b;
        return sum / scratch.Length;
    }

    [Benchmark(Description = "stackalloc float[] (stack scratch)")]
    public float StackScratchBuffer()
    {
        Span<float> scratch = stackalloc float[pixels.Length];
        for (int i = 0; i < pixels.Length; i++)
            scratch[i] = pixels[i].Brightness;

        float sum = 0f;
        foreach (float b in scratch)
            sum += b;
        return sum / scratch.Length;
    }

    [Benchmark(Description = "Conditional stack-or-pool")]
    public float ConditionalStackOrPool()
    {
        float[]? rented = null;
        Span<float> scratch = pixels.Length <= 128
            ? stackalloc float[pixels.Length]
            : (rented = ArrayPool<float>.Shared.Rent(pixels.Length));

        try
        {
            for (int i = 0; i < pixels.Length; i++)
                scratch[i] = pixels[i].Brightness;

            float sum = 0f;
            foreach (float b in scratch[..pixels.Length])
                sum += b;
            return sum / pixels.Length;
        }
        finally
        {
            if (rented is not null)
                ArrayPool<float>.Shared.Return(rented);
        }
    }
}
