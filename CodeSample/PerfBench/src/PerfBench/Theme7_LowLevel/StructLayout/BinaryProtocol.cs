namespace PerfBench.Theme7_LowLevel;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: StructLayout and Memory-Mapped I/O
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  PROBLEM
//  -------
//  Binary protocols (network packets, file formats, hardware
//  registers) require exact byte layouts.  The default C#
//  struct layout is LayoutKind.Sequential — fields are placed
//  in declaration order, but the runtime may insert padding
//  for alignment.  For binary I/O you need explicit control
//  over every byte offset.
//
//  SOLUTION
//  --------
//  [StructLayout(LayoutKind.Explicit)] with [FieldOffset(n)]
//  gives you byte-level control over struct layout.  Combined
//  with MemoryMarshal.Read<T> / Write<T>, you can serialize
//  and deserialize structs directly to/from byte spans with
//  zero allocation and zero copying.
//
//  WHY IT MATTERS
//  ──────────────
//  Network servers, game engines, embedded systems, and file
//  parsers all deal with binary data.  Explicit layouts let
//  you define a struct that IS the wire format — no manual
//  serialization code, no intermediate objects, no allocations.
//
//  TRY IT
//  ──────
//  1. Add a new field to the protocol header (e.g., Checksum)
//     and adjust the offsets.
//  2. Create an overlapping union (two fields at the same offset).
//  3. Read a PacketHeader from a file using MemoryMarshal.Read.
//  4. Experiment with Pack = 1 vs Pack = 4 on SensorReading.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Demonstrates StructLayout for binary protocol handling,
/// contrasting Sequential vs Explicit layouts, and showing
/// zero-copy round-trip serialization with MemoryMarshal.
/// </summary>
public static class BinaryProtocolDemo
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — Manual byte-by-byte serialization
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Before: Manually writing each field to a byte array.
    /// Tedious, error-prone, and hard to maintain as the
    /// protocol evolves.
    /// </summary>
    private static void SerializeManually()
    {
        Console.WriteLine("  BEFORE (Manual byte-by-byte serialization):");
        Console.WriteLine();

        var header = PacketHeader.Create(PacketTypes.Data, 256, 1);
        byte[] buffer = new byte[PacketHeader.SizeInBytes];

        // Manual serialization — every field written individually
        buffer[0] = header.Version;
        buffer[1] = header.Type;
        BitConverter.TryWriteBytes(buffer.AsSpan(2, 2), header.Length);
        BitConverter.TryWriteBytes(buffer.AsSpan(4, 4), header.SequenceNumber);
        BitConverter.TryWriteBytes(buffer.AsSpan(8, 8), header.Timestamp);

        Console.WriteLine($"    Header: {header}");
        Console.Write("    Bytes:  ");
        PrintBytes(buffer);

        // Manual deserialization — every field read individually
        var restored = new PacketHeader
        {
            Version = buffer[0],
            Type = buffer[1],
            Length = BitConverter.ToUInt16(buffer, 2),
            SequenceNumber = BitConverter.ToUInt32(buffer, 4),
            Timestamp = BitConverter.ToUInt64(buffer, 8)
        };

        Console.WriteLine($"    Restored: {restored}");
        Console.WriteLine("    -> 8 lines of code, error-prone, hard to maintain");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — MemoryMarshal with explicit layout
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// After: MemoryMarshal.Write and Read perform zero-copy
    /// serialization using the struct's explicit layout.
    /// The struct IS the wire format.
    /// </summary>
    private static void SerializeWithMemoryMarshal()
    {
        Console.WriteLine("  AFTER (MemoryMarshal.Write/Read — zero-copy):");
        Console.WriteLine();

        var header = PacketHeader.Create(PacketTypes.Data, 256, 1);
        byte[] buffer = new byte[PacketHeader.SizeInBytes];

        // One line: struct -> bytes (zero-copy, uses explicit layout)
        MemoryMarshal.Write(buffer.AsSpan(), in header);

        Console.WriteLine($"    Header:  {header}");
        Console.Write("    Bytes:   ");
        PrintBytes(buffer);

        // One line: bytes -> struct (zero-copy, uses explicit layout)
        var restored = MemoryMarshal.Read<PacketHeader>(buffer);

        Console.WriteLine($"    Restored: {restored}");
        Console.WriteLine("    -> 2 lines of code, zero-copy, always in sync with layout");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 3: Layout comparison — Sequential vs Explicit
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Demonstrates the difference between Sequential and Explicit
    /// layout kinds using PerfBench domain models.
    /// </summary>
    private static void CompareLayouts()
    {
        Console.WriteLine("  LAYOUT COMPARISON — Sequential vs Explicit:");
        Console.WriteLine();

        // Sequential layout (fields in declaration order, with alignment padding)
        Console.WriteLine("    ── LayoutKind.Sequential (Pixel) ──");
        Console.WriteLine("    [StructLayout(LayoutKind.Sequential)]");
        Console.WriteLine("    readonly record struct Pixel(byte R, byte G, byte B, byte A)");
        Console.WriteLine($"    Size: {Unsafe.SizeOf<Pixel>()} bytes (4 bytes, no padding needed)");
        Console.WriteLine("    Layout: [R:1][G:1][B:1][A:1]  — contiguous, no gaps");
        Console.WriteLine();

        // Sequential layout with Pack (SensorReading)
        Console.WriteLine("    ── LayoutKind.Sequential, Pack=1 (SensorReading) ──");
        Console.WriteLine("    [StructLayout(LayoutKind.Sequential, Pack = 1)]");
        Console.WriteLine("    readonly record struct SensorReading(int, long, double, SensorStatus)");
        Console.WriteLine($"    Size: {Unsafe.SizeOf<SensorReading>()} bytes");
        Console.WriteLine("    Layout: [SensorId:4][Timestamp:8][Value:8][Status:1]");
        Console.WriteLine("    Pack=1 eliminates alignment padding between fields.");
        Console.WriteLine();

        // Show what happens without Pack=1
        Console.WriteLine($"    Without Pack=1, natural alignment would pad SensorReading to");
        Console.WriteLine($"    a larger size (long/double want 8-byte alignment).");
        Console.WriteLine();

        // Explicit layout (PacketHeader)
        Console.WriteLine("    ── LayoutKind.Explicit, Size=16 (PacketHeader) ──");
        Console.WriteLine("    [StructLayout(LayoutKind.Explicit, Size = 16)]");
        Console.WriteLine("    [FieldOffset(0)]  byte    Version         // offset 0");
        Console.WriteLine("    [FieldOffset(1)]  byte    Type            // offset 1");
        Console.WriteLine("    [FieldOffset(2)]  ushort  Length          // offset 2");
        Console.WriteLine("    [FieldOffset(4)]  uint    SequenceNumber  // offset 4");
        Console.WriteLine("    [FieldOffset(8)]  ulong   Timestamp       // offset 8");
        Console.WriteLine($"    Size: {Unsafe.SizeOf<PacketHeader>()} bytes (matches wire format exactly)");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 4: Round-trip demonstration with multiple packet types
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Demonstrates round-trip serialization of multiple packets
    /// into a contiguous byte buffer — simulating a network stream.
    /// </summary>
    private static void DemonstrateRoundTrip()
    {
        Console.WriteLine("  ROUND-TRIP — Simulated packet stream:");
        Console.WriteLine();

        // Create a stream of packets
        PacketHeader[] packets =
        [
            PacketHeader.Create(PacketTypes.Data,      512, 1),
            PacketHeader.Create(PacketTypes.Ack,         0, 2),
            PacketHeader.Create(PacketTypes.Heartbeat,   0, 3),
            PacketHeader.Create(PacketTypes.Data,     1024, 4),
            PacketHeader.Create(PacketTypes.Close,       0, 5),
        ];

        // Serialize all packets into a contiguous byte buffer
        int packetSize = PacketHeader.SizeInBytes;
        byte[] stream = new byte[packets.Length * packetSize];

        for (int i = 0; i < packets.Length; i++)
        {
            var span = stream.AsSpan(i * packetSize, packetSize);
            MemoryMarshal.Write(span, in packets[i]);
        }

        Console.WriteLine($"    Wrote {packets.Length} packets ({stream.Length} bytes total)");
        Console.Write("    Stream: ");
        PrintBytes(stream.AsSpan(0, Math.Min(48, stream.Length)));
        if (stream.Length > 48)
            Console.Write($"... ({stream.Length - 48} more bytes)");
        Console.WriteLine();
        Console.WriteLine();

        // Deserialize and verify
        Console.WriteLine("    Deserialized packets:");
        var readOnlyStream = new ReadOnlySpan<byte>(stream);

        for (int i = 0; i < packets.Length; i++)
        {
            var slice = readOnlyStream.Slice(i * packetSize, packetSize);
            var restored = MemoryMarshal.Read<PacketHeader>(slice);

            string typeName = restored.Type switch
            {
                PacketTypes.Data      => "DATA",
                PacketTypes.Ack       => "ACK ",
                PacketTypes.Heartbeat => "PING",
                PacketTypes.Close     => "CLOS",
                _                     => "????"
            };

            bool matches = restored.SequenceNumber == packets[i].SequenceNumber
                        && restored.Type == packets[i].Type
                        && restored.Length == packets[i].Length;

            Console.WriteLine($"    [{i}] {typeName} seq={restored.SequenceNumber} " +
                              $"len={restored.Length,4} — round-trip: {(matches ? "OK" : "MISMATCH")}");
        }
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 5: MemoryMarshal.Cast for bulk reinterpretation
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Demonstrates MemoryMarshal.Cast to reinterpret a byte span
    /// as a span of structs — zero-copy, zero-allocation.
    /// </summary>
    private static void DemonstrateCast()
    {
        Console.WriteLine("  MemoryMarshal.Cast — Bulk reinterpretation:");
        Console.WriteLine();

        // Create pixel data as bytes (simulating data read from a file)
        byte[] rawPixelData =
        [
            0xFF, 0x00, 0x00, 0xFF, // Red
            0x00, 0xFF, 0x00, 0xFF, // Green
            0x00, 0x00, 0xFF, 0xFF, // Blue
            0xFF, 0xFF, 0xFF, 0xFF, // White
            0x80, 0x80, 0x80, 0xFF, // Gray
        ];

        // Reinterpret bytes as Pixel structs — zero-copy!
        ReadOnlySpan<Pixel> pixels = MemoryMarshal.Cast<byte, Pixel>(rawPixelData);

        Console.WriteLine($"    Raw bytes: {rawPixelData.Length} bytes");
        Console.WriteLine($"    As Pixel[]: {pixels.Length} pixels (zero-copy reinterpretation)");
        Console.WriteLine();

        for (int i = 0; i < pixels.Length; i++)
            Console.WriteLine($"    [{i}] {pixels[i]} (brightness: {pixels[i].Brightness:F2})");
        Console.WriteLine();

        // Reverse: Pixel span back to bytes
        Pixel[] pixelArray = [new(0xAA, 0xBB, 0xCC, 0xDD), new(0x11, 0x22, 0x33, 0x44)];
        ReadOnlySpan<byte> asBytes = MemoryMarshal.AsBytes(pixelArray.AsSpan());

        Console.Write("    Pixels as bytes: ");
        PrintBytes(asBytes);
        Console.WriteLine();
        Console.WriteLine("    -> No serialization code, no allocations, no copying");
        Console.WriteLine();
    }

    // WHY THIS MATTERS:
    // When your struct IS the wire format, serialization becomes
    // a no-op.  MemoryMarshal.Read/Write and Cast<T> compile to
    // a single memory copy (or even just a pointer cast for Cast).
    // This is how high-performance network servers, game engines,
    // and real-time systems handle binary data in C#.

    // GOING DEEPER:
    // FieldOffset allows overlapping fields (unions).  For example:
    //   [FieldOffset(0)] uint PackedRgba;
    //   [FieldOffset(0)] byte R;
    //   [FieldOffset(1)] byte G;
    //   [FieldOffset(2)] byte B;
    //   [FieldOffset(3)] byte A;
    // This creates a union where PackedRgba and R/G/B/A share the
    // same memory — useful for color formats, register maps, and
    // variant types.

    // ──────────────────────────────────────────────────────────
    // TRY IT
    // ──────────────────────────────────────────────────────────

    // TODO: Create a union struct that overlaps a uint with four bytes
    //       (like the GOING DEEPER example above) and verify the
    //       byte order matches your platform's endianness.

    // TODO: Serialize an array of SensorReadings to a byte[] and
    //       read them back using MemoryMarshal.Cast.

    // TODO: Add a Checksum field to PacketHeader at offset 14
    //       (ushort) and verify the total size stays at 16 bytes.

    /// <summary>Runs the complete StructLayout demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: StructLayout & Memory-Mapped I/O");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        SerializeManually();
        SerializeWithMemoryMarshal();
        CompareLayouts();
        DemonstrateRoundTrip();
        DemonstrateCast();

        Console.WriteLine("  KEY TAKEAWAY:");
        Console.WriteLine("  Make your struct the wire format.  With LayoutKind.Explicit");
        Console.WriteLine("  and MemoryMarshal, serialization is zero-copy and zero-alloc.");
        Console.WriteLine("  No manual byte manipulation, no intermediate objects.");
        Console.WriteLine();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }

    // ── Helpers ────────────────────────────────────────────────

    private static void PrintBytes(ReadOnlySpan<byte> bytes)
    {
        for (int i = 0; i < bytes.Length; i++)
        {
            Console.Write($"{bytes[i]:X2}");
            if (i < bytes.Length - 1)
                Console.Write(i % 4 == 3 ? "  " : " ");
        }
        Console.WriteLine();
    }
}
