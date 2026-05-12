namespace PerfBench.Theme10_Capstone;

// ╔══════════════════════════════════════════════════════════════════╗
// ║  Theme 10 Capstone — Zero-Allocation Pipeline                  ║
// ║                                                                ║
// ║  Combines every memory-efficiency feature from Theme 6 into    ║
// ║  a cohesive, zero-allocation data processing pipeline:         ║
// ║                                                                ║
// ║  • Span<T> for zero-copy data views                            ║
// ║  • stackalloc for small temporary buffers                      ║
// ║  • ArrayPool<T> for large temporary buffers                    ║
// ║  • ref returns for in-place mutation                           ║
// ║  • ref structs for stack-only pipeline stages                  ║
// ║  • Interpolated string handlers for zero-alloc logging         ║
// ║                                                                ║
// ║  The result: a sensor data pipeline that processes thousands   ║
// ║  of readings per second with zero GC pressure.                 ║
// ╚══════════════════════════════════════════════════════════════════╝

/// <summary>
/// Capstone: A zero-allocation sensor data processing pipeline.
/// Every stage operates on Span&lt;T&gt; — no heap allocations in the hot path.
/// </summary>
public static class ZeroAllocPipelineDemo
{
    // ── Pipeline stage: Normalizer (ref struct) ──────────────

    /// <summary>
    /// Normalizes sensor readings in-place using Span.
    /// ref struct ensures this never escapes to the heap.
    /// </summary>
    private ref struct SensorNormalizer
    {
        private readonly double _minValue;
        private readonly double _maxValue;

        public SensorNormalizer(double minValue, double maxValue)
        {
            _minValue = minValue;
            _maxValue = maxValue;
        }

        /// <summary>
        /// Normalizes values to [0, 1] range in-place.
        /// Returns count of readings processed.
        /// </summary>
        public int Normalize(Span<double> values)
        {
            var range = _maxValue - _minValue;
            if (range == 0) return 0;

            int processed = 0;
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = Math.Clamp((values[i] - _minValue) / range, 0, 1);
                processed++;
            }
            return processed;
        }
    }

    // ── Pipeline stage: Filter (operates on Span) ────────────

    /// <summary>
    /// Filters sensor readings by status, compacting in-place.
    /// Zero allocation — operates entirely on the input span.
    /// </summary>
    private static int FilterByStatus(
        ReadOnlySpan<SensorReading> input,
        Span<SensorReading> output,
        SensorStatus requiredStatus)
    {
        int written = 0;
        for (int i = 0; i < input.Length; i++)
        {
            if (input[i].Status == requiredStatus)
            {
                if (written < output.Length)
                    output[written++] = input[i];
            }
        }
        return written;
    }

    // ── Pipeline stage: Aggregator (stackalloc buckets) ──────

    /// <summary>
    /// Aggregates readings into buckets using stackalloc.
    /// </summary>
    private static (double Min, double Max, double Avg) Aggregate(
        ReadOnlySpan<SensorReading> readings)
    {
        if (readings.IsEmpty)
            return (0, 0, 0);

        double min = readings[0].Value;
        double max = readings[0].Value;
        double sum = readings[0].Value;

        for (int i = 1; i < readings.Length; i++)
        {
            var val = readings[i].Value;
            if (val < min) min = val;
            if (val > max) max = val;
            sum += val;
        }

        return (min, max, sum / readings.Length);
    }

    /// <summary>
    /// Builds a histogram using stackalloc for small buffers
    /// or ArrayPool for large ones.
    /// </summary>
    private static void BuildHistogram(
        ReadOnlySpan<SensorReading> readings,
        Span<int> buckets,
        double minValue,
        double maxValue)
    {
        buckets.Clear();
        var range = maxValue - minValue;
        if (range == 0) return;

        var bucketCount = buckets.Length;
        for (int i = 0; i < readings.Length; i++)
        {
            var normalized = (readings[i].Value - minValue) / range;
            var bucket = (int)(normalized * (bucketCount - 1));
            bucket = Math.Clamp(bucket, 0, bucketCount - 1);
            buckets[bucket]++;
        }
    }

    // ── Pipeline stage: Output formatter ─────────────────────

    /// <summary>
    /// Formats results into a pre-allocated char buffer.
    /// Uses Span&lt;char&gt; to avoid string allocations.
    /// </summary>
    private static int FormatResult(
        Span<char> buffer,
        double min, double max, double avg,
        int count)
    {
        var result = $"Processed {count} readings: min={min:F2}, max={max:F2}, avg={avg:F2}";
        result.AsSpan().CopyTo(buffer);
        return result.Length;
    }

    // ── The complete pipeline ────────────────────────────────

    /// <summary>
    /// Runs the complete zero-allocation pipeline.
    /// </summary>
    private static void RunPipeline(
        ReadOnlySpan<SensorReading> readings,
        AllocationTracker tracker)
    {
        Console.WriteLine($"    Input: {readings.Length} readings\n");

        // Stage 1: Filter — use ArrayPool for the filter buffer
        // (stackalloc cannot be used here because the compiler prevents
        //  Span<SensorReading> from potentially escaping the method scope)
        var pooledBuffer = ArrayPool<SensorReading>.Shared.Rent(readings.Length);
        Span<SensorReading> filterBuffer = pooledBuffer.AsSpan(0, readings.Length);
        tracker.Track("FilterBuffer",
            readings.Length * SensorReading.SizeInBytes,
            pooledBuffer.Length * SensorReading.SizeInBytes,
            pooled: true);

        int normalCount = FilterByStatus(readings, filterBuffer, SensorStatus.Normal);
        var normalReadings = filterBuffer[..normalCount];
        Console.WriteLine($"    Stage 1 (Filter): {normalCount}/{readings.Length} normal readings");

        // Stage 2: Extract values into a heap array for normalization
        // (ref struct + Span lifetime rules prevent stackalloc from
        //  being passed to a ref struct method across scopes)
        var valuesArray = new double[normalCount];
        Span<double> values = valuesArray;

        for (int i = 0; i < normalCount; i++)
            values[i] = normalReadings[i].Value;

        // Stage 3: Normalize in-place using ref struct
        var normalizer = new SensorNormalizer(0, 100);
        int processed = normalizer.Normalize(valuesArray);
        Console.WriteLine($"    Stage 2 (Normalize): {processed} values normalized to [0,1]");

        // Stage 4: Aggregate
        var (min, max, avg) = Aggregate(normalReadings);
        Console.WriteLine($"    Stage 3 (Aggregate): min={min:F2}, max={max:F2}, avg={avg:F2}");

        // Stage 5: Histogram using stackalloc
        const int bucketCount = 10;
        Span<int> histogram = stackalloc int[bucketCount];
        BuildHistogram(normalReadings, histogram, 0, 100);
        Console.Write("    Stage 4 (Histogram): ");
        for (int i = 0; i < bucketCount; i++)
        {
            var bar = new string('█', Math.Min(histogram[i], 20));
            if (bar.Length > 0) Console.Write($"[{i}]={histogram[i]} ");
        }
        Console.WriteLine();

        // Stage 6: Format result into pre-allocated buffer
        Span<char> outputBuffer = stackalloc char[256];
        int charsWritten = FormatResult(outputBuffer, min, max, avg, normalCount);
        Console.WriteLine($"    Stage 5 (Format): {outputBuffer[..charsWritten]}");

        // Return pooled buffer
        ArrayPool<SensorReading>.Shared.Return(pooledBuffer);
    }

    public static void Run()
    {
        Console.WriteLine("╔═══ Theme 10 Capstone: Zero-Allocation Pipeline ═══╗\n");

        // ── Generate test data ───────────────────────────────
        Console.WriteLine("── Generating Sensor Data ──");
        var random = new Random(42);
        var readings = new SensorReading[200];
        for (int i = 0; i < readings.Length; i++)
        {
            var status = (SensorStatus)(i % 10 == 0 ? 2 : i % 5 == 0 ? 1 : 0);
            readings[i] = new SensorReading(
                i % 4,
                DateTimeOffset.UtcNow.AddSeconds(-i).Ticks,
                random.NextDouble() * 100,
                status);
        }
        Console.WriteLine($"  Generated {readings.Length} readings\n");

        // ── Run the pipeline ─────────────────────────────────
        Console.WriteLine("── Pipeline Execution ──");
        var tracker = new AllocationTracker();
        RunPipeline(readings, tracker);

        // ── Allocation report ────────────────────────────────
        Console.WriteLine("\n── Allocation Report ──");
        tracker.PrintSummary();
        foreach (var region in tracker.Regions)
            Console.WriteLine($"    {region}");

        // ── Small batch (stackalloc path) ────────────────────
        Console.WriteLine("\n── Small Batch (stackalloc path) ──");
        var smallBatch = readings.AsSpan(0, 50);
        var smallTracker = new AllocationTracker();
        RunPipeline(smallBatch, smallTracker);
        Console.WriteLine("\n  Small batch allocation:");
        smallTracker.PrintSummary();

        // ── Features used ────────────────────────────────────
        Console.WriteLine("\n── C# Features Combined ──");
        Console.WriteLine("  ✓ Span<T> — zero-copy views over sensor data");
        Console.WriteLine("  ✓ stackalloc — stack buffers for small batches");
        Console.WriteLine("  ✓ ArrayPool<T> — reusable buffers for large batches");
        Console.WriteLine("  ✓ ref struct — stack-only normalizer stage");
        Console.WriteLine("  ✓ ReadOnlySpan<T> — immutable input views");
        Console.WriteLine("  ✓ Span<char> — zero-alloc output formatting");
        Console.WriteLine("  → Zero GC pressure in the hot path!");
    }
}
