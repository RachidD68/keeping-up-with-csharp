namespace PerfBench.Theme6_Memory;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: Interpolated String Handlers  (C# 10)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  PROBLEM
//  -------
//  `$"Sensor {id}: {value:F2}"` looks innocent, but the
//  compiler generates a string.Format call that:
//  1. Boxes every value type (int → object, double → object)
//  2. Allocates intermediate strings for each formatted segment
//  3. Concatenates everything into a final string
//  In logging-heavy code, this creates millions of short-lived
//  strings — even when the log level is disabled.
//
//  SOLUTION
//  --------
//  C# 10 introduced the [InterpolatedStringHandler] pattern.
//  The compiler rewrites interpolation calls to invoke Append
//  methods on a handler struct — writing directly to a buffer,
//  skipping boxing, and enabling short-circuit evaluation
//  (e.g., skip formatting entirely if log level is off).
//
//  WHY IT MATTERS
//  ──────────────
//  .NET 6's DefaultInterpolatedStringHandler already powers
//  string.Create and StringBuilder.  Custom handlers let you
//  build zero-allocation logging, formatting, and serialization
//  pipelines tailored to your domain.
//
//  TRY IT
//  ──────
//  1. Add AppendFormatted overloads for Pixel and SensorReading.
//  2. Count allocations with GC.GetAllocatedBytesForCurrentThread()
//     comparing $"..." vs handler.
//  3. Build a handler that writes directly to a Span<char>.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Demonstrates interpolated string handlers for zero-allocation formatting.
/// </summary>
public static class ZeroAllocFormatterDemo
{
    // ── Before: string interpolation boxes and allocates ─────

    /// <summary>
    /// Traditional logging — allocates even when the log is disabled.
    /// </summary>
    private static class OldLogger
    {
        public static bool IsEnabled { get; set; } = true;

        public static void Log(string message)
        {
            if (!IsEnabled) return;
            Console.WriteLine($"    [OLD-LOG] {message}");
        }
    }

    /// <summary>
    /// Old-style: the interpolated string is ALWAYS built, even if
    /// logging is disabled.  Boxing + allocation every call.
    /// </summary>
    private static void LogSensorOldWay(int id, double value, SensorStatus status)
    {
        // This string is always allocated, even if IsEnabled is false!
        OldLogger.Log($"Sensor {id}: value={value:F2}, status={status}");
    }

    // ── After: custom InterpolatedStringHandler ──────────────

    /// <summary>
    /// A custom interpolated string handler that writes to an internal
    /// buffer and can short-circuit when logging is disabled.
    /// </summary>
    [InterpolatedStringHandler]
    public ref struct LogInterpolatedStringHandler
    {
        private DefaultInterpolatedStringHandler inner;
        private readonly bool enabled;

        /// <summary>
        /// The compiler calls this constructor FIRST.  If we determine
        /// that logging is disabled, we set _enabled = false and the
        /// Append methods become no-ops — zero work, zero allocation.
        /// </summary>
        /// <param name="literalLength">Total length of literal parts.</param>
        /// <param name="formattedCount">Number of interpolation holes.</param>
        /// <param name="logger">The logger to check.</param>
        /// <param name="shouldAppend">Out: tells compiler whether to proceed.</param>
        public LogInterpolatedStringHandler(
            int literalLength,
            int formattedCount,
            SmartLogger logger,
            out bool shouldAppend)
        {
            enabled = logger.IsEnabled;
            shouldAppend = enabled;

            if (enabled)
                inner = new DefaultInterpolatedStringHandler(literalLength, formattedCount);
        }

        /// <summary>Appends a literal string segment.</summary>
        public void AppendLiteral(string s)
        {
            if (enabled) inner.AppendLiteral(s);
        }

        /// <summary>Appends a formatted value — no boxing for common types.</summary>
        public void AppendFormatted<T>(T value)
        {
            if (enabled) inner.AppendFormatted(value);
        }

        /// <summary>Appends a formatted value with a format string.</summary>
        public void AppendFormatted<T>(T value, string? format)
        {
            if (enabled) inner.AppendFormatted(value, format);
        }

        /// <summary>Appends a formatted value with alignment.</summary>
        public void AppendFormatted<T>(T value, int alignment)
        {
            if (enabled) inner.AppendFormatted(value, alignment);
        }

        /// <summary>Appends a formatted value with alignment and format.</summary>
        public void AppendFormatted<T>(T value, int alignment, string? format)
        {
            if (enabled) inner.AppendFormatted(value, alignment, format);
        }

        /// <summary>Appends a ReadOnlySpan of chars (avoids string allocation).</summary>
        public void AppendFormatted(ReadOnlySpan<char> value)
        {
            if (enabled) inner.AppendFormatted(value);
        }

        public override string ToString() =>
            enabled ? inner.ToStringAndClear() : string.Empty;
    }

    /// <summary>
    /// A smart logger that uses the custom handler to avoid
    /// allocations when logging is disabled.
    /// </summary>
    public sealed class SmartLogger
    {
        public bool IsEnabled { get; set; } = true;
        public int MessageCount { get; private set; }

        /// <summary>
        /// The compiler rewrites: logger.Log($"Sensor {id}: {value}")
        /// into calls to the handler's constructor + AppendLiteral/AppendFormatted.
        /// If IsEnabled is false, nothing is formatted.
        /// </summary>
        public void Log([InterpolatedStringHandlerArgument("")] ref LogInterpolatedStringHandler handler)
        {
            if (!IsEnabled) return;
            MessageCount++;
            Console.WriteLine($"    [SMART-LOG] {handler.ToString()}");
        }
    }

    /// <summary>
    /// Demonstrates DefaultInterpolatedStringHandler directly —
    /// the built-in handler that powers string.Create.
    /// </summary>
    private static string FormatSensorWithBuiltinHandler(SensorReading reading)
    {
        // Use the built-in handler directly for manual control
        var handler = new DefaultInterpolatedStringHandler(
            literalLength: 30,
            formattedCount: 4);

        handler.AppendLiteral("Sensor[");
        handler.AppendFormatted(reading.SensorId);
        handler.AppendLiteral("] = ");
        handler.AppendFormatted(reading.Value, "F2");
        handler.AppendLiteral(" (");
        handler.AppendFormatted(reading.Status);
        handler.AppendLiteral(")");

        return handler.ToStringAndClear();
    }

    /// <summary>
    /// Demonstrates string.Create with a span-based callback
    /// for maximum control over the output buffer.
    /// </summary>
    private static string FormatPixelWithStringCreate(Pixel pixel)
    {
        // string.Create pre-allocates the exact buffer size
        // and lets you write directly into it — one allocation total.
        return string.Create(null, stackalloc char[64],
            $"Pixel({pixel.R},{pixel.G},{pixel.B}) brightness={pixel.Brightness:F3}");
    }

    public static void Run()
    {
        Console.WriteLine("\u2554\u2550\u2550\u2550 Interpolated String Handlers \u2550\u2550\u2550\u2557\n");

        var reading = SensorReading.Create(42, 98.6, SensorStatus.Warning);
        var pixel = new Pixel(200, 100, 50);

        // ── 1. Before: always allocates ───────────────────────
        Console.WriteLine("\u2500\u2500 Before: Old-Style Interpolation \u2500\u2500");
        Console.WriteLine("  Logging enabled:");
        OldLogger.IsEnabled = true;
        LogSensorOldWay(reading.SensorId, reading.Value, reading.Status);

        Console.WriteLine("  Logging disabled (string STILL allocated):");
        long beforeDisabled = GC.GetAllocatedBytesForCurrentThread();
        OldLogger.IsEnabled = false;
        for (int i = 0; i < 1_000; i++)
            LogSensorOldWay(reading.SensorId, reading.Value, reading.Status);
        long afterDisabled = GC.GetAllocatedBytesForCurrentThread();
        Console.WriteLine($"    1,000 disabled calls allocated: ~{afterDisabled - beforeDisabled:N0} bytes (wasted!)");

        // ── 2. After: custom handler short-circuits ───────────
        Console.WriteLine("\n\u2500\u2500 After: Smart Logger with Handler \u2500\u2500");
        var logger = new SmartLogger { IsEnabled = true };

        Console.WriteLine("  Logging enabled:");
        logger.Log($"Sensor {reading.SensorId}: value={reading.Value:F2}, status={reading.Status}");

        Console.WriteLine("  Logging disabled (zero formatting work):");
        logger.IsEnabled = false;
        long beforeSmart = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1_000; i++)
            logger.Log($"Sensor {reading.SensorId}: value={reading.Value:F2}, status={reading.Status}");
        long afterSmart = GC.GetAllocatedBytesForCurrentThread();
        Console.WriteLine($"    1,000 disabled calls allocated: ~{afterSmart - beforeSmart:N0} bytes (saved!)");
        Console.WriteLine($"    Messages actually logged: {logger.MessageCount}");

        // ── 3. Re-enable and log various data ─────────────────
        Console.WriteLine("\n\u2500\u2500 Handler with Various Types \u2500\u2500");
        logger.IsEnabled = true;
        logger.Log($"Pixel: {pixel} brightness={pixel.Brightness:F3}");
        logger.Log($"Reading: Sensor[{reading.SensorId}] = {reading.Value:F2}");
        logger.Log($"Timestamp: {reading.Timestamp:HH:mm:ss.fff}");
        logger.Log($"Memory: {GC.GetTotalMemory(false):N0} bytes in use");

        // ── 4. DefaultInterpolatedStringHandler directly ──────
        Console.WriteLine("\n\u2500\u2500 DefaultInterpolatedStringHandler (Built-in) \u2500\u2500");
        string formatted = FormatSensorWithBuiltinHandler(reading);
        Console.WriteLine($"  Manual handler: {formatted}");
        Console.WriteLine("  Same mechanism the compiler uses for $\"...\" strings.");

        // ── 5. string.Create with stackalloc buffer ───────────
        Console.WriteLine("\n\u2500\u2500 string.Create with stackalloc \u2500\u2500");
        string pixelStr = FormatPixelWithStringCreate(pixel);
        Console.WriteLine($"  {pixelStr}");
        Console.WriteLine("  Single allocation — formatted directly into the string buffer.");

        // ── 6. Allocation comparison ──────────────────────────
        Console.WriteLine("\n\u2500\u2500 Allocation Comparison: 10,000 Iterations \u2500\u2500");
        const int iterations = 10_000;

        // Old way — always allocates
        long before1 = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < iterations; i++)
        {
            _ = $"Sensor {reading.SensorId}: value={reading.Value:F2}, status={reading.Status}";
        }
        long after1 = GC.GetAllocatedBytesForCurrentThread();

        // string.Create way — one precise allocation per iteration
        // Note: stackalloc is hoisted outside the loop to avoid CA2014.
        long before2 = GC.GetAllocatedBytesForCurrentThread();
        Span<char> createBuffer = stackalloc char[128];
        for (int i = 0; i < iterations; i++)
        {
            _ = string.Create(null, createBuffer,
                $"Sensor {reading.SensorId}: value={reading.Value:F2}, status={reading.Status}");
        }
        long after2 = GC.GetAllocatedBytesForCurrentThread();

        Console.WriteLine($"  Standard $\"...\": ~{after1 - before1:N0} bytes");
        Console.WriteLine($"  string.Create:   ~{after2 - before2:N0} bytes");

        // ── 7. Key takeaway ───────────────────────────────────
        Console.WriteLine("\n\u2500\u2500 Key Takeaway \u2500\u2500");
        Console.WriteLine("  [InterpolatedStringHandler] rewrites interpolation at compile time.");
        Console.WriteLine("  The handler can: short-circuit, avoid boxing, write to pooled buffers.");
        Console.WriteLine("  Use it for logging, diagnostics, and any hot-path formatting.");
    }
}
