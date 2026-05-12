// Chapter 5 — Type System & OOP Flexibility — BASIC
// ----------------------------------------------------------------
// Exercise: Take the ILoggable interface with the default Log
//   method. Add a second default method, LogInfo, that prefixes
//   the message with "[INFO] " and delegates to Log. Implement it
//   in a new TimestampPlugin that overrides Log to prefix the
//   message with the current time. Verify that LogInfo picks up
//   the overridden Log, not the default.
//
// Hint: Default methods call each other through the interface, so
//   dispatch always goes through the concrete implementation.
// ----------------------------------------------------------------

namespace KeepUpCs.Exercises.Ch05;

public interface ILoggable
{
    string Name { get; }

    /// <summary>Default Log — implementations may override.</summary>
    void Log(string message) =>
        Console.WriteLine($"    [{Name}] {message}");

    /// <summary>
    /// Default LogInfo — prefixes with "[INFO] " and delegates to Log.
    /// Because the call goes through the interface dispatch, any
    /// override of Log() in a concrete type will be picked up here.
    /// </summary>
    void LogInfo(string message) =>
        Log($"[INFO] {message}");
}

/// <summary>Uses both default methods — no override of Log().</summary>
public sealed class PlainPlugin : ILoggable
{
    public string Name => "Plain";
}

/// <summary>Overrides Log() — LogInfo() must pick up the override.</summary>
public sealed class TimestampPlugin : ILoggable
{
    public string Name => "Timestamp";

    // Override the default Log() — explicit interface implementation.
    void ILoggable.Log(string message) =>
        Console.WriteLine($"    [{DateTimeOffset.UtcNow:HH:mm:ss}] [{Name}] {message}");
}

public static class BasicDemo
{
    public static void Run()
    {
        Console.WriteLine("Ch05 Basic — Default interface methods + virtual dispatch");
        Console.WriteLine(new string('─', 60));

        ILoggable plain     = new PlainPlugin();
        ILoggable timestamp = new TimestampPlugin();

        Console.WriteLine("  PlainPlugin (uses default Log):");
        plain.Log("direct call");
        plain.LogInfo("from LogInfo");

        Console.WriteLine();
        Console.WriteLine("  TimestampPlugin (overrides Log):");
        timestamp.Log("direct call");
        timestamp.LogInfo("from LogInfo — should also be timestamped");

        Console.WriteLine();
        Console.WriteLine("  Note: both LogInfo outputs are routed through the");
        Console.WriteLine("  concrete implementation of Log via interface dispatch.");
    }
}
