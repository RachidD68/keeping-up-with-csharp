namespace PerfBench.Theme6_Memory;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: ref fields in ref structs  (C# 11)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  PROBLEM
//  -------
//  ref structs like Span<T> could hold references via their
//  internal pointer, but user-defined ref structs couldn't
//  declare ref fields.  This limited their usefulness — you
//  couldn't build a ref struct that tracked a position within
//  a span, or a cursor that referenced the current element.
//
//  SOLUTION
//  --------
//  C# 11 allows `ref` fields inside `ref struct` declarations.
//  A ref field stores a managed reference to a storage location
//  — array element, span slot, stack variable — without pinning
//  or unsafe code.  Combined with scoped parameters, the
//  compiler enforces that references don't escape their scope.
//
//  WHY IT MATTERS
//  ──────────────
//  ref fields enable building zero-allocation cursors, writers,
//  and readers that work directly over buffers.  The .NET
//  runtime itself uses this pattern for Span<T>'s internal
//  representation.  It's the final piece that makes ref structs
//  a complete tool for high-performance, GC-free abstractions.
//
//  TRY IT
//  ──────
//  1. Add a SpanReader that mirrors SpanWriter for read ops.
//  2. Build a ref struct cursor over Matrix row data.
//  3. Try storing a SpanWriter in a class field — observe the
//     compiler error (ref structs can't escape the stack).
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Demonstrates ref fields in ref structs for zero-allocation buffer manipulation.
/// </summary>
public static class RefStructContainersDemo
{
    // ── Before: ref structs couldn't hold ref fields ─────────

    /// <summary>
    /// Old-style: a writer that tracks position with an integer index.
    /// Works, but can't hold a direct reference to a storage location.
    /// Every write requires re-indexing into the span.
    /// </summary>
    private ref struct OldStyleWriter
    {
        private readonly Span<byte> buffer;
        private int position;

        public OldStyleWriter(Span<byte> buffer)
        {
            this.buffer = buffer;
            position = 0;
        }

        public int Position => position;
        public int Remaining => buffer.Length - position;

        public void WriteByte(byte value)
        {
            // Must index into span every time
            buffer[position++] = value;
        }

        public void WriteUInt16(ushort value)
        {
            BitConverter.TryWriteBytes(buffer[position..], value);
            position += sizeof(ushort);
        }
    }

    // ── After: ref fields enable direct references ───────────

    /// <summary>
    /// A SpanWriter with a ref field that tracks the current write
    /// position.  The ref field points directly into the underlying
    /// buffer — no re-indexing overhead.
    /// </summary>
    /// <remarks>
    /// This ref struct holds:
    /// - A Span&lt;byte&gt; for the full buffer (itself a ref struct)
    /// - A ref int field for the position counter
    /// The position is shared: if the caller passes a ref to their
    /// own int, both sides see the same position updates.
    /// </remarks>
    private ref struct SpanWriter
    {
        private readonly Span<byte> buffer;
        private ref int position;  // C# 11: ref field!

        /// <summary>
        /// Creates a writer that tracks position via a ref field.
        /// The position variable lives outside this struct — when
        /// the writer advances, the caller's variable updates too.
        /// </summary>
        public SpanWriter(Span<byte> buffer, ref int position)
        {
            this.buffer = buffer;
            this.position = ref position;  // ref assignment
        }

        public readonly int Position => position;
        public readonly int Remaining => buffer.Length - position;
        public readonly bool CanWrite(int bytes) => Remaining >= bytes;

        public void WriteByte(byte value)
        {
            buffer[position] = value;
            position++;  // Modifies the caller's variable!
        }

        public void WriteUInt16(ushort value)
        {
            BitConverter.TryWriteBytes(buffer[position..], value);
            position += sizeof(ushort);
        }

        public void WriteUInt32(uint value)
        {
            BitConverter.TryWriteBytes(buffer[position..], value);
            position += sizeof(uint);
        }

        public void WriteUInt64(ulong value)
        {
            BitConverter.TryWriteBytes(buffer[position..], value);
            position += sizeof(ulong);
        }

        public void WriteBytes(ReadOnlySpan<byte> data)
        {
            data.CopyTo(buffer[position..]);
            position += data.Length;
        }

        /// <summary>
        /// Returns the written portion of the buffer as a ReadOnlySpan.
        /// </summary>
        public readonly ReadOnlySpan<byte> WrittenSpan => buffer[..position];
    }

    /// <summary>
    /// A SensorReader ref struct that wraps a ReadOnlySpan of SensorReadings
    /// with a ref field tracking the current read position.
    /// </summary>
    private ref struct SensorReader
    {
        private readonly ReadOnlySpan<SensorReading> readings;
        private ref int index;  // C# 11: ref field!

        public SensorReader(ReadOnlySpan<SensorReading> readings, ref int index)
        {
            this.readings = readings;
            this.index = ref index;
        }

        public readonly int Position => index;
        public readonly int Count => readings.Length;
        public readonly bool HasMore => index < readings.Length;

        /// <summary>
        /// Reads the next sensor reading and advances the position.
        /// The ref field means the caller's index variable advances too.
        /// </summary>
        public SensorReading ReadNext()
        {
            if (!HasMore)
                throw new InvalidOperationException("No more readings");
            return readings[index++];
        }

        /// <summary>
        /// Peeks at the next reading without advancing.
        /// </summary>
        public readonly ref readonly SensorReading Peek()
        {
            if (!HasMore)
                throw new InvalidOperationException("No more readings");
            return ref readings[index];
        }

        /// <summary>
        /// Skips readings until one matches the predicate.
        /// Returns the number of readings skipped.
        /// </summary>
        public int SkipUntil(SensorStatus status)
        {
            int skipped = 0;
            while (HasMore && readings[index].Status != status)
            {
                index++;
                skipped++;
            }
            return skipped;
        }

        /// <summary>
        /// Reads all remaining readings of a given status.
        /// </summary>
        public int CountRemaining(SensorStatus status)
        {
            int count = 0;
            for (int i = index; i < readings.Length; i++)
            {
                if (readings[i].Status == status)
                    count++;
            }
            return count;
        }
    }

    /// <summary>
    /// A PixelCursor ref struct that tracks a mutable position
    /// over a Span of Pixels using a ref field.
    /// </summary>
    private ref struct PixelCursor
    {
        private readonly Span<Pixel> pixels;
        private ref int offset;  // C# 11: ref field!

        public PixelCursor(Span<Pixel> pixels, ref int offset)
        {
            this.pixels = pixels;
            this.offset = ref offset;
        }

        public readonly int Position => offset;
        public readonly bool HasCurrent => offset >= 0 && offset < pixels.Length;

        public readonly ref Pixel Current => ref pixels[offset];

        public void MoveNext() => offset++;
        public void MovePrev() => offset--;
        public void MoveTo(int position) => offset = position;

        /// <summary>
        /// Applies a brightness multiplier to all remaining pixels
        /// from the current position onward.
        /// </summary>
        public void ApplyBrightness(float factor)
        {
            while (HasCurrent)
            {
                ref Pixel p = ref pixels[offset];
                pixels[offset] = new Pixel(
                    (byte)Math.Clamp(p.R * factor, 0, 255),
                    (byte)Math.Clamp(p.G * factor, 0, 255),
                    (byte)Math.Clamp(p.B * factor, 0, 255),
                    p.A);
                offset++;
            }
        }
    }

    /// <summary>
    /// Demonstrates building a binary packet using SpanWriter.
    /// </summary>
    private static void DemonstrateSpanWriter()
    {
        Span<byte> buffer = stackalloc byte[64];
        int pos = 0;

        // The writer holds a ref to 'pos' — when it writes, pos advances
        var writer = new SpanWriter(buffer, ref pos);

        // Write a PacketHeader manually
        writer.WriteByte(1);                        // version
        writer.WriteByte(PacketTypes.Data);          // type
        writer.WriteUInt16(12);                      // payload length
        writer.WriteUInt32(1001);                    // sequence number
        writer.WriteUInt64((ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()); // timestamp

        // Write some payload
        ReadOnlySpan<byte> payload = [0xCA, 0xFE, 0xBA, 0xBE];
        writer.WriteBytes(payload);

        Console.WriteLine($"  Writer position: {writer.Position}");
        Console.WriteLine($"  Caller's pos:    {pos}");  // Same value — ref field!
        Console.WriteLine($"  Remaining:       {writer.Remaining} bytes");
        Console.WriteLine($"  Written bytes:   {BitConverter.ToString(writer.WrittenSpan.ToArray())}");
        Console.WriteLine("  Note: writer.Position and caller's pos are the SAME variable (ref field).");
    }

    /// <summary>
    /// Demonstrates the SensorReader with ref field position tracking.
    /// </summary>
    private static void DemonstrateSensorReader()
    {
        var readings = new SensorReading[]
        {
            SensorReading.Create(1, 22.0, SensorStatus.Normal),
            SensorReading.Create(2, 45.0, SensorStatus.Normal),
            SensorReading.Create(3, 88.0, SensorStatus.Warning),
            SensorReading.Create(4, 120.0, SensorStatus.Critical),
            SensorReading.Create(5, 35.0, SensorStatus.Normal),
            SensorReading.Create(6, 0.0, SensorStatus.Offline),
            SensorReading.Create(7, 91.0, SensorStatus.Warning)
        };

        int index = 0;
        var reader = new SensorReader(readings, ref index);

        // Read first two
        Console.WriteLine("  Reading first 2:");
        for (int i = 0; i < 2 && reader.HasMore; i++)
        {
            var r = reader.ReadNext();
            Console.WriteLine($"    {r}");
        }
        Console.WriteLine($"  Reader position: {reader.Position}, caller index: {index}");

        // Peek without advancing
        if (reader.HasMore)
        {
            ref readonly var peeked = ref reader.Peek();
            Console.WriteLine($"\n  Peek (no advance): {peeked}");
            Console.WriteLine($"  Position after peek: {reader.Position}");
        }

        // Skip to first Warning
        int skipped = reader.SkipUntil(SensorStatus.Warning);
        Console.WriteLine($"\n  Skipped {skipped} readings to reach Warning");
        if (reader.HasMore)
        {
            Console.WriteLine($"  Current: {reader.ReadNext()}");
        }
        Console.WriteLine($"  Position: {reader.Position}, caller index: {index}");

        // Count remaining warnings
        int remainingWarnings = reader.CountRemaining(SensorStatus.Warning);
        Console.WriteLine($"  Remaining warnings: {remainingWarnings}");
    }

    /// <summary>
    /// Demonstrates PixelCursor with ref field position tracking.
    /// </summary>
    private static void DemonstratePixelCursor()
    {
        Span<Pixel> pixels = stackalloc Pixel[6];
        pixels[0] = new Pixel(200, 100, 50);
        pixels[1] = new Pixel(100, 200, 150);
        pixels[2] = new Pixel(50, 50, 200);
        pixels[3] = new Pixel(255, 255, 0);
        pixels[4] = new Pixel(128, 128, 128);
        pixels[5] = new Pixel(0, 255, 128);

        Console.WriteLine("  Before cursor modification:");
        for (int i = 0; i < pixels.Length; i++)
            Console.WriteLine($"    [{i}] {pixels[i]}  brightness={pixels[i].Brightness:F3}");

        int offset = 3; // Start at pixel 3
        var cursor = new PixelCursor(pixels, ref offset);

        Console.WriteLine($"\n  Cursor starts at position {cursor.Position}");
        cursor.ApplyBrightness(0.5f); // Dim pixels 3-5

        Console.WriteLine($"  Cursor now at position {cursor.Position}, caller offset: {offset}");

        Console.WriteLine("\n  After dimming pixels 3-5 (0.5x brightness):");
        for (int i = 0; i < pixels.Length; i++)
            Console.WriteLine($"    [{i}] {pixels[i]}  brightness={pixels[i].Brightness:F3}");
    }

    /// <summary>
    /// Shows the shared-position pattern: multiple writers sharing
    /// the same position variable via ref fields.
    /// </summary>
    private static void DemonstrateSharedPosition()
    {
        Span<byte> buffer = stackalloc byte[32];
        int sharedPos = 0;

        // Two writers share the same position — one writes the header,
        // the other continues with the payload.
        var headerWriter = new SpanWriter(buffer, ref sharedPos);
        headerWriter.WriteByte(1);          // version
        headerWriter.WriteByte(0x01);       // type
        headerWriter.WriteUInt16(8);        // length
        Console.WriteLine($"  After header: position = {sharedPos}");

        // Second writer picks up where the first left off
        var payloadWriter = new SpanWriter(buffer, ref sharedPos);
        payloadWriter.WriteUInt32(0xDEADBEEF);
        payloadWriter.WriteUInt32(0xCAFEBABE);
        Console.WriteLine($"  After payload: position = {sharedPos}");
        Console.WriteLine($"  Total written: {sharedPos} bytes");
        Console.WriteLine("  Both writers shared the same ref int position.");
    }

    public static void Run()
    {
        Console.WriteLine("\u2554\u2550\u2550\u2550 ref fields in ref structs \u2550\u2550\u2550\u2557\n");

        // ── 1. SpanWriter with ref field ──────────────────────
        Console.WriteLine("\u2500\u2500 SpanWriter: ref Field Position Tracking \u2500\u2500");
        DemonstrateSpanWriter();

        // ── 2. SensorReader with ref field ────────────────────
        Console.WriteLine("\n\u2500\u2500 SensorReader: Cursor Over Readings \u2500\u2500");
        DemonstrateSensorReader();

        // ── 3. PixelCursor with ref field ─────────────────────
        Console.WriteLine("\n\u2500\u2500 PixelCursor: Modify In-Place via Cursor \u2500\u2500");
        DemonstratePixelCursor();

        // ── 4. Shared position pattern ────────────────────────
        Console.WriteLine("\n\u2500\u2500 Shared Position: Multiple Writers, One ref int \u2500\u2500");
        DemonstrateSharedPosition();

        // ── 5. Why ref fields matter ──────────────────────────
        Console.WriteLine("\n\u2500\u2500 What ref Fields Enable \u2500\u2500");
        Console.WriteLine("  Before C# 11:");
        Console.WriteLine("    ref struct MyWriter { int _pos; }       // position is a copy");
        Console.WriteLine("    Changes to _pos don't propagate to caller.");
        Console.WriteLine();
        Console.WriteLine("  After C# 11:");
        Console.WriteLine("    ref struct MyWriter { ref int _pos; }   // position is a reference");
        Console.WriteLine("    Changes to _pos ARE visible to caller.");
        Console.WriteLine();
        Console.WriteLine("  Constraints (compiler-enforced):");
        Console.WriteLine("    - ref fields only in ref structs (can't escape to heap)");
        Console.WriteLine("    - scoped parameters prevent dangling references");
        Console.WriteLine("    - Cannot store a ref struct in a class or regular struct");
    }
}
