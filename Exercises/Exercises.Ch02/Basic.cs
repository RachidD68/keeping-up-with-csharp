// Chapter 2 — Expressiveness & Boilerplate Reduction — BASIC
// ----------------------------------------------------------------
// Exercise: Take DevScripts' OldConfigService class and rewrite it
//   as a class with a primary constructor. Confirm the ToString()
//   output is identical.
//
// Hint: The parameters go on the type declaration line; you can
//   delete the three fields and the constructor body.
// ----------------------------------------------------------------

namespace KeepUpCs.Exercises.Ch02;

public enum LogLevel { Trace, Debug, Info, Warning, Error }

// ── BEFORE (C# 11 — 12 lines) ───────────────────────────────────
public sealed class OldConfigService
{
    private readonly string _name;
    private readonly LogLevel _level;
    private readonly int _timeoutSeconds;

    public OldConfigService(string name, LogLevel level, int timeoutSeconds)
    {
        _name = name;
        _level = level;
        _timeoutSeconds = timeoutSeconds;
    }

    public override string ToString() =>
        $"{_name} [{_level}] timeout={_timeoutSeconds}s";
}

// ── AFTER (C# 12 — 3 lines) ─────────────────────────────────────
public sealed class ConfigService(string name, LogLevel level, int timeoutSeconds)
{
    public override string ToString() =>
        $"{name} [{level}] timeout={timeoutSeconds}s";
}

public static class BasicDemo
{
    public static void Run()
    {
        Console.WriteLine("Ch02 Basic — Primary constructor rewrite");
        Console.WriteLine(new string('─', 60));

        var oldSvc = new OldConfigService("AuthService", LogLevel.Info, 30);
        var newSvc = new ConfigService("AuthService", LogLevel.Info, 30);

        var oldStr = oldSvc.ToString();
        var newStr = newSvc.ToString();

        Console.WriteLine($"  old: {oldStr}");
        Console.WriteLine($"  new: {newStr}");
        Console.WriteLine($"  identical? {oldStr == newStr}");
    }
}
