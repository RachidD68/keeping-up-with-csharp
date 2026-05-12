namespace ChatStream.Models;

/// <summary>
/// A chat channel backed by <see cref="Channel{T}"/> for async pub/sub.
/// </summary>
public sealed class ChatChannel
{
    private readonly Channel<ChatMessage> _channel;

    /// <summary>The channel name.</summary>
    public string Name { get; }

    /// <summary>The writer for producing messages.</summary>
    public ChannelWriter<ChatMessage> Writer => _channel.Writer;

    /// <summary>The reader for consuming messages.</summary>
    public ChannelReader<ChatMessage> Reader => _channel.Reader;

    /// <summary>Creates a bounded chat channel with backpressure.</summary>
    public ChatChannel(string name, int capacity = 100)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        Name = name;
        _channel = Channel.CreateBounded<ChatMessage>(
            new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = false,
                SingleWriter = false
            });
    }

    /// <summary>Creates an unbounded chat channel.</summary>
    public static ChatChannel CreateUnbounded(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        var ch = new ChatChannel(name, int.MaxValue);
        return ch;
    }

    /// <inheritdoc />
    public override string ToString() => $"Channel: {Name}";
}
