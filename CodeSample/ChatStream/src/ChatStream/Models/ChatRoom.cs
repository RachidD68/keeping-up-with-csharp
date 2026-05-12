namespace ChatStream.Models;

/// <summary>
/// Manages users, message history, and async message streaming.
/// </summary>
public sealed class ChatRoom
{
    private readonly List<User> _users = [];
    private readonly List<ChatMessage> _history = [];
    private readonly ChatChannel _channel;

    /// <summary>Creates a new chat room.</summary>
    public ChatRoom(string name, int capacity = 100)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        Name = name;
        _channel = new ChatChannel(name, capacity);
    }

    /// <summary>Room name.</summary>
    public string Name { get; }

    /// <summary>Currently joined users.</summary>
    public IReadOnlyList<User> Users => _users.AsReadOnly();

    /// <summary>Message history.</summary>
    public IReadOnlyList<ChatMessage> History => _history.AsReadOnly();

    /// <summary>The underlying channel for async messaging.</summary>
    public ChatChannel Channel => _channel;

    /// <summary>Adds a user to the room.</summary>
    public void Join(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        if (_users.All(u => u.Name != user.Name))
            _users.Add(user);
    }

    /// <summary>Sends a message to the room.</summary>
    public async ValueTask SendAsync(ChatMessage message, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        _history.Add(message);
        await _channel.Writer.WriteAsync(message, ct);
    }

    /// <summary>Reads messages as an async stream.</summary>
    public IAsyncEnumerable<ChatMessage> ReadMessagesAsync(CancellationToken ct = default) =>
        _channel.Reader.ReadAllAsync(ct);

    /// <inheritdoc />
    public override string ToString() =>
        $"Room '{Name}': {_users.Count} users, {_history.Count} messages";
}
