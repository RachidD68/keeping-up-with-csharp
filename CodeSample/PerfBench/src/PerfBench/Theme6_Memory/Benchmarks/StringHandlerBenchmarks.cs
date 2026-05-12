namespace PerfBench.Theme6_Memory.Benchmarks;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Benchmarks: Interpolated String Handlers vs old-style
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  Run:  dotnet run -c Release -- --bench --filter *StringHandler*
//
//  The killer feature of [InterpolatedStringHandler] is
//  short-circuiting: when logging is disabled, zero formatting
//  work happens.  These benchmarks quantify the difference.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[MemoryDiagnoser]
public class StringHandlerBenchmarks
{
    private SensorReading reading;
    private ZeroAllocFormatterDemo.SmartLogger enabledLogger = null!;
    private ZeroAllocFormatterDemo.SmartLogger disabledLogger = null!;

    [GlobalSetup]
    public void Setup()
    {
        reading  = SensorReading.Create(42, 98.6, SensorStatus.Warning);
        enabledLogger = new ZeroAllocFormatterDemo.SmartLogger { IsEnabled = true };
        disabledLogger = new ZeroAllocFormatterDemo.SmartLogger { IsEnabled = false };
    }

    // ── Standard interpolation (always allocates) ────────────

    [Benchmark(Baseline = true, Description = "Standard $\"...\" interpolation")]
    public string StandardInterpolation() => $"Sensor {reading.SensorId}: value={reading.Value:F2}, status={reading.Status}";

    [Benchmark(Description = "string.Create with stackalloc")]
    public string StringCreateInterpolation()
    {
        return string.Create(null, stackalloc char[128],
            $"Sensor {reading.SensorId}: value={reading.Value:F2}, status={reading.Status}");
    }

    // ── Handler: logging enabled vs disabled ─────────────────

    [Benchmark(Description = "Old-style log (disabled, still allocates)")]
    public string OldStyleLogDisabled() =>
        // The string is always built, even though we discard it
        $"Sensor {reading.SensorId}: value={reading.Value:F2}, status={reading.Status}";

    [Benchmark(Description = "Handler log (disabled, zero work)")]
    public void HandlerLogDisabled() =>
        // The handler short-circuits — no formatting, no allocation
        disabledLogger.Log($"Sensor {reading.SensorId}: value={reading.Value:F2}, status={reading.Status}");

    [Benchmark(Description = "Handler log (enabled)")]
    public void HandlerLogEnabled() => enabledLogger.Log($"Sensor {reading.SensorId}: value={reading.Value:F2}, status={reading.Status}");
}
