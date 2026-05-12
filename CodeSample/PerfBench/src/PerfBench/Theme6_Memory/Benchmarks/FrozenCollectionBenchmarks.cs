using System.Collections.Frozen;

namespace PerfBench.Theme6_Memory;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Benchmarks: FrozenDictionary vs Dictionary, FrozenSet vs HashSet
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  Run:  dotnet run -c Release -- --bench --filter *FrozenCollection*
//
//  Frozen collections are optimized at construction time for
//  read-heavy workloads.  These benchmarks measure lookup and
//  contains-check performance against standard collections.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[MemoryDiagnoser]
public class FrozenCollectionBenchmarks
{
    private Dictionary<SensorStatus, string> dictionary = null!;
    private FrozenDictionary<SensorStatus, string> frozenDictionary = null!;
    private HashSet<SensorStatus> hashSet = null!;
    private FrozenSet<SensorStatus> frozenSet = null!;
    private SensorStatus[] lookupKeys = null!;

    // String-keyed collections for testing optimized string hashing
    private Dictionary<string, int> stringDict = null!;
    private FrozenDictionary<string, int> frozenStringDict = null!;
    private string[] stringKeys = null!;

    [GlobalSetup]
    public void Setup()
    {
        dictionary = new Dictionary<SensorStatus, string>
        {
            [SensorStatus.Normal] = "Operating within normal parameters",
            [SensorStatus.Warning] = "Elevated readings detected",
            [SensorStatus.Critical] = "Critical threshold exceeded",
            [SensorStatus.Offline] = "Sensor offline"
        };
        frozenDictionary = dictionary.ToFrozenDictionary();

        hashSet = [SensorStatus.Warning, SensorStatus.Critical];
        frozenSet = hashSet.ToFrozenSet();

        lookupKeys = Enum.GetValues<SensorStatus>();

        // Build string-keyed collections (50 entries)
        stringDict = Enumerable.Range(0, 50)
            .ToDictionary(i => $"sensor_{i:D3}", i => i);
        frozenStringDict = stringDict.ToFrozenDictionary();
        stringKeys = stringDict.Keys.ToArray();
    }

    // ── Dictionary vs FrozenDictionary (enum keys) ───────────

    [Benchmark(Baseline = true, Description = "Dictionary<TKey,TValue> lookup")]
    public string DictionaryLookup()
    {
        string result = null!;
        foreach (var key in lookupKeys)
            result = dictionary[key];
        return result;
    }

    [Benchmark(Description = "FrozenDictionary<TKey,TValue> lookup")]
    public string FrozenDictionaryLookup()
    {
        string result = null!;
        foreach (var key in lookupKeys)
            result = frozenDictionary[key];
        return result;
    }

    // ── HashSet vs FrozenSet ─────────────────────────────────

    [Benchmark(Description = "HashSet<T>.Contains")]
    public bool HashSetContains()
    {
        bool found = false;
        foreach (var key in lookupKeys)
            found = hashSet.Contains(key);
        return found;
    }

    [Benchmark(Description = "FrozenSet<T>.Contains")]
    public bool FrozenSetContains()
    {
        bool found = false;
        foreach (var key in lookupKeys)
            found = frozenSet.Contains(key);
        return found;
    }

    // ── String-keyed lookup (Frozen optimizes string hashing) ─

    [Benchmark(Description = "Dictionary<string,int> string lookup")]
    public int DictionaryStringLookup()
    {
        int result = 0;
        foreach (var key in stringKeys)
            result = stringDict[key];
        return result;
    }

    [Benchmark(Description = "FrozenDictionary<string,int> string lookup")]
    public int FrozenDictionaryStringLookup()
    {
        int result = 0;
        foreach (var key in stringKeys)
            result = frozenStringDict[key];
        return result;
    }
}
