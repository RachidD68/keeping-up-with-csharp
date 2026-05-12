namespace PerfBench.Models;

// ╔══════════════════════════════════════════════════════════╗
// ║  PacketHeader — a fixed-layout struct for native        ║
// ║  interop scenarios (P/Invoke, memory-mapped files).     ║
// ╚══════════════════════════════════════════════════════════╝

/// <summary>
/// A network packet header — explicit layout for binary protocols.
/// Demonstrates StructLayout, FieldOffset, and fixed-size buffers.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct PacketHeader
{
    [FieldOffset(0)] public byte Version;
    [FieldOffset(1)] public byte Type;
    [FieldOffset(2)] public ushort Length;
    [FieldOffset(4)] public uint SequenceNumber;
    [FieldOffset(8)] public ulong Timestamp;

    /// <summary>Creates a standard header.</summary>
    public static PacketHeader Create(byte type, ushort length, uint sequenceNumber) =>
        new()
        {
            Version = 1,
            Type = type,
            Length = length,
            SequenceNumber = sequenceNumber,
            Timestamp = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

    /// <summary>Size matches the explicit layout.</summary>
    public static int SizeInBytes => 16;

    public override readonly string ToString() =>
        $"Packet(v{Version}, type={Type}, len={Length}, seq={SequenceNumber})";
}

/// <summary>Common packet type constants.</summary>
public static class PacketTypes
{
    public const byte Data = 0x01;
    public const byte Ack = 0x02;
    public const byte Heartbeat = 0x03;
    public const byte Close = 0xFF;
}
