namespace PerfBench.Models;

// ╔══════════════════════════════════════════════════════════╗
// ║  SensorReading — an unmanaged blittable struct for      ║
// ║  demonstrating fixed-size buffers, Span<T>, and         ║
// ║  zero-copy data processing.                             ║
// ╚══════════════════════════════════════════════════════════╝

/// <summary>
/// Sensor data — unmanaged, fixed layout for interop.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly record struct SensorReading(
    int SensorId,
    long TimestampTicks,
    double Value,
    SensorStatus Status)
{
    /// <summary>The timestamp as DateTimeOffset.</summary>
    public DateTimeOffset Timestamp =>
        new(TimestampTicks, TimeSpan.Zero);

    /// <summary>Creates a reading for "now".</summary>
    public static SensorReading Create(int sensorId, double value, SensorStatus status = SensorStatus.Normal) =>
        new(sensorId, DateTimeOffset.UtcNow.Ticks, value, status);

    /// <summary>Size in bytes (for buffer calculations).</summary>
    public static int SizeInBytes => Unsafe.SizeOf<SensorReading>();

    public override string ToString() =>
        $"Sensor[{SensorId}] = {Value:F2} ({Status}) @ {Timestamp:HH:mm:ss.fff}";
}

/// <summary>Sensor status flags.</summary>
public enum SensorStatus : byte
{
    Normal = 0,
    Warning = 1,
    Critical = 2,
    Offline = 3
}
