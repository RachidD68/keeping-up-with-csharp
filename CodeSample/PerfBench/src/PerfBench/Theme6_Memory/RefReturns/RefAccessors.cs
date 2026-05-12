namespace PerfBench.Theme6_Memory;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: ref returns and ref locals  (C# 7)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  PROBLEM
//  -------
//  Structs are value types — getting one from a collection
//  creates a defensive copy.  Modifying the copy changes
//  nothing in the original.  For large structs (SensorReading
//  is 21 bytes), this wastes time and creates subtle bugs
//  when developers expect in-place mutation.
//
//  SOLUTION
//  --------
//  `ref return` lets a method return a reference to a storage
//  location (array element, field, etc.) rather than a copy.
//  `ref local` captures that reference so you can read or
//  write the original.  `ref readonly` provides the same
//  zero-copy benefit while preventing mutation.
//
//  WHY IT MATTERS
//  ──────────────
//  In hot paths like game engines, signal processing, or
//  matrix math, avoiding struct copies can cut memory traffic
//  dramatically.  The Matrix model's indexer already returns
//  `ref double` — this pattern scales to any collection.
//
//  TRY IT
//  ──────
//  1. Remove `ref` from the local and verify the original
//     array element doesn't change.
//  2. Try assigning to a `ref readonly` local — observe the
//     compiler error.
//  3. Add a ref-returning method to Matrix for single-element
//     access with bounds checking.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Demonstrates ref returns and ref locals for zero-copy struct access.
/// </summary>
public static class RefAccessorsDemo
{
    // ── Before: indexer returns a copy ────────────────────────

    /// <summary>
    /// Traditional collection — the indexer returns a copy of the
    /// struct.  Modifying the returned value does NOT modify the
    /// stored element.
    /// </summary>
    private sealed class SensorArrayCopy(int size)
    {
        private readonly SensorReading[] data = new SensorReading[size];

        /// <summary>Returns a COPY — changes are lost.</summary>
        public SensorReading this[int index]
        {
            get => data[index];
            set => data[index] = value;
        }

        public void Set(int index, SensorReading reading) =>
            data[index] = reading;

        public int Length => data.Length;
    }

    // ── After: ref indexer returns a reference ───────────────

    /// <summary>
    /// Ref-returning collection — the indexer returns a reference
    /// to the original storage location.  Modifications through
    /// the ref go straight to the array element.
    /// </summary>
    private sealed class SensorArrayRef(int size)
    {
        private readonly SensorReading[] data = new SensorReading[size];

        /// <summary>
        /// Returns a ref to the array element — caller can modify in-place.
        /// </summary>
        public ref SensorReading this[int index] => ref data[index];

        /// <summary>
        /// Returns a ref readonly — caller can read without copy, but
        /// cannot modify.  Prevents accidental mutation.
        /// </summary>
        public ref readonly SensorReading GetReadOnly(int index) =>
            ref data[index];

        /// <summary>
        /// Finds the first reading matching a status and returns a ref
        /// to it — the caller can modify the original in-place.
        /// </summary>
        public ref SensorReading FindByStatus(SensorStatus status)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].Status == status)
                    return ref data[i];
            }
            throw new InvalidOperationException(
                $"No reading with status {status}");
        }

        public ReadOnlySpan<SensorReading> AsReadOnlySpan() => data;
        public int Length => data.Length;
    }

    /// <summary>
    /// Finds the maximum-value element and returns a ref to it.
    /// The caller can then modify the original array element in-place.
    /// </summary>
    private static ref SensorReading FindMax(SensorReading[] readings)
    {
        int maxIndex = 0;
        for (int i = 1; i < readings.Length; i++)
        {
            if (readings[i].Value > readings[maxIndex].Value)
                maxIndex = i;
        }
        return ref readings[maxIndex];
    }

    /// <summary>
    /// Demonstrates ref return from Matrix — its indexer already
    /// returns ref double for in-place modification.
    /// </summary>
    private static void DemonstrateMatrixRefIndexer()
    {
        using var matrix = new Matrix(3, 3, usePool: true);
        matrix.Fill(1.0);

        // The Matrix indexer returns `ref double` — we modify in-place.
        ref double center = ref matrix[1, 1];
        center = 99.0;

        // Also can assign directly through the ref indexer
        matrix[0, 0] = 42.0;
        matrix[2, 2] = 7.0;

        Console.WriteLine("  Matrix with ref-modified values:");
        for (int r = 0; r < 3; r++)
        {
            Console.Write("    ");
            for (int c = 0; c < 3; c++)
            {
                Console.Write($"{matrix[r, c],6:F1} ");
            }
            Console.WriteLine();
        }
    }

    /// <summary>
    /// Shows the copy problem — modifying a copy does not modify
    /// the original.
    /// </summary>
    private static void DemonstrateCopyProblem()
    {
        var readings = new SensorReading[3];
        readings[0] = SensorReading.Create(1, 25.0, SensorStatus.Normal);
        readings[1] = SensorReading.Create(2, 98.5, SensorStatus.Warning);
        readings[2] = SensorReading.Create(3, 15.0, SensorStatus.Normal);

        // Getting a struct by value creates a copy
        SensorReading copy = readings[1]; // copy!
        Console.WriteLine($"  Copy value:     {copy.Value:F1}");
        Console.WriteLine($"  Original value: {readings[1].Value:F1}");
        Console.WriteLine("  (They start equal, but they are independent copies)");

        // With readonly record struct, we can't mutate the copy directly,
        // but the key point is: there IS a copy, wasting memory.
        Console.WriteLine($"  SensorReading size: {SensorReading.SizeInBytes} bytes per copy");
    }

    /// <summary>
    /// Shows ref locals eliminating the copy.
    /// </summary>
    private static void DemonstrateRefLocal()
    {
        var readings = new SensorReading[3];
        readings[0] = SensorReading.Create(1, 25.0, SensorStatus.Normal);
        readings[1] = SensorReading.Create(2, 98.5, SensorStatus.Warning);
        readings[2] = SensorReading.Create(3, 15.0, SensorStatus.Normal);

        // ref local points directly to the array element — no copy
        ref SensorReading original = ref readings[1];
        Console.WriteLine($"  Ref to readings[1]: SensorId={original.SensorId}, Value={original.Value:F1}");

        // Replace the element in-place via the ref
        original = SensorReading.Create(2, 42.0, SensorStatus.Critical);
        Console.WriteLine($"  After ref assignment: readings[1].Value = {readings[1].Value:F1}");
        Console.WriteLine($"  Status changed to: {readings[1].Status}");
    }

    /// <summary>
    /// Demonstrates ref readonly for read-only zero-copy access.
    /// </summary>
    private static void DemonstrateRefReadonly()
    {
        var readings = new SensorReading[3];
        readings[0] = SensorReading.Create(1, 25.0, SensorStatus.Normal);
        readings[1] = SensorReading.Create(2, 98.5, SensorStatus.Warning);
        readings[2] = SensorReading.Create(3, 15.0, SensorStatus.Normal);

        // ref readonly — zero-copy read, but no modification allowed
        ref readonly SensorReading peek = ref readings[2];
        Console.WriteLine($"  ref readonly: SensorId={peek.SensorId}, Value={peek.Value:F1}");
        Console.WriteLine("  Cannot assign through ref readonly — compiler prevents it.");
        // peek = SensorReading.Create(3, 999.0); // Error CS8331
    }

    public static void Run()
    {
        Console.WriteLine("\u2554\u2550\u2550\u2550 ref returns and ref locals \u2550\u2550\u2550\u2557\n");

        // ── 1. The copy problem ───────────────────────────────
        Console.WriteLine("\u2500\u2500 The Copy Problem \u2500\u2500");
        DemonstrateCopyProblem();

        // ── 2. ref local eliminates the copy ──────────────────
        Console.WriteLine("\n\u2500\u2500 ref Local: Modify In-Place \u2500\u2500");
        DemonstrateRefLocal();

        // ── 3. ref readonly for safe zero-copy reads ──────────
        Console.WriteLine("\n\u2500\u2500 ref readonly: Zero-Copy Read \u2500\u2500");
        DemonstrateRefReadonly();

        // ── 4. ref-returning collection ───────────────────────
        Console.WriteLine("\n\u2500\u2500 Ref-Returning Collection \u2500\u2500");
        var sensors = new SensorArrayRef(4);
        sensors[0] = SensorReading.Create(10, 22.5, SensorStatus.Normal);
        sensors[1] = SensorReading.Create(20, 95.0, SensorStatus.Warning);
        sensors[2] = SensorReading.Create(30, 120.0, SensorStatus.Critical);
        sensors[3] = SensorReading.Create(40, 0.0, SensorStatus.Offline);

        Console.WriteLine("  Initial readings:");
        for (int i = 0; i < sensors.Length; i++)
            Console.WriteLine($"    [{i}] {sensors[i]}");

        // Find and modify in-place via ref return
        ref SensorReading critical = ref sensors.FindByStatus(SensorStatus.Critical);
        Console.WriteLine($"\n  Found critical sensor: {critical.SensorId}");
        critical = SensorReading.Create(critical.SensorId, 75.0, SensorStatus.Warning);
        Console.WriteLine($"  Downgraded in-place to: {sensors[2]}");

        // ref readonly access — zero-copy, no mutation
        ref readonly SensorReading peeked = ref sensors.GetReadOnly(0);
        Console.WriteLine($"\n  ref readonly peek: {peeked}");
        Console.WriteLine("  No copy made, no mutation possible.");

        // ── 5. ref return from a free function ────────────────
        Console.WriteLine("\n\u2500\u2500 ref Return from Free Function \u2500\u2500");
        var data = new SensorReading[5];
        for (int i = 0; i < data.Length; i++)
            data[i] = SensorReading.Create(i, Random.Shared.NextDouble() * 100, SensorStatus.Normal);

        Console.WriteLine("  Before:");
        foreach (var r in data)
            Console.WriteLine($"    {r}");

        ref SensorReading max = ref FindMax(data);
        Console.WriteLine($"\n  Max reading: Sensor {max.SensorId}, Value={max.Value:F2}");
        max = SensorReading.Create(max.SensorId, max.Value, SensorStatus.Critical);
        Console.WriteLine($"  Marked as critical in-place: {max}");

        // ── 6. Matrix ref indexer ─────────────────────────────
        Console.WriteLine("\n\u2500\u2500 Matrix ref Indexer \u2500\u2500");
        DemonstrateMatrixRefIndexer();

        // ── 7. Performance note ───────────────────────────────
        Console.WriteLine("\n\u2500\u2500 Why It Matters \u2500\u2500");
        Console.WriteLine($"  SensorReading is {SensorReading.SizeInBytes} bytes.");
        Console.WriteLine("  Without ref: every access copies those bytes.");
        Console.WriteLine("  With ref:    zero copies, direct access to storage.");
        Console.WriteLine("  In a loop of 1M readings, that's megabytes saved.");
    }
}
