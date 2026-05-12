namespace PerfBench.Theme10_Capstone;

// ╔══════════════════════════════════════════════════════════════════╗
// ║  Theme 10 Capstone — Native Interop Wrapper                    ║
// ║                                                                ║
// ║  Combines every low-level feature from Theme 7 into a          ║
// ║  cohesive, production-quality native interop layer:            ║
// ║                                                                ║
// ║  • StructLayout for binary protocol headers                    ║
// ║  • MemoryMarshal for zero-copy serialization                   ║
// ║  • Unsafe class for memory reinterpretation                    ║
// ║  • Span<T> for safe buffer management                          ║
// ║  • ref struct for scoped native resource handles               ║
// ║  • Function pointers for high-perf callbacks                   ║
// ║                                                                ║
// ║  The result: a binary protocol handler that reads/writes       ║
// ║  network packets with minimal allocation and maximum safety.   ║
// ╚══════════════════════════════════════════════════════════════════╝

/// <summary>
/// Capstone: A binary protocol handler combining all low-level features.
/// Reads and writes PacketHeader + payload with zero-copy techniques.
/// </summary>
public static class NativeInteropWrapperDemo
{
    // ── Protocol message types ───────────────────────────────

    /// <summary>
    /// A complete protocol message: header + payload.
    /// </summary>
    private readonly record struct ProtocolMessage(
        PacketHeader Header,
        ReadOnlyMemory<byte> Payload)
    {
        public override string ToString() =>
            $"{Header} payload={Payload.Length}B";
    }

    // ── Binary serializer using MemoryMarshal ────────────────

    /// <summary>
    /// Serializes PacketHeader to bytes using MemoryMarshal — zero copy.
    /// </summary>
    private static class PacketSerializer
    {
        /// <summary>
        /// Writes a header to a span — zero allocation.
        /// Uses MemoryMarshal to write the struct bytes directly.
        /// </summary>
        public static bool TryWriteHeader(Span<byte> destination, ref PacketHeader header)
        {
            if (destination.Length < PacketHeader.SizeInBytes)
                return false;

            MemoryMarshal.Write(destination, in header);
            return true;
        }

        /// <summary>
        /// Reads a header from a span — zero copy.
        /// </summary>
        public static bool TryReadHeader(ReadOnlySpan<byte> source, out PacketHeader header)
        {
            if (source.Length < PacketHeader.SizeInBytes)
            {
                header = default;
                return false;
            }

            header = MemoryMarshal.Read<PacketHeader>(source);
            return true;
        }

        /// <summary>
        /// Serializes a complete message (header + payload) into a buffer.
        /// </summary>
        public static int WriteMessage(
            Span<byte> buffer,
            byte packetType,
            ReadOnlySpan<byte> payload,
            uint sequenceNumber)
        {
            var header = PacketHeader.Create(
                packetType,
                (ushort)(PacketHeader.SizeInBytes + payload.Length),
                sequenceNumber);

            if (!TryWriteHeader(buffer, ref header))
                return 0;

            payload.CopyTo(buffer[PacketHeader.SizeInBytes..]);
            return PacketHeader.SizeInBytes + payload.Length;
        }

        /// <summary>
        /// Deserializes a message from a buffer.
        /// </summary>
        public static bool TryReadMessage(
            ReadOnlySpan<byte> buffer,
            out PacketHeader header,
            out ReadOnlySpan<byte> payload)
        {
            payload = default;
            if (!TryReadHeader(buffer, out header))
                return false;

            int payloadLength = header.Length - PacketHeader.SizeInBytes;
            if (buffer.Length < header.Length || payloadLength < 0)
                return false;

            payload = buffer.Slice(PacketHeader.SizeInBytes, payloadLength);
            return true;
        }
    }

    // ── Pixel buffer converter using Unsafe ──────────────────

    /// <summary>
    /// Converts between Pixel arrays and byte spans using
    /// MemoryMarshal.Cast — true zero-copy reinterpretation.
    /// </summary>
    private static class PixelConverter
    {
        /// <summary>
        /// Reinterprets a Pixel span as bytes — zero copy.
        /// The bytes are the raw RGBA values of the pixels.
        /// </summary>
        public static ReadOnlySpan<byte> AsBytes(ReadOnlySpan<Pixel> pixels) =>
            MemoryMarshal.AsBytes(pixels);

        /// <summary>
        /// Reinterprets bytes as Pixels — zero copy.
        /// </summary>
        public static ReadOnlySpan<Pixel> AsPixels(ReadOnlySpan<byte> bytes) =>
            MemoryMarshal.Cast<byte, Pixel>(bytes);

        /// <summary>
        /// Reads a Pixel at a specific byte offset using Unsafe.
        /// </summary>
        public static Pixel ReadAt(ReadOnlySpan<byte> buffer, int byteOffset)
        {
            var slice = buffer[byteOffset..];
            return MemoryMarshal.Read<Pixel>(slice);
        }
    }

    // ── Scoped buffer manager (ref struct) ───────────────────

    /// <summary>
    /// A ref struct that manages a pooled byte buffer for protocol I/O.
    /// Implements IDisposable for automatic cleanup.
    /// </summary>
    private ref struct ScopedBuffer : IDisposable
    {
        private byte[]? _pooled;
        private readonly Span<byte> _span;
        private int _position;

        public ScopedBuffer(int size)
        {
            _pooled = ArrayPool<byte>.Shared.Rent(size);
            _span = _pooled.AsSpan(0, size);
            _span.Clear();
            _position = 0;
        }

        public Span<byte> Written => _span[.._position];
        public Span<byte> Remaining => _span[_position..];
        public int Position => _position;
        public int Capacity => _span.Length;

        /// <summary>Advances the position after writing.</summary>
        public void Advance(int count) =>
            _position += count;

        /// <summary>Resets position to start for reading.</summary>
        public void Reset() =>
            _position = 0;

        public void Dispose()
        {
            if (_pooled is not null)
            {
                ArrayPool<byte>.Shared.Return(_pooled);
                _pooled = null;
            }
        }
    }

    // ── Transform pipeline using function-pointer-like pattern ─

    /// <summary>Pixel transform delegate.</summary>
    private delegate Pixel PixelTransform(Pixel input);

    /// <summary>Applies a transform to all pixels in a span.</summary>
    private static void ApplyTransform(
        Span<Pixel> pixels,
        PixelTransform transform)
    {
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = transform(pixels[i]);
    }

    /// <summary>Inverts a pixel's colors.</summary>
    private static Pixel InvertPixel(Pixel p) =>
        new((byte)(255 - p.R), (byte)(255 - p.G), (byte)(255 - p.B), p.A);

    /// <summary>Converts to grayscale.</summary>
    private static Pixel GrayscalePixel(Pixel p)
    {
        var gray = (byte)(0.299 * p.R + 0.587 * p.G + 0.114 * p.B);
        return new(gray, gray, gray, p.A);
    }

    public static void Run()
    {
        Console.WriteLine("╔═══ Theme 10 Capstone: Native Interop Wrapper ═══╗\n");

        // ── 1. Binary protocol serialization ─────────────────
        Console.WriteLine("── Binary Protocol I/O ──");

        using var buffer = new ScopedBuffer(1024);

        // Write multiple packets
        var payloads = new byte[][]
        {
            "Hello, PerfBench!"u8.ToArray(),
            "Sensor data packet"u8.ToArray(),
            "Heartbeat"u8.ToArray(),
        };

        var types = new byte[] { PacketTypes.Data, PacketTypes.Data, PacketTypes.Heartbeat };

        for (int i = 0; i < payloads.Length; i++)
        {
            int written = PacketSerializer.WriteMessage(
                buffer.Remaining,
                types[i],
                payloads[i],
                (uint)(i + 1));

            if (written > 0)
            {
                buffer.Advance(written);
                Console.WriteLine($"  Wrote packet #{i + 1}: {written} bytes (type={types[i]}, payload={payloads[i].Length}B)");
            }
        }
        Console.WriteLine($"  Total buffer used: {buffer.Position}/{buffer.Capacity} bytes");

        // Read packets back
        Console.WriteLine("\n── Reading Packets Back ──");
        var readBuffer = buffer.Written;
        int offset = 0;
        int packetNum = 0;

        while (offset < readBuffer.Length)
        {
            var remaining = readBuffer[offset..];
            if (!PacketSerializer.TryReadMessage(remaining, out var header, out var payload))
                break;

            packetNum++;
            Console.WriteLine($"  Packet #{packetNum}: {header}, payload=\"{System.Text.Encoding.UTF8.GetString(payload)}\"");
            offset += header.Length;
        }

        // ── 2. Pixel buffer reinterpretation ─────────────────
        Console.WriteLine("\n── Zero-Copy Pixel ↔ Bytes ──");
        Pixel[] pixels = [Pixel.Red, Pixel.Green, Pixel.Blue, Pixel.White];

        var bytes = PixelConverter.AsBytes(pixels);
        Console.WriteLine($"  {pixels.Length} pixels = {bytes.Length} bytes");
        Console.Write("  Bytes: ");
        for (int i = 0; i < Math.Min(16, bytes.Length); i++)
            Console.Write($"{bytes[i]:X2} ");
        Console.WriteLine("...");

        // Round-trip: bytes → pixels
        var roundTrip = PixelConverter.AsPixels(bytes);
        Console.Write("  Round-trip: ");
        foreach (var p in roundTrip)
            Console.Write($"{p} ");
        Console.WriteLine();

        // ── 3. Struct sizes using Unsafe ─────────────────────
        Console.WriteLine("\n── Struct Sizes (Unsafe.SizeOf) ──");
        Console.WriteLine($"  Pixel:         {Unsafe.SizeOf<Pixel>()} bytes");
        Console.WriteLine($"  SensorReading: {Unsafe.SizeOf<SensorReading>()} bytes");
        Console.WriteLine($"  PacketHeader:  {Unsafe.SizeOf<PacketHeader>()} bytes");

        // ── 4. Pixel transforms ──────────────────────────────
        Console.WriteLine("\n── Pixel Transform Pipeline ──");
        Pixel[] image = [Pixel.Red, Pixel.Green, Pixel.Blue, new(128, 64, 32)];

        Console.Write("  Original:  ");
        foreach (var p in image) Console.Write($"{p} ");
        Console.WriteLine();

        var inverted = image.ToArray();
        ApplyTransform(inverted, InvertPixel);
        Console.Write("  Inverted:  ");
        foreach (var p in inverted) Console.Write($"{p} ");
        Console.WriteLine();

        var grayscale = image.ToArray();
        ApplyTransform(grayscale, GrayscalePixel);
        Console.Write("  Grayscale: ");
        foreach (var p in grayscale) Console.Write($"{p} ");
        Console.WriteLine();

        // ── 5. Architecture summary ──────────────────────────
        Console.WriteLine("\n── Architecture ──");
        Console.WriteLine("  ┌────────────┐  MemoryMarshal  ┌──────────┐");
        Console.WriteLine("  │ PacketHeader│ ←──────────────→│ byte[]   │");
        Console.WriteLine("  │ (struct)   │   zero-copy      │ (buffer) │");
        Console.WriteLine("  └────────────┘                  └──────────┘");
        Console.WriteLine("  ┌────────────┐  Cast<Pixel,byte> ┌────────┐");
        Console.WriteLine("  │ Pixel[]    │ ←────────────────→│ byte[] │");
        Console.WriteLine("  └────────────┘   reinterpret      └────────┘");

        // ── 6. Features used ─────────────────────────────────
        Console.WriteLine("\n── C# Features Combined ──");
        Console.WriteLine("  ✓ StructLayout(Explicit) — PacketHeader binary layout");
        Console.WriteLine("  ✓ MemoryMarshal.Read/Write — zero-copy struct I/O");
        Console.WriteLine("  ✓ MemoryMarshal.Cast — Pixel[] ↔ byte[] reinterpret");
        Console.WriteLine("  ✓ Unsafe.SizeOf<T> — compile-time struct sizes");
        Console.WriteLine("  ✓ ref struct + IDisposable — ScopedBuffer lifecycle");
        Console.WriteLine("  ✓ ArrayPool<T> — pooled buffer management");
        Console.WriteLine("  ✓ Span<T> — safe buffer access throughout");
        Console.WriteLine("  → Safe, efficient, native-grade binary I/O in managed C#.");
    }
}
