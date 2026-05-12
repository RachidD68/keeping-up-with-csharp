// Chapter 8 — Low-Level & Interop Primitives — INTERMEDIATE
// ----------------------------------------------------------------
// Exercise: Extend the PacketHeader struct with a Checksum field
//   of type ushort at offset 14. Verify that Unsafe.SizeOf<PacketHeader>()
//   still reports 16 and that round-trip serialization still works.
//   Write a helper ushort ComputeChecksum(ReadOnlySpan<byte> packet)
//   that XORs every byte of the header except the checksum field
//   itself.
//
// Hint: Look at FieldOffset(14) positioning and make sure no field
//   overlaps.
// ----------------------------------------------------------------

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace KeepUpCs.Exercises.Ch08;

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct PacketHeaderV2
{
    [FieldOffset(0)]  public byte   Version;
    [FieldOffset(1)]  public byte   Type;
    [FieldOffset(2)]  public ushort Length;
    [FieldOffset(4)]  public uint   SequenceNumber;
    [FieldOffset(8)]  public uint   Timestamp;        // shrunk from ulong → uint
    [FieldOffset(12)] public ushort Reserved;         // padding so checksum is at 14
    [FieldOffset(14)] public ushort Checksum;        // new field — fits in last 2 bytes

    public override readonly string ToString() =>
        $"Packet v{Version} type={Type} len={Length} seq={SequenceNumber} ts={Timestamp} checksum=0x{Checksum:X4}";
}

public static class ChecksumHelper
{
    /// <summary>
    /// XOR-checksum over every byte except the two bytes at offset 14–15
    /// (the checksum field itself).
    /// </summary>
    public static ushort ComputeChecksum(ReadOnlySpan<byte> packet)
    {
        if (packet.Length < 16)
            throw new ArgumentException("Packet header is 16 bytes minimum.", nameof(packet));

        ushort sum = 0;
        for (int i = 0; i < packet.Length; i++)
        {
            if (i is 14 or 15) continue;   // skip the checksum field itself
            sum ^= packet[i];
        }
        return sum;
    }
}

public static class IntermediateDemo
{
    public static void Run()
    {
        Console.WriteLine("Ch08 Intermediate — PacketHeaderV2 with Checksum field");
        Console.WriteLine(new string('─', 60));

        Console.WriteLine($"  Unsafe.SizeOf<PacketHeaderV2>() = {Unsafe.SizeOf<PacketHeaderV2>()}  (must be 16)");

        var h = new PacketHeaderV2
        {
            Version = 1, Type = 0x01, Length = 256,
            SequenceNumber = 42, Timestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Reserved = 0, Checksum = 0,
        };

        // Round-trip the header through a byte buffer.
        Span<byte> buf = stackalloc byte[16];
        MemoryMarshal.Write(buf, in h);

        // Compute checksum over the marshalled bytes (with Checksum = 0).
        ushort cs = ChecksumHelper.ComputeChecksum(buf);
        h.Checksum = cs;
        MemoryMarshal.Write(buf, in h);

        // Decode it back.
        var decoded = MemoryMarshal.Read<PacketHeaderV2>(buf);

        Console.WriteLine($"  original: {h}");
        Console.WriteLine($"  decoded:  {decoded}");
        Console.WriteLine($"  round-trip OK? {h.Equals(decoded)}");
    }
}
