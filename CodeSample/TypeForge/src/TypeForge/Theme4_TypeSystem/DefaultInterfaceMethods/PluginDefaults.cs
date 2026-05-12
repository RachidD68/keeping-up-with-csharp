namespace TypeForge.Theme4_TypeSystem;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: Default Interface Methods  (C# 8)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  PROBLEM
//  -------
//  Adding a new method to an interface broke every existing
//  implementation.  The only escape was to create a new
//  interface (IFoo2) — leading to interface proliferation.
//
//  SOLUTION
//  --------
//  C# 8 allows interfaces to provide method bodies.
//  Implementations inherit the default behavior and can
//  override it.  This enables safe interface evolution
//  without breaking existing code.
//
//  WHY IT MATTERS
//  ──────────────
//  Library authors can add methods to interfaces without
//  breaking downstream code.  It's the "traits" pattern:
//  mix behavior into types through interface composition.
//
//  TRY IT
//  ──────
//  1. Add a default method Priority to IPlugin.
//  2. Override Describe() in one plugin but not another.
//  3. Try calling a default method through a concrete type vs interface.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Demonstrates default interface methods with a plugin system.
/// </summary>
public static class PluginDefaultsDemo
{
    // ── Interface with default methods ───────────────────────

    /// <summary>
    /// A logging mixin — provides logging via default interface methods.
    /// Types that implement this get logging for free.
    /// </summary>
    private interface ILoggable
    {
        string Name { get; }

        /// <summary>
        /// Default method — implementations get this automatically.
        /// They can override it if they need custom behavior.
        /// </summary>
        void Log(string message) =>
            Console.WriteLine($"    [{Name}] {message}");

        /// <summary>Another default method — structured logging.</summary>
        void LogError(string message) =>
            Console.WriteLine($"    [{Name}] ERROR: {message}");
    }

    /// <summary>
    /// A metrics mixin — another default interface with behavior.
    /// </summary>
    private interface IMetricsCollector
    {
        /// <summary>Default: returns a basic metric.</summary>
        Dictionary<string, int> CollectMetrics() => new()
        {
            ["invocations"] = 0,
            ["errors"] = 0
        };
    }

    // ── Plugins using default interface methods ──────────────

    /// <summary>
    /// UpperPlugin — uses the default Log() method as-is.
    /// </summary>
    private sealed class UpperPlugin : IPlugin, ILoggable, IMetricsCollector
    {
        public string Id => "upper";
        public string Name => "UpperCase";
        public string Version => "1.0.0";
        private int _invocations;

        public void Initialize() =>
            ((ILoggable)this).Log("Initialized");

        public string Execute(string input)
        {
            _invocations++;
            return input.ToUpperInvariant();
        }

        // CollectMetrics overridden with real data
        Dictionary<string, int> IMetricsCollector.CollectMetrics() => new()
        {
            ["invocations"] = _invocations,
            ["errors"] = 0
        };
    }

    /// <summary>
    /// ReversePlugin — overrides the default Log() method.
    /// </summary>
    private sealed class ReversePlugin : IPlugin, ILoggable
    {
        public string Id => "reverse";
        public string Name => "Reverse";
        public string Version => "2.0.0";

        public void Initialize() =>
            ((ILoggable)this).Log("Initialized");

        public string Execute(string input)
        {
            var chars = input.ToCharArray();
            Array.Reverse(chars);
            return new string(chars);
        }

        // Override default method with custom behavior
        void ILoggable.Log(string message) =>
            Console.WriteLine($"    [🔄 {Name}] {message}");
    }

    public static void Run()
    {
        Console.WriteLine("╔═══ Default Interface Methods ═══╗\n");

        // ── 1. Using default Describe() ──────────────────────
        Console.WriteLine("── Default Describe() ──");
        IPlugin upper = new UpperPlugin();
        IPlugin reverse = new ReversePlugin();

        // Describe() comes from IPlugin's default implementation
        Console.WriteLine($"  {upper.Describe()}");
        Console.WriteLine($"  {reverse.Describe()}");

        // ── 2. Default vs overridden Log() ───────────────────
        Console.WriteLine("\n── Default vs Overridden Methods ──");

        // Must cast to ILoggable to access default methods!
        // Default interface methods are NOT accessible through the class.
        var upperLoggable = (ILoggable)new UpperPlugin();
        var reverseLoggable = (ILoggable)new ReversePlugin();

        upperLoggable.Log("Uses default Log()");
        reverseLoggable.Log("Uses overridden Log()");

        upperLoggable.LogError("Default error formatting");
        reverseLoggable.LogError("Also default (not overridden)");

        // ── 3. Default metrics collector ─────────────────────
        Console.WriteLine("\n── Default Metrics ──");
        var upperMetrics = (IMetricsCollector)new UpperPlugin();
        upper.Execute("test");
        upper.Execute("test again");

        // UpperPlugin overrides CollectMetrics
        var metrics1 = ((IMetricsCollector)(UpperPlugin)upper).CollectMetrics();
        Console.WriteLine("  UpperPlugin metrics (overridden):");
        foreach (var (key, value) in metrics1)
            Console.WriteLine($"    {key}: {value}");

        // ── 4. Plugin execution ──────────────────────────────
        Console.WriteLine("\n── Plugin Execution ──");
        var plugins = new IPlugin[] { upper, reverse };
        foreach (var plugin in plugins)
        {
            var result = plugin.Execute("Hello, TypeForge!");
            Console.WriteLine($"  {plugin.Name}: \"{result}\"");
        }

        // ── 5. Important caveat ──────────────────────────────
        Console.WriteLine("\n── Key Insight ──");
        Console.WriteLine("  Default methods are only accessible via the interface type.");
        Console.WriteLine("  var x = new UpperPlugin();");
        Console.WriteLine("  x.Log(\"...\");        // Compile error!");
        Console.WriteLine("  ((ILoggable)x).Log(\"...\");  // Works!");
        Console.WriteLine("  This prevents accidental diamond-problem issues.");
    }
}
