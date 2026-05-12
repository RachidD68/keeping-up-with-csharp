namespace PerfBench.Theme6_Memory;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Benchmarks: Span<T> vs Array.Copy & Substring
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  Run:  dotnet run -c Release -- --bench --filter *SpanBenchmarks*
//
//  These benchmarks prove that Span<T> slicing is allocation-free
//  compared to Array.Copy (which allocates a new array) and
//  Substring (which allocates a new string).
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[MemoryDiagnoser]
public class SpanBenchmarks
{
    private byte[] buffer = null!;
    private string csvLine = null!;

    [Params(64, 1024)]
    public int SliceSize { get; set; }

    private byte[] reverseData = null!;

    [GlobalSetup]
    public void Setup()
    {
        buffer = new byte[4096];
        Random.Shared.NextBytes(buffer);
        csvLine = "42:98.6,17:23.1,99:45.7,3:67.2,88:12.9";
        reverseData = new byte[2048];
        Random.Shared.NextBytes(reverseData);
    }

    // ── Slice extraction ─────────────────────────────────────

    [Benchmark(Description = "Array.Copy (allocates)")]
    public byte[] ArrayCopySlice()
    {
        var slice = new byte[SliceSize];
        Array.Copy(buffer, 128, slice, 0, SliceSize);
        return slice;
    }

    [Benchmark(Description = "Span.Slice (zero-alloc)")]
    public byte SpanSlice()
    {
        Span<byte> slice = buffer.AsSpan(128, SliceSize);
        return slice[0]; // return value to prevent elimination
    }

    // ── String parsing ───────────────────────────────────────

    [Benchmark(Description = "Substring parse (allocates)")]
    public (int id, double value) SubstringParse()
    {
        int colonIdx = csvLine.IndexOf(':');
        string idPart = csvLine[..colonIdx];
        int commaIdx = csvLine.IndexOf(',');
        string valPart = csvLine[(colonIdx + 1)..commaIdx];
        return (int.Parse(idPart), double.Parse(valPart));
    }

    [Benchmark(Description = "Span parse (zero-alloc)")]
    public (int id, double value) SpanParse()
    {
        ReadOnlySpan<char> line = csvLine.AsSpan();
        int colonIdx = line.IndexOf(':');
        int commaIdx = line.IndexOf(',');
        int id = int.Parse(line[..colonIdx]);
        double value = double.Parse(line[(colonIdx + 1)..commaIdx]);
        return (id, value);
    }

    // ── Reverse: Array copy vs Span.Reverse ──────────────────

    [Benchmark(Description = "Array reverse (allocates new array)")]
    public byte[] ArrayReverse()
    {
        byte[] result = new byte[reverseData.Length];
        for (int i = 0; i < reverseData.Length; i++)
            result[i] = reverseData[reverseData.Length - i - 1];
        return result;
    }

    [Benchmark(Description = "Span.Reverse (in-place, zero-alloc)")]
    public byte SpanReverse()
    {
        Span<byte> span = stackalloc byte[64];
        reverseData.AsSpan(0, 64).CopyTo(span);
        span.Reverse();
        return span[0];
    }
}

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Benchmarks: Date Parsing — Substring vs Split vs Span
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  Run:  dotnet run -c Release -- --bench --filter *DateParsing*
//
//  Three-way comparison: Substring (allocates), String.Split
//  (allocates more), ReadOnlySpan (zero-alloc).
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[MemoryDiagnoser]
public class DateParsingBenchmarks
{
    private string date = null!;

    [GlobalSetup]
    public void Setup()
    {
        date = "2023/05/31"; // YYYY/MM/DD format
    }

    [Benchmark(Description = "Substring parse (allocates)")]
    public DateTime SubstringParse()
    {
        int year = int.Parse(date.Substring(0, 4));
        int month = int.Parse(date.Substring(5, 2));
        int day = int.Parse(date.Substring(8, 2));
        return new DateTime(year, month, day);
    }

    [Benchmark(Description = "String.Split parse (allocates more)")]
    public DateTime SplitParse()
    {
        var parts = date.Split('/');
        int year = int.Parse(parts[0]);
        int month = int.Parse(parts[1]);
        int day = int.Parse(parts[2]);
        return new DateTime(year, month, day);
    }

    [Benchmark(Description = "Span parse (zero-alloc)")]
    public DateTime SpanDateParse()
    {
        ReadOnlySpan<char> span = date.AsSpan();
        int year = int.Parse(span.Slice(0, 4));
        int month = int.Parse(span.Slice(5, 2));
        int day = int.Parse(span.Slice(8, 2));
        return new DateTime(year, month, day);
    }
}
