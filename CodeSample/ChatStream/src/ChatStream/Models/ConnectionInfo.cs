namespace ChatStream.Models;

/// <summary>Connection settings for a chat endpoint.</summary>
/// <param name="Endpoint">The server URL or address.</param>
/// <param name="Timeout">Connection timeout.</param>
/// <param name="MaxRetries">Maximum retry attempts.</param>
/// <param name="RetryDelay">Delay between retries.</param>
public record ConnectionInfo(
    string Endpoint,
    TimeSpan Timeout,
    int MaxRetries = 3,
    TimeSpan? RetryDelay = null)
{
    /// <summary>Default connection settings.</summary>
    public static ConnectionInfo Default { get; } = new(
        "localhost:8080",
        TimeSpan.FromSeconds(30),
        MaxRetries: 3,
        RetryDelay: TimeSpan.FromSeconds(1));
}
