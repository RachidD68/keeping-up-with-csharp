namespace TypeForge.Models;

// ╔══════════════════════════════════════════════════════════╗
// ║  Plugin system — demonstrates interfaces, default       ║
// ║  methods, static abstracts, and the extension pattern.  ║
// ╚══════════════════════════════════════════════════════════╝

/// <summary>
/// The base plugin interface — all plugins implement this.
/// </summary>
public interface IPlugin
{
    /// <summary>Unique identifier for this plugin.</summary>
    string Id { get; }

    /// <summary>Human-readable name.</summary>
    string Name { get; }

    /// <summary>Semantic version string.</summary>
    string Version { get; }

    /// <summary>Initializes the plugin.</summary>
    void Initialize();

    /// <summary>Executes the plugin's main action.</summary>
    string Execute(string input);

    /// <summary>
    /// Default interface method (C# 8) — provides a description
    /// without requiring every implementation to override it.
    /// </summary>
    string Describe() => $"{Name} v{Version} ({Id})";
}

/// <summary>
/// Plugin lifecycle interface — optional, for plugins that need cleanup.
/// </summary>
public interface IPluginLifecycle : IPlugin
{
    /// <summary>Called when the plugin is being unloaded.</summary>
    void Shutdown();

    /// <summary>
    /// Default implementation logs shutdown.
    /// Implementations can override for custom cleanup.
    /// </summary>
    void IPlugin.Initialize()
    {
        Console.WriteLine($"  [Lifecycle] {Name} initialized.");
    }
}

/// <summary>
/// A plugin that can create instances of itself — uses static abstract.
/// </summary>
public interface IPluginFactory<TSelf> where TSelf : IPluginFactory<TSelf>
{
    /// <summary>Static factory method — enforced at the type level.</summary>
    static abstract TSelf Create();

    /// <summary>Plugin metadata available without an instance.</summary>
    static abstract string PluginId { get; }
}

/// <summary>Plugin priority for ordering.</summary>
public enum PluginPriority { Low = 0, Normal = 5, High = 10, Critical = 15 }

/// <summary>
/// Plugin metadata record — used by the plugin registry.
/// </summary>
public record PluginInfo(
    string Id,
    string Name,
    string Version,
    PluginPriority Priority = PluginPriority.Normal,
    string[]? Dependencies = null);
