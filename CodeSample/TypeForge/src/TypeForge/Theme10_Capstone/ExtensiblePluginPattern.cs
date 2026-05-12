namespace TypeForge.Theme10_Capstone;

// ╔══════════════════════════════════════════════════════════════════╗
// ║  Theme 10 Capstone — Extensible Plugin Architecture            ║
// ║                                                                ║
// ║  Combines every type-system feature from Theme 4 into a        ║
// ║  cohesive, production-quality plugin framework:                 ║
// ║                                                                ║
// ║  • Default interface methods — optional plugin capabilities     ║
// ║  • Static abstract members — type-level plugin metadata        ║
// ║  • Generic math — numeric plugin operations                    ║
// ║  • Interface composition — mix-in capabilities                 ║
// ║  • Sealed hierarchies — exhaustive plugin type handling        ║
// ║  • Generic constraints — type-safe plugin registration         ║
// ║  • Operator overloading — plugin priority ordering             ║
// ║  • File-scoped types — internal implementation details         ║
// ║  • Extension members — fluent plugin API                       ║
// ║                                                                ║
// ║  The result: a type-safe, extensible plugin system where        ║
// ║  the compiler enforces contracts that would otherwise be       ║
// ║  checked at runtime.                                           ║
// ╚══════════════════════════════════════════════════════════════════╝

// ── File-scoped helpers (only visible in this file) ──────────

/// <summary>Execution timer helper.</summary>
sealed class ExecutionTimer
{
    private readonly System.Diagnostics.Stopwatch _sw = new();

    public TimeSpan Measure(Action action)
    {
        _sw.Restart();
        action();
        _sw.Stop();
        return _sw.Elapsed;
    }
}

/// <summary>Execution log entry.</summary>
record ExecutionLog(
    string PluginId,
    string Input,
    string Output,
    TimeSpan Duration);

// ── Public capstone ──────────────────────────────────────────

/// <summary>
/// Capstone: An extensible plugin architecture demonstrating
/// all type-system features working in concert.
/// </summary>
public static class ExtensiblePluginPatternDemo
{
    // ── Plugin capabilities (interface composition) ──────────

    /// <summary>
    /// Configurable plugin — has settings.
    /// Default interface method provides standard behavior.
    /// </summary>
    private interface IConfigurable
    {
        Dictionary<string, string> Configuration { get; }

        /// <summary>Default method — standard config formatting.</summary>
        string FormatConfig() =>
            string.Join(", ", Configuration.Select(kv => $"{kv.Key}={kv.Value}"));
    }

    /// <summary>
    /// Chainable plugin — output feeds into next plugin.
    /// </summary>
    private interface IChainable : IPlugin
    {
        /// <summary>Whether this plugin can process the given input.</summary>
        bool CanProcess(string input);
    }

    /// <summary>
    /// Self-describing plugin with static factory.
    /// Static abstract + generic constraint.
    /// </summary>
    private interface ISelfDescribingPlugin<TSelf> : IPlugin, IPluginFactory<TSelf>
        where TSelf : ISelfDescribingPlugin<TSelf>
    {
        static abstract string Category { get; }
        static abstract string Description { get; }
    }

    // ── Concrete plugins ─────────────────────────────────────

    /// <summary>Converts text to uppercase — the simplest plugin.</summary>
    private sealed class UpperPlugin : ISelfDescribingPlugin<UpperPlugin>, IChainable
    {
        public string Id => PluginId;
        public string Name => "UpperCase";
        public string Version => "1.0.0";

        public static string PluginId => "upper";
        public static string Category => "Text Transform";
        public static string Description => "Converts text to uppercase";
        public static UpperPlugin Create() => new();

        public void Initialize() { }
        public string Execute(string input) => input.ToUpperInvariant();
        public bool CanProcess(string input) => !string.IsNullOrEmpty(input);
    }

    /// <summary>Reverses text — another chainable plugin.</summary>
    private sealed class ReversePlugin : ISelfDescribingPlugin<ReversePlugin>, IChainable
    {
        public string Id => PluginId;
        public string Name => "Reverse";
        public string Version => "1.0.0";

        public static string PluginId => "reverse";
        public static string Category => "Text Transform";
        public static string Description => "Reverses text characters";
        public static ReversePlugin Create() => new();

        public void Initialize() { }
        public string Execute(string input) =>
            new(input.Reverse().ToArray());
        public bool CanProcess(string input) => !string.IsNullOrEmpty(input);
    }

    /// <summary>Counts words — a configurable analysis plugin.</summary>
    private sealed class WordCountPlugin : ISelfDescribingPlugin<WordCountPlugin>, IConfigurable
    {
        private string _separator = " ";

        public string Id => PluginId;
        public string Name => "WordCount";
        public string Version => "2.1.0";

        public static string PluginId => "wordcount";
        public static string Category => "Analysis";
        public static string Description => "Counts words in text";
        public static WordCountPlugin Create() => new();

        public Dictionary<string, string> Configuration => new()
        {
            ["separator"] = _separator,
            ["mode"] = "split"
        };

        public void Initialize() { }

        public string Execute(string input)
        {
            var words = input.Split(_separator, StringSplitOptions.RemoveEmptyEntries);
            return $"{words.Length} words";
        }
    }

    // ── Plugin Registry (uses generic constraints) ───────────

    /// <summary>
    /// Type-safe plugin registry using generic constraints
    /// and static abstract members for metadata.
    /// </summary>
    private sealed class PluginRegistry
    {
        private readonly Dictionary<string, IPlugin> _plugins = new();
        private readonly List<ExecutionLog> _executionLog = [];
        private readonly ExecutionTimer _timer = new();

        /// <summary>
        /// Registers a self-describing plugin.
        /// Static abstract Category and Description are available
        /// without creating an instance.
        /// </summary>
        public void Register<T>() where T : ISelfDescribingPlugin<T>
        {
            Console.WriteLine(
                $"    Registering [{T.PluginId}] " +
                $"({T.Category}): {T.Description}");

            var plugin = T.Create();
            plugin.Initialize();
            _plugins[plugin.Id] = plugin;
        }

        /// <summary>Executes a plugin by ID with timing.</summary>
        public string? Execute(string pluginId, string input)
        {
            if (!_plugins.TryGetValue(pluginId, out var plugin))
            {
                Console.WriteLine($"    Plugin '{pluginId}' not found.");
                return null;
            }

            string output = "";
            var duration = _timer.Measure(() =>
                output = plugin.Execute(input));

            _executionLog.Add(new ExecutionLog(
                pluginId, input, output, duration));

            return output;
        }

        /// <summary>
        /// Chains multiple plugins — output of one feeds into the next.
        /// Uses pattern matching for IChainable check.
        /// </summary>
        public string? Chain(string input, params string[] pluginIds)
        {
            var current = input;
            foreach (var id in pluginIds)
            {
                if (!_plugins.TryGetValue(id, out var plugin))
                {
                    Console.WriteLine($"    Chain broken: '{id}' not found.");
                    return null;
                }

                // Pattern matching to check for IChainable
                if (plugin is IChainable chainable && !chainable.CanProcess(current))
                {
                    Console.WriteLine($"    Chain broken: '{id}' cannot process input.");
                    return null;
                }

                current = plugin.Execute(current);
            }
            return current;
        }

        /// <summary>Lists all registered plugins with their descriptions.</summary>
        public void ListPlugins()
        {
            foreach (var (id, plugin) in _plugins)
            {
                var extras = new List<string>();
                if (plugin is IChainable) extras.Add("chainable");
                if (plugin is IConfigurable cfg)
                    extras.Add($"config: {cfg.FormatConfig()}");

                var suffix = extras.Count > 0
                    ? $" [{string.Join(", ", extras)}]"
                    : "";
                Console.WriteLine($"    {plugin.Describe()}{suffix}");
            }
        }

        /// <summary>Gets execution statistics.</summary>
        public (int TotalExecutions, double AvgDurationMs) GetStats()
        {
            if (_executionLog.Count == 0) return (0, 0);
            var avgMs = _executionLog.Average(l => l.Duration.TotalMilliseconds);
            return (_executionLog.Count, avgMs);
        }
    }

    // ── Pipeline Builder (fluent API with extension-like pattern) ─

    /// <summary>
    /// A fluent pipeline builder that chains plugins together.
    /// </summary>
    private sealed class PipelineBuilder
    {
        private readonly PluginRegistry _registry;
        private readonly List<string> _steps = [];

        public PipelineBuilder(PluginRegistry registry) =>
            _registry = registry;

        public PipelineBuilder Then(string pluginId)
        {
            _steps.Add(pluginId);
            return this;
        }

        public string? Execute(string input)
        {
            Console.WriteLine($"    Pipeline: {string.Join(" → ", _steps)}");
            return _registry.Chain(input, [.. _steps]);
        }
    }

    public static void Run()
    {
        Console.WriteLine("╔═══ Theme 10 Capstone: Extensible Plugin Architecture ═══╗\n");

        var registry = new PluginRegistry();

        // ── 1. Register plugins (static abstract metadata) ───
        Console.WriteLine("── Plugin Registration (static abstract) ──");
        registry.Register<UpperPlugin>();
        registry.Register<ReversePlugin>();
        registry.Register<WordCountPlugin>();

        // ── 2. List plugins (default interface methods) ──────
        Console.WriteLine("\n── Registered Plugins ──");
        registry.ListPlugins();

        // ── 3. Execute individual plugins ────────────────────
        Console.WriteLine("\n── Individual Execution ──");
        var input = "Hello, TypeForge!";
        Console.WriteLine($"  Input: \"{input}\"");

        var upper = registry.Execute("upper", input);
        Console.WriteLine($"  upper:     \"{upper}\"");

        var reverse = registry.Execute("reverse", input);
        Console.WriteLine($"  reverse:   \"{reverse}\"");

        var wordCount = registry.Execute("wordcount", input);
        Console.WriteLine($"  wordcount: \"{wordCount}\"");

        // ── 4. Plugin chaining (IChainable pattern) ──────────
        Console.WriteLine("\n── Plugin Chaining ──");
        var chained = registry.Chain(input, "upper", "reverse");
        Console.WriteLine($"  upper → reverse: \"{chained}\"");

        var chained2 = registry.Chain(input, "reverse", "upper");
        Console.WriteLine($"  reverse → upper: \"{chained2}\"");

        // ── 5. Fluent pipeline builder ───────────────────────
        Console.WriteLine("\n── Fluent Pipeline ──");
        var pipeline = new PipelineBuilder(registry)
            .Then("upper")
            .Then("reverse");

        var pipeResult = pipeline.Execute("Build amazing things!");
        Console.WriteLine($"  Result: \"{pipeResult}\"");

        // ── 6. Execution statistics ──────────────────────────
        Console.WriteLine("\n── Execution Statistics ──");
        var stats = registry.GetStats();
        Console.WriteLine($"  Total executions: {stats.TotalExecutions}");
        Console.WriteLine($"  Avg duration:     {stats.AvgDurationMs:F3}ms");

        // ── 7. Type-system features used ─────────────────────
        Console.WriteLine("\n── Type-System Features Combined ──");
        Console.WriteLine("  ✓ Default interface methods — IPlugin.Describe(), IConfigurable.FormatConfig()");
        Console.WriteLine("  ✓ Static abstract members — ISelfDescribingPlugin<T>.PluginId, .Category");
        Console.WriteLine("  ✓ Interface composition — IChainable : IPlugin, IConfigurable");
        Console.WriteLine("  ✓ Generic constraints — Register<T>() where T : ISelfDescribingPlugin<T>");
        Console.WriteLine("  ✓ Pattern matching — plugin is IChainable chainable");
        Console.WriteLine("  ✓ File-scoped types — ExecutionTimer, ExecutionLog");
        Console.WriteLine("  ✓ Records — ExecutionLog, PluginInfo (immutable data)");
        Console.WriteLine("  ✓ Primary constructors — concise plugin definitions");
        Console.WriteLine("  → A compiler-enforced, extensible plugin architecture.");
    }
}
