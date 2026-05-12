namespace ChatStream.Theme8_Async;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: System.Threading.Channels  (.NET Core 3.0+)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  PROBLEM
//  -------
//  Producer-consumer patterns are fundamental in async
//  systems: one part produces data, another consumes it,
//  at different rates.  Before Channels, you needed
//  ConcurrentQueue + SemaphoreSlim, or BlockingCollection
//  (which blocks threads).
//
//  SOLUTION
//  --------
//  System.Threading.Channels provides thread-safe, async-
//  native, backpressure-aware producer-consumer queues.
//  Bounded channels apply backpressure when full; unbounded
//  channels grow without limit.
//
//  WHY IT MATTERS
//  ──────────────
//  Channels are the backbone of ChatStream.  They decouple
//  message producers (senders) from consumers (receivers),
//  support multiple readers/writers, and integrate naturally
//  with IAsyncEnumerable.
//
//  TRY IT
//  ──────
//  1. Create a bounded channel with capacity 5 and observe backpressure.
//  2. Add a second consumer — messages are distributed, not duplicated.
//  3. Use ChannelReader.TryRead for non-blocking polling.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Demonstrates System.Threading.Channels as a message bus.
/// </summary>
public static class MessageBusDemo
{
    /// <summary>
    /// A multi-topic message bus using channels.
    /// Each topic has its own bounded channel with backpressure.
    /// </summary>
    private sealed class TopicMessageBus : IAsyncDisposable
    {
        private readonly Dictionary<string, Channel<ChatMessage>> _topics = new();
        private readonly Lock _topicLock = new();
        private readonly int _capacity;
        private bool _disposed;

        public TopicMessageBus(int capacityPerTopic = 10)
        {
            _capacity = capacityPerTopic;
        }

        /// <summary>Gets or creates a channel for the given topic.</summary>
        private Channel<ChatMessage> GetOrCreateTopic(string topic)
        {
            lock (_topicLock)
            {
                if (!_topics.TryGetValue(topic, out var channel))
                {
                    channel = Channel.CreateBounded<ChatMessage>(
                        new BoundedChannelOptions(_capacity)
                        {
                            FullMode = BoundedChannelFullMode.Wait,
                            SingleReader = false,
                            SingleWriter = false
                        });
                    _topics[topic] = channel;
                }
                return channel;
            }
        }

        /// <summary>
        /// Publishes a message to a topic.
        /// Applies backpressure if the channel is full.
        /// </summary>
        public async ValueTask PublishAsync(
            string topic, ChatMessage message, CancellationToken ct = default)
        {
            var channel = GetOrCreateTopic(topic);
            await channel.Writer.WriteAsync(message, ct);
        }

        /// <summary>
        /// Subscribes to a topic, receiving messages as an async stream.
        /// </summary>
        public IAsyncEnumerable<ChatMessage> SubscribeAsync(
            string topic, CancellationToken ct = default)
        {
            var channel = GetOrCreateTopic(topic);
            return channel.Reader.ReadAllAsync(ct);
        }

        /// <summary>Tries to read a message without blocking.</summary>
        public bool TryRead(string topic, out ChatMessage? message)
        {
            message = null;
            lock (_topicLock)
            {
                if (_topics.TryGetValue(topic, out var channel))
                    return channel.Reader.TryRead(out message);
            }
            return false;
        }

        /// <summary>Completes all topic channels.</summary>
        public ValueTask DisposeAsync()
        {
            if (_disposed) return ValueTask.CompletedTask;
            _disposed = true;

            lock (_topicLock)
            {
                foreach (var (_, channel) in _topics)
                    channel.Writer.TryComplete();
            }
            return ValueTask.CompletedTask;
        }
    }

    /// <summary>
    /// Demonstrates basic bounded channel with backpressure.
    /// </summary>
    private static async Task DemoBackpressureAsync()
    {
        Console.WriteLine("── Bounded Channel with Backpressure ──");

        // Create a tiny channel to demonstrate backpressure.
        var channel = Channel.CreateBounded<ChatMessage>(
            new BoundedChannelOptions(3)
            {
                FullMode = BoundedChannelFullMode.Wait
            });

        // Producer: writes 5 messages to a channel with capacity 3
        var producer = Task.Run(async () =>
        {
            for (int i = 1; i <= 5; i++)
            {
                var msg = ChatMessage.Create("Producer", $"Item {i}", "backpressure");
                Console.WriteLine($"    Producing item {i}...");
                await channel.Writer.WriteAsync(msg);
                Console.WriteLine($"    Produced item {i} ✓");
            }
            channel.Writer.Complete();
        });

        // Consumer: reads slowly, creating backpressure
        var consumer = Task.Run(async () =>
        {
            await foreach (var msg in channel.Reader.ReadAllAsync())
            {
                Console.WriteLine($"    Consuming: {msg.Content}");
                await Task.Delay(100); // Slow consumer
            }
        });

        await Task.WhenAll(producer, consumer);
    }

    /// <summary>
    /// Demonstrates multiple producers writing to one channel.
    /// </summary>
    private static async Task DemoMultiProducerAsync()
    {
        Console.WriteLine("── Multiple Producers ──");

        var channel = Channel.CreateBounded<ChatMessage>(20);

        var producers = new[]
        {
            ProduceMessagesAsync(channel.Writer, "Alice", 3),
            ProduceMessagesAsync(channel.Writer, "Bob", 3),
            ProduceMessagesAsync(channel.Writer, "Charlie", 3),
        };

        // When all producers finish, complete the writer
        _ = Task.WhenAll(producers).ContinueWith(_ =>
            channel.Writer.TryComplete());

        // Single consumer reads from all producers
        int count = 0;
        await foreach (var msg in channel.Reader.ReadAllAsync())
        {
            count++;
            Console.WriteLine($"    [{count}] {msg}");
        }
        Console.WriteLine($"  Total consumed: {count} messages");

        static async Task ProduceMessagesAsync(
            ChannelWriter<ChatMessage> writer, string sender, int count)
        {
            for (int i = 1; i <= count; i++)
            {
                await writer.WriteAsync(
                    ChatMessage.Create(sender, $"Message {i}", "multi"));
                await Task.Delay(30);
            }
        }
    }

    /// <summary>
    /// Demonstrates the FullMode options for bounded channels.
    /// </summary>
    private static async Task DemoFullModesAsync()
    {
        Console.WriteLine("── Channel Full Modes ──");

        // DropOldest: when full, the oldest item is silently dropped
        var dropOldest = Channel.CreateBounded<ChatMessage>(
            new BoundedChannelOptions(2)
            {
                FullMode = BoundedChannelFullMode.DropOldest
            });

        await dropOldest.Writer.WriteAsync(ChatMessage.Create("A", "First", "drop"));
        await dropOldest.Writer.WriteAsync(ChatMessage.Create("B", "Second", "drop"));
        await dropOldest.Writer.WriteAsync(ChatMessage.Create("C", "Third (drops First)", "drop"));
        dropOldest.Writer.Complete();

        Console.Write("  DropOldest (capacity 2, wrote 3): ");
        await foreach (var msg in dropOldest.Reader.ReadAllAsync())
            Console.Write($"[{msg.Sender}:{msg.Content}] ");
        Console.WriteLine();

        // DropNewest: when full, the newest item already IN THE BUFFER is
        // removed to make room for the incoming write.
        // Buffer [First, Second] is full → "Second" (newest in buffer) is
        // evicted, "Third" is written → result: [First, Third]
        var dropNewest = Channel.CreateBounded<ChatMessage>(
            new BoundedChannelOptions(2)
            {
                FullMode = BoundedChannelFullMode.DropNewest
            });

        await dropNewest.Writer.WriteAsync(ChatMessage.Create("A", "First", "drop"));
        await dropNewest.Writer.WriteAsync(ChatMessage.Create("B", "Second (evicted)", "drop"));
        await dropNewest.Writer.WriteAsync(ChatMessage.Create("C", "Third", "drop"));
        dropNewest.Writer.Complete();

        Console.Write("  DropNewest (capacity 2, wrote 3): ");
        await foreach (var msg in dropNewest.Reader.ReadAllAsync())
            Console.Write($"[{msg.Sender}:{msg.Content}] ");
        Console.WriteLine();

        // DropWrite: the write operation itself is dropped
        var dropWrite = Channel.CreateBounded<ChatMessage>(
            new BoundedChannelOptions(2)
            {
                FullMode = BoundedChannelFullMode.DropWrite
            });

        await dropWrite.Writer.WriteAsync(ChatMessage.Create("A", "First", "drop"));
        await dropWrite.Writer.WriteAsync(ChatMessage.Create("B", "Second", "drop"));
        await dropWrite.Writer.WriteAsync(ChatMessage.Create("C", "Third (dropped)", "drop"));
        dropWrite.Writer.Complete();

        Console.Write("  DropWrite (capacity 2, wrote 3):  ");
        await foreach (var msg in dropWrite.Reader.ReadAllAsync())
            Console.Write($"[{msg.Sender}:{msg.Content}] ");
        Console.WriteLine();
    }

    public static async Task RunAsync()
    {
        Console.WriteLine("╔═══ System.Threading.Channels ═══╗\n");

        // ── 1. Backpressure ──────────────────────────────────
        await DemoBackpressureAsync();
        Console.WriteLine();

        // ── 2. Multi-producer ────────────────────────────────
        await DemoMultiProducerAsync();
        Console.WriteLine();

        // ── 3. Full modes ────────────────────────────────────
        await DemoFullModesAsync();
        Console.WriteLine();

        // ── 4. Topic-based message bus ───────────────────────
        Console.WriteLine("── Topic Message Bus ──");
        await using var bus = new TopicMessageBus(capacityPerTopic: 5);

        // Publish to multiple topics
        await bus.PublishAsync("general",
            ChatMessage.Create("Alice", "Hello general!", "general"));
        await bus.PublishAsync("dev",
            ChatMessage.Create("Bob", "Bug fix incoming", "dev"));
        await bus.PublishAsync("general",
            ChatMessage.Create("Charlie", "Good morning!", "general"));
        await bus.PublishAsync("dev",
            ChatMessage.Create("Alice", "PR review please", "dev"));

        // Non-blocking read
        Console.WriteLine("  Non-blocking reads:");
        while (bus.TryRead("general", out var msg))
            Console.WriteLine($"    #general: {msg}");
        while (bus.TryRead("dev", out var devMsg))
            Console.WriteLine($"    #dev: {devMsg}");

        // ── 5. Channel vs alternatives ───────────────────────
        Console.WriteLine("\n── Channels vs Alternatives ──");
        Console.WriteLine("  ┌─────────────────────┬─────────┬───────────┬────────────┐");
        Console.WriteLine("  │ Feature             │ Channel │ ConcQueue │ BlockingCol│");
        Console.WriteLine("  ├─────────────────────┼─────────┼───────────┼────────────┤");
        Console.WriteLine("  │ Async-native        │ ✓       │ ✗         │ ✗          │");
        Console.WriteLine("  │ Backpressure        │ ✓       │ ✗         │ ✓ (blocks) │");
        Console.WriteLine("  │ IAsyncEnumerable    │ ✓       │ ✗         │ ✗          │");
        Console.WriteLine("  │ Completion signal   │ ✓       │ ✗         │ ✓          │");
        Console.WriteLine("  │ Non-blocking read   │ ✓       │ ✓         │ ✗          │");
        Console.WriteLine("  │ Bounded + unbounded │ ✓       │ unbounded │ bounded    │");
        Console.WriteLine("  └─────────────────────┴─────────┴───────────┴────────────┘");
    }
}
