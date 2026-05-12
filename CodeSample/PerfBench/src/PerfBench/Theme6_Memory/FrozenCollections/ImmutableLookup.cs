using System.Collections.Frozen;
using System.Diagnostics;

namespace PerfBench.Theme6_Memory;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: FrozenDictionary / FrozenSet  (.NET 8)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  PROBLEM
//  -------
//  Standard Dictionary<TKey, TValue> and HashSet<T> are
//  optimized for a balance of reads and writes.  Immutable
//  collections (ImmutableDictionary, etc.) optimize for
//  structural sharing during writes.  Neither is optimal
//  for the common case: build once, read millions of times.
//
//  SOLUTION
//  --------
//  FrozenDictionary and FrozenSet are created once (from an
//  existing dictionary or set) and then become read-only.
//  The runtime analyzes the keys at creation time and picks
//  the best internal layout — sometimes a perfect hash,
//  sometimes a flat array, sometimes a length-bucketed lookup.
//  Reads are dramatically faster.
//
//  WHY IT MATTERS
//  ──────────────
//  Configuration maps, status code descriptions, permission
//  tables, enum-to-string lookups — all are created at startup
//  and read in every request.  FrozenDictionary turns O(1)
//  with high constant into O(1) with minimal constant.
//
//  TRY IT
//  ──────
//  1. Benchmark FrozenDictionary vs Dictionary for 1M lookups.
//  2. Try FrozenSet<SensorStatus>.Contains in a hot loop.
//  3. Create a FrozenDictionary with string keys — observe
//     the optimized string comparison strategy.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Demonstrates FrozenDictionary and FrozenSet for read-optimized immutable lookups.
/// </summary>
public static class ImmutableLookupDemo
{
    // ── Before: Dictionary — good for read/write mix ─────────

    /// <summary>
    /// Standard Dictionary — the traditional approach for lookups.
    /// Perfectly fine for general use, but not optimized for
    /// read-heavy, write-never scenarios.
    /// </summary>
    private static readonly Dictionary<SensorStatus, string> statusDescriptionsDictionary = new()
    {
        [SensorStatus.Normal] = "Operating within normal parameters",
        [SensorStatus.Warning] = "Elevated readings detected — monitor closely",
        [SensorStatus.Critical] = "Critical threshold exceeded — immediate action required",
        [SensorStatus.Offline] = "Sensor offline — no data available"
    };

    /// <summary>
    /// Standard HashSet for membership checks.
    /// </summary>
    private static readonly HashSet<SensorStatus> alertableStatusesHashSet =
    [
        SensorStatus.Warning,
        SensorStatus.Critical
    ];

    // ── After: Frozen collections — optimized for reads ──────

    /// <summary>
    /// FrozenDictionary — created once from the Dictionary, then
    /// provides optimized read-only access for the lifetime of
    /// the application.
    /// </summary>
    private static readonly FrozenDictionary<SensorStatus, string> statusDescriptionsFrozen =
        statusDescriptionsDictionary.ToFrozenDictionary();

    /// <summary>
    /// FrozenSet — optimized contains-checks for read-heavy scenarios.
    /// </summary>
    private static readonly FrozenSet<SensorStatus> alertableStatusesFrozen =
        alertableStatusesHashSet.ToFrozenSet();

    /// <summary>
    /// A more complex lookup: sensor ID ranges to their physical locations.
    /// Built once at startup, queried on every reading.
    /// </summary>
    private static readonly FrozenDictionary<int, string> sensorLocations =
        Enumerable.Range(0, 50)
            .ToDictionary(
                id => id,
                id => id switch
                {
                    < 10 => "Building A — Floor 1",
                    < 20 => "Building A — Floor 2",
                    < 30 => "Building B — Floor 1",
                    < 40 => "Building B — Floor 2",
                    _ => "External — Roof Array"
                })
            .ToFrozenDictionary();

    /// <summary>
    /// FrozenSet of known packet types for fast validation.
    /// </summary>
    private static readonly FrozenSet<byte> validPacketTypes =
        new HashSet<byte>([PacketTypes.Data, PacketTypes.Ack, PacketTypes.Heartbeat, PacketTypes.Close])
            .ToFrozenSet();

    /// <summary>
    /// Demonstrates looking up sensor status descriptions.
    /// </summary>
    private static void DemonstrateLookup()
    {
        Console.WriteLine("  Status descriptions (FrozenDictionary):");
        foreach (SensorStatus status in Enum.GetValues<SensorStatus>())
        {
            string description = statusDescriptionsFrozen[status];
            Console.WriteLine($"    {status,-10} : {description}");
        }
    }

    /// <summary>
    /// Demonstrates FrozenSet membership checks.
    /// </summary>
    private static void DemonstrateMembership()
    {
        var readings = new SensorReading[]
        {
            SensorReading.Create(1, 22.5, SensorStatus.Normal),
            SensorReading.Create(2, 85.0, SensorStatus.Warning),
            SensorReading.Create(3, 120.0, SensorStatus.Critical),
            SensorReading.Create(4, 0.0, SensorStatus.Offline),
            SensorReading.Create(5, 45.0, SensorStatus.Normal)
        };

        Console.WriteLine("  Alert check (FrozenSet.Contains):");
        foreach (var reading in readings)
        {
            bool shouldAlert = alertableStatusesFrozen.Contains(reading.Status);
            string marker = shouldAlert ? "ALERT" : "  ok ";
            Console.WriteLine($"    [{marker}] {reading}");
        }
    }

    /// <summary>
    /// Demonstrates the location lookup for sensor IDs.
    /// </summary>
    private static void DemonstrateLocationLookup()
    {
        int[] sensorIds = [0, 5, 15, 25, 35, 45];

        Console.WriteLine("  Sensor locations (FrozenDictionary<int, string>):");
        foreach (int id in sensorIds)
        {
            string location = sensorLocations[id];
            Console.WriteLine($"    Sensor {id,2} -> {location}");
        }
    }

    /// <summary>
    /// Demonstrates packet type validation with FrozenSet.
    /// </summary>
    private static void DemonstratePacketValidation()
    {
        byte[] incomingTypes = [0x01, 0x02, 0x03, 0xFF, 0x42, 0x00];

        Console.WriteLine("  Packet type validation (FrozenSet<byte>):");
        foreach (byte type in incomingTypes)
        {
            bool valid = validPacketTypes.Contains(type);
            Console.WriteLine($"    Type 0x{type:X2}: {(valid ? "valid" : "INVALID")}");
        }
    }

    /// <summary>
    /// Compares lookup performance between Dictionary and FrozenDictionary.
    /// </summary>
    private static void CompareLookupPerformance()
    {
        const int lookups = 1_000_000;
        var statuses = Enum.GetValues<SensorStatus>();

        // Warm up
        foreach (var s in statuses)
        {
            _ = statusDescriptionsDictionary[s];
            _ = statusDescriptionsFrozen[s];
        }

        // Dictionary lookup
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < lookups; i++)
        {
            var status = statuses[i % statuses.Length];
            _ = statusDescriptionsDictionary[status];
        }
        sw.Stop();
        long dictTime = sw.ElapsedTicks;

        // FrozenDictionary lookup
        sw.Restart();
        for (int i = 0; i < lookups; i++)
        {
            var status = statuses[i % statuses.Length];
            _ = statusDescriptionsFrozen[status];
        }
        sw.Stop();
        long frozenTime = sw.ElapsedTicks;

        double dictMs = (double)dictTime / Stopwatch.Frequency * 1000;
        double frozenMs = (double)frozenTime / Stopwatch.Frequency * 1000;

        Console.WriteLine($"  Dictionary:       {lookups:N0} lookups in {dictMs:F2} ms");
        Console.WriteLine($"  FrozenDictionary: {lookups:N0} lookups in {frozenMs:F2} ms");
        if (dictMs > 0)
            Console.WriteLine($"  Frozen is {dictMs / frozenMs:F1}x the speed (lower is better for time)");
    }

    /// <summary>
    /// Compares Contains performance between HashSet and FrozenSet.
    /// </summary>
    private static void CompareContainsPerformance()
    {
        const int checks = 1_000_000;
        var statuses = Enum.GetValues<SensorStatus>();

        // HashSet
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < checks; i++)
        {
            var status = statuses[i % statuses.Length];
            _ = alertableStatusesHashSet.Contains(status);
        }
        sw.Stop();
        long hashTime = sw.ElapsedTicks;

        // FrozenSet
        sw.Restart();
        for (int i = 0; i < checks; i++)
        {
            var status = statuses[i % statuses.Length];
            _ = alertableStatusesFrozen.Contains(status);
        }
        sw.Stop();
        long frozenTime = sw.ElapsedTicks;

        double hashMs = (double)hashTime / Stopwatch.Frequency * 1000;
        double frozenMs = (double)frozenTime / Stopwatch.Frequency * 1000;

        Console.WriteLine($"  HashSet:    {checks:N0} Contains in {hashMs:F2} ms");
        Console.WriteLine($"  FrozenSet:  {checks:N0} Contains in {frozenMs:F2} ms");
    }

    public static void Run()
    {
        Console.WriteLine("\u2554\u2550\u2550\u2550 FrozenDictionary / FrozenSet \u2550\u2550\u2550\u2557\n");

        // ── 1. FrozenDictionary lookup ────────────────────────
        Console.WriteLine("\u2500\u2500 Status Descriptions (FrozenDictionary) \u2500\u2500");
        DemonstrateLookup();

        // ── 2. FrozenSet membership ───────────────────────────
        Console.WriteLine("\n\u2500\u2500 Alert Filtering (FrozenSet) \u2500\u2500");
        DemonstrateMembership();

        // ── 3. Complex lookup table ───────────────────────────
        Console.WriteLine("\n\u2500\u2500 Sensor Location Lookup \u2500\u2500");
        DemonstrateLocationLookup();

        // ── 4. Packet validation ──────────────────────────────
        Console.WriteLine("\n\u2500\u2500 Packet Type Validation \u2500\u2500");
        DemonstratePacketValidation();

        // ── 5. Performance: Dictionary vs Frozen ──────────────
        Console.WriteLine("\n\u2500\u2500 Performance: Dictionary vs FrozenDictionary \u2500\u2500");
        CompareLookupPerformance();

        // ── 6. Performance: HashSet vs FrozenSet ──────────────
        Console.WriteLine("\n\u2500\u2500 Performance: HashSet vs FrozenSet \u2500\u2500");
        CompareContainsPerformance();

        // ── 7. API surface ────────────────────────────────────
        Console.WriteLine("\n\u2500\u2500 API Highlights \u2500\u2500");
        Console.WriteLine($"  FrozenDictionary.Count: {statusDescriptionsFrozen.Count}");
        Console.WriteLine($"  FrozenDictionary.Keys:  [{string.Join(", ", statusDescriptionsFrozen.Keys)}]");
        Console.WriteLine($"  FrozenSet.Count:        {alertableStatusesFrozen.Count}");
        Console.WriteLine($"  FrozenSet.Items:        [{string.Join(", ", alertableStatusesFrozen)}]");

        bool found = statusDescriptionsFrozen.TryGetValue(SensorStatus.Critical, out string? desc);
        Console.WriteLine($"  TryGetValue(Critical):  {found} -> \"{desc}\"");

        Console.WriteLine("\n\u2500\u2500 When to Use \u2500\u2500");
        Console.WriteLine("  Use FrozenDictionary/FrozenSet when:");
        Console.WriteLine("    - Data is built once (startup, config load, etc.)");
        Console.WriteLine("    - Reads vastly outnumber writes (which are impossible)");
        Console.WriteLine("    - Lookup is on a hot path (per-request, per-frame)");
        Console.WriteLine("  Use Dictionary/HashSet when:");
        Console.WriteLine("    - Data changes at runtime");
        Console.WriteLine("    - Write performance matters");
    }
}
