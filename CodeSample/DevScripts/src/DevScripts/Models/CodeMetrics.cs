// ╔══════════════════════════════════════════════════════════╗
// ║  Model: CodeMetrics & Supporting Types                  ║
// ║  Domain models for the DevScripts toolbox               ║
// ╚══════════════════════════════════════════════════════════╝

namespace DevScripts.Models;

/// <summary>Metrics for a source code file.</summary>
/// <param name="FileName">Name of the analyzed file.</param>
/// <param name="Lines">Total line count.</param>
/// <param name="CodeLines">Non-blank, non-comment lines.</param>
/// <param name="Complexity">Cyclomatic complexity estimate.</param>
/// <param name="Dependencies">Number of using directives.</param>
public record CodeMetrics(
    string FileName,
    int Lines,
    int CodeLines,
    int Complexity,
    int Dependencies);

/// <summary>A log entry from the developer scripts system.</summary>
/// <param name="Timestamp">When the entry was logged.</param>
/// <param name="Level">Severity level.</param>
/// <param name="Message">The log message.</param>
/// <param name="Source">The originating component.</param>
public record LogEntry(
    DateTimeOffset Timestamp,
    LogLevel Level,
    string Message,
    string Source);

/// <summary>Log severity levels.</summary>
public enum LogLevel
{
    Trace,
    Debug,
    Info,
    Warning,
    Error,
    Critical
}

/// <summary>A configuration template with key-value pairs and interpolation support.</summary>
/// <param name="Name">Template name.</param>
/// <param name="Values">Key-value pairs for the template.</param>
public record ConfigTemplate(
    string Name,
    IReadOnlyDictionary<string, string> Values);

/// <summary>Marker attribute for the AutoToString source generator.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class AutoToStringAttribute : Attribute;
