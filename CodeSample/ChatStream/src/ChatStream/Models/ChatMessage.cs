namespace ChatStream.Models;

/// <summary>
/// An immutable chat message — the core domain type.
/// </summary>
/// <param name="Id">Unique message identifier.</param>
/// <param name="Sender">The user who sent the message.</param>
/// <param name="Content">Message text content.</param>
/// <param name="Channel">The channel this message was sent in.</param>
/// <param name="Timestamp">When the message was created.</param>
public record ChatMessage(
    Guid Id,
    string Sender,
    string Content,
    string Channel,
    DateTimeOffset Timestamp)
{
    /// <summary>Creates a new message with auto-generated ID and timestamp.</summary>
    public static ChatMessage Create(string sender, string content, string channel) =>
        new(Guid.NewGuid(), sender, content, channel, DateTimeOffset.UtcNow);

    /// <inheritdoc />
    public override string ToString() =>
        $"[{Channel}] {Sender} ({Timestamp:HH:mm:ss}): {Content}";
}
