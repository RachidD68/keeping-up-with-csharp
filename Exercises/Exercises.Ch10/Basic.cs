// Chapter 10 — Compiler & Tooling Integration — BASIC
// ----------------------------------------------------------------
// Exercise: Write a Timed(string operation) method in DevScripts
//   that logs "Starting {operation} from {member} ({file}:{line})"
//   using [CallerMemberName], [CallerFilePath], and [CallerLineNumber].
//   Call it from three different methods and verify the log shows
//   the correct caller info without you passing anything manually.
//
// Hint: The three attributes have default parameter values you
//   fill in with = "" / = 0.
// ----------------------------------------------------------------

using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace KeepUpCs.Exercises.Ch10;

public static class TimedLogger
{
    public static IDisposable Timed(
        string operation,
        [CallerMemberName] string member = "",
        [CallerFilePath]   string file   = "",
        [CallerLineNumber] int    line   = 0)
    {
        var fileName = Path.GetFileName(file);
        Console.WriteLine($"  Starting {operation} from {member} ({fileName}:{line})");
        return new TimingScope(operation, member, Stopwatch.StartNew());
    }

    private sealed class TimingScope(string operation, string member, Stopwatch sw) : IDisposable
    {
        public void Dispose()
        {
            sw.Stop();
            Console.WriteLine($"  Finished {operation} from {member} in {sw.ElapsedMilliseconds} ms");
        }
    }
}

public static class BasicDemo
{
    public static void Run()
    {
        Console.WriteLine("Ch10 Basic — Timed() with caller-info attributes");
        Console.WriteLine(new string('─', 60));

        DoFastWork();
        DoSlowWork();
        DoMixedWork();
    }

    private static void DoFastWork()
    {
        using var _ = TimedLogger.Timed("fast work");
        Thread.Sleep(15);
    }

    private static void DoSlowWork()
    {
        using var _ = TimedLogger.Timed("slow work");
        Thread.Sleep(80);
    }

    private static void DoMixedWork()
    {
        using var _ = TimedLogger.Timed("mixed work");
        Thread.Sleep(40);
    }
}
