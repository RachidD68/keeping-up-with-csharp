namespace ChatStream.Models;

/// <summary>
/// Filters chat messages using null-safe patterns.
/// </summary>
public sealed class MessageFilter
{
    /// <summary>Filter by sender name (null = no filter).</summary>
    public string? SenderFilter { get; init; }

    /// <summary>Filter by channel name (null = no filter).</summary>
    public string? ChannelFilter { get; init; }

    /// <summary>Filter by content keyword (null = no filter).</summary>
    public string? ContentKeyword { get; init; }

    /// <summary>Filter by minimum timestamp (null = no filter).</summary>
    public DateTimeOffset? After { get; init; }

    /// <summary>Applies all active filters to a message.</summary>
    public bool Matches(ChatMessage? message)
    {
        if (message is null) return false;

        if (SenderFilter is not null && message.Sender != SenderFilter)
            return false;
        if (ChannelFilter is not null && message.Channel != ChannelFilter)
            return false;
        if (ContentKeyword is not null &&
            !message.Content.Contains(ContentKeyword, StringComparison.OrdinalIgnoreCase))
            return false;
        if (After.HasValue && message.Timestamp < After.Value)
            return false;

        return true;
    }
}
