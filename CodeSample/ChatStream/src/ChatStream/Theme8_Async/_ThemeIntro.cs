namespace ChatStream.Theme8_Async;

// ╔══════════════════════════════════════════════════════════════════╗
// ║  Theme 8 — Async, Concurrency & Streaming                     ║
// ║                                                                ║
// ║  C# pioneered async/await in a mainstream language (C# 5).     ║
// ║  Each version has deepened the async story:                     ║
// ║                                                                ║
// ║  ● C# 5  — async/await (the revolution)                        ║
// ║  ● C# 7  — ValueTask (allocation-free fast paths)              ║
// ║  ● C# 8  — IAsyncEnumerable & IAsyncDisposable                 ║
// ║  ● C# 8  — ConfigureAwait improvements                         ║
// ║  ● C# 8  — Cancellation best practices                         ║
// ║  ● .NET 6 — Parallel.ForEachAsync                              ║
// ║  ● .NET 9 — Task.WhenEach                                      ║
// ║  ● Channels — bounded backpressure pub/sub                     ║
// ║                                                                ║
// ║  In ChatStream, every feature is grounded in real async         ║
// ║  messaging: sending, receiving, streaming, and coordinating    ║
// ║  chat operations.                                              ║
// ╚══════════════════════════════════════════════════════════════════╝

/// <summary>Theme 8 introduction and metadata.</summary>
public static class Theme8Intro
{
    public static string Title => "Theme 8 — Async, Concurrency & Streaming";

    public static string Tagline =>
        "From callbacks to channels: async that scales.";

    public static void PrintHeader()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(new string('═', 60));
        Console.WriteLine($"  {Title}");
        Console.WriteLine($"  {Tagline}");
        Console.WriteLine(new string('═', 60));
        Console.ResetColor();
    }
}
