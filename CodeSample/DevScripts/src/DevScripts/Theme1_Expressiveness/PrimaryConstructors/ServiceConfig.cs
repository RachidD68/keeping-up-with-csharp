// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: Primary Constructors                          ║
// ║  Introduced: C# 12 (class/struct)                       ║
// ║  Theme: 1 — Expressiveness & Boilerplate Reduction      ║
// ╚══════════════════════════════════════════════════════════╝

namespace DevScripts.Theme1_Expressiveness.PrimaryConstructors;

/// <summary>
/// Demonstrates primary constructors on classes and structs —
/// parameters declared directly on the type, eliminating
/// constructor + field assignment boilerplate.
/// </summary>
public static class ServiceConfigDemo
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — How we did it before (C# 11)
    // ──────────────────────────────────────────────────────────

    public static void BeforePrimaryConstructors()
    {
        Console.WriteLine("  BEFORE (C# 11 — Constructor + field assignment):");
        Console.WriteLine();

        var service = new OldConfigService("AuthService", LogLevel.Info, 30);
        Console.WriteLine($"    {service}");
        Console.WriteLine("    ⚠ 3 fields + 3 assignments + 1 constructor = ~12 lines");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — The modern way (C# 12)
    // ──────────────────────────────────────────────────────────

    public static void WithPrimaryConstructors()
    {
        Console.WriteLine("  AFTER (C# 12 — Primary constructors):");
        Console.WriteLine();

        // Class with primary constructor — parameters available everywhere
        var service = new ConfigService("AuthService", LogLevel.Info, 30);
        service.LogMessage("User authenticated");
        service.LogMessage("Token refreshed");
        Console.WriteLine($"    Service: {service}");

        Console.WriteLine();

        // Struct with primary constructor
        var endpoint = new Endpoint("https://api.example.com", 443, true);
        Console.WriteLine($"    Endpoint: {endpoint}");

        Console.WriteLine();

        // DI-style pattern — the most common use case
        var logger = new AppLogger("DevScripts", LogLevel.Debug);
        logger.Log("Application started");
        logger.Log("Configuration loaded");

        Console.WriteLine();
        Console.WriteLine("    ✓ Parameters declared on the type declaration line");
        Console.WriteLine("    ✓ Available in all members — no field assignment needed");
        Console.WriteLine("    ✓ Ideal for dependency injection patterns");
        Console.WriteLine("    ✓ Works on both class and struct");
    }

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // Dependency injection is the most common pattern in modern C#.
    // Every service class has a constructor that takes dependencies
    // and assigns them to fields. Primary constructors collapse this
    // from ~12 lines to ~1 line. This is the same transformation
    // records brought to data classes, now applied to service classes.

    // GOING DEEPER:
    // Primary constructor parameters are NOT fields — they're
    // captured by the compiler into hidden fields only if used
    // outside the constructor. This means:
    // 1. They don't appear in reflection as fields
    // 2. They can be mutable (no readonly guarantee)
    // 3. For records: parameters become public properties
    //    For classes/structs: parameters are private captures
    // If you need readonly semantics, assign to a private readonly
    // field in the constructor body or use the 'field' keyword (C# 14).

    // TRADE-OFF:
    // Primary constructor parameters on class/struct are mutable —
    // you can accidentally reassign them. For record types, this
    // isn't an issue because parameters become init-only properties.
    // For services with readonly dependencies, consider:
    //   class Svc(ILogger logger) { private readonly ILogger _logger = logger; }

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: Convert a class with 3+ injected dependencies to use
    // a primary constructor. Compare before/after line count.

    private sealed class OldConfigService
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

        public override string ToString() => $"{_name} [{_level}] timeout={_timeoutSeconds}s";
    }

    /// <summary>C# 12 primary constructor — same functionality, much less code.</summary>
    private sealed class ConfigService(string name, LogLevel level, int timeoutSeconds)
    {
        private readonly List<string> _logs = [];

        public void LogMessage(string message)
        {
            if (level <= LogLevel.Info)
            {
                var entry = $"[{name}] [{level}] {message}";
                _logs.Add(entry);
                Console.WriteLine($"      {entry}");
            }
        }

        public override string ToString() =>
            $"{name} [{level}] timeout={timeoutSeconds}s ({_logs.Count} logged)";
    }

    /// <summary>Struct with primary constructor.</summary>
    private readonly struct Endpoint(string url, int port, bool useTls)
    {
        public override string ToString() =>
            $"{(useTls ? "🔒" : "🔓")} {url}:{port}";
    }

    /// <summary>DI-style primary constructor — the most common pattern.</summary>
    private sealed class AppLogger(string component, LogLevel minLevel)
    {
        public void Log(string message)
        {
            if (minLevel <= LogLevel.Debug)
                Console.WriteLine($"      [{component}] {message}");
        }
    }

    /// <summary>Runs the complete primary constructors demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: Primary Constructors (C# 12)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforePrimaryConstructors();
        WithPrimaryConstructors();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
