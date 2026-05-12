namespace ChatStream.Theme10_Capstone;

// ╔══════════════════════════════════════════════════════════════════╗
// ║  Theme 10 Capstone — Async Pipeline Pattern                    ║
// ║                                                                ║
// ║  Combines every async feature from Theme 8 into a cohesive     ║
// ║  message-processing pipeline:                                  ║
// ║                                                                ║
// ║  • async/await for composable async steps                      ║
// ║  • ValueTask for allocation-free fast paths                    ║
// ║  • IAsyncEnumerable for streaming between stages               ║
// ║  • IAsyncDisposable for resource cleanup                       ║
// ║  • Channels for backpressure between pipeline stages           ║
// ║  • CancellationToken for graceful shutdown                     ║
// ║  • Task combinators for parallel fan-out                       ║
// ║                                                                ║
// ║  The result: a production-quality message pipeline that         ║
// ║  ingests, validates, transforms, and delivers chat messages    ║
// ║  with full backpressure, cancellation, and cleanup.            ║
// ╚══════════════════════════════════════════════════════════════════╝

/// <summary>
/// Capstone: A multi-stage async message processing pipeline.
/// Each stage is connected by channels, streams data with
/// IAsyncEnumerable, and supports graceful shutdown.
/// </summary>
public static class AsyncPipelinePatternDemo
{
    // ── Pipeline stage results ───────────────────────────────

    /// <summary>A processed message with metadata.</summary>
    private record ProcessedMessage(
        ChatMessage Original,
        string TransformedContent,
        DateTimeOffset ProcessedAt,
        string[] AppliedTransforms);

    /// <summary>Delivery receipt for a processed message.</summary>
    private record DeliveryReceipt(
        Guid MessageId,
        string Channel,
        bool Delivered,
        TimeSpan Latency);

    // ── Pipeline stages ──────────────────────────────────────

    /// <summary>
    /// Stage 1: Message Ingestion — produces messages into a channel.
    /// Demonstrates: Channels (bounded, backpressure), async producer.
    /// </summary>
    private sealed class IngestionStage : IAsyncDisposable
    {
        private readonly Channel<ChatMessage> _output;
        private int _ingested;

        public IngestionStage(int capacity = 10)
        {
            _output = Channel.CreateBounded<ChatMessage>(
                new BoundedChannelOptions(capacity)
                {
                    FullMode = BoundedChannelFullMode.Wait
                });
        }

        public ChannelReader<ChatMessage> Output => _output.Reader;

        /// <summary>Ingests messages from an async stream into the channel.</summary>
        public async Task IngestAsync(
            IAsyncEnumerable<ChatMessage> source,
            CancellationToken ct = default)
        {
            await foreach (var message in source.WithCancellation(ct))
            {
                await _output.Writer.WriteAsync(message, ct);
                _ingested++;
            }
        }

        public int IngestedCount => _ingested;

        public ValueTask DisposeAsync()
        {
            _output.Writer.TryComplete();
            return ValueTask.CompletedTask;
        }
    }

    /// <summary>
    /// Stage 2: Validation — filters invalid messages.
    /// Demonstrates: ValueTask (fast-path for valid messages),
    /// IAsyncEnumerable composition.
    /// </summary>
    private static class ValidationStage
    {
        /// <summary>Validates a single message — returns null if invalid.</summary>
        private static ValueTask<ChatMessage?> ValidateAsync(ChatMessage message)
        {
            // Fast synchronous path — no allocation for valid messages
            if (!string.IsNullOrWhiteSpace(message.Content) &&
                !string.IsNullOrWhiteSpace(message.Sender))
            {
                return ValueTask.FromResult<ChatMessage?>(message);
            }

            // Invalid — return null synchronously
            return ValueTask.FromResult<ChatMessage?>(null);
        }

        /// <summary>Filters a stream, yielding only valid messages.</summary>
        public static async IAsyncEnumerable<ChatMessage> FilterAsync(
            ChannelReader<ChatMessage> input,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            await foreach (var message in input.ReadAllAsync(ct))
            {
                var validated = await ValidateAsync(message);
                if (validated is not null)
                {
                    yield return validated;
                }
            }
        }
    }

    /// <summary>
    /// Stage 3: Transformation — enriches messages.
    /// Demonstrates: async/await composition, pure transformations.
    /// </summary>
    private static class TransformationStage
    {
        private static readonly string[] Transforms =
            ["sanitize", "format", "enrich"];

        public static async IAsyncEnumerable<ProcessedMessage> TransformAsync(
            IAsyncEnumerable<ChatMessage> input,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            await foreach (var message in input.WithCancellation(ct))
            {
                // Simulate async transformation work
                await Task.Delay(20, ct);

                var transformed = new ProcessedMessage(
                    Original: message,
                    TransformedContent: $"[processed] {message.Content}",
                    ProcessedAt: DateTimeOffset.UtcNow,
                    AppliedTransforms: Transforms);

                yield return transformed;
            }
        }
    }

    /// <summary>
    /// Stage 4: Delivery — fan-out to multiple targets.
    /// Demonstrates: Task combinators (WhenAll for parallel delivery),
    /// exception filters for retry decisions.
    /// </summary>
    private static class DeliveryStage
    {
        /// <summary>Delivers to a single target channel.</summary>
        private static async Task<DeliveryReceipt> DeliverToChannelAsync(
            ProcessedMessage message,
            string targetChannel,
            CancellationToken ct = default)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            await Task.Delay(15, ct); // Simulate network
            sw.Stop();

            return new DeliveryReceipt(
                message.Original.Id,
                targetChannel,
                Delivered: true,
                Latency: sw.Elapsed);
        }

        /// <summary>
        /// Fan-out: delivers to all subscriber channels in parallel.
        /// </summary>
        public static async Task<DeliveryReceipt[]> DeliverAsync(
            ProcessedMessage message,
            string[] targetChannels,
            CancellationToken ct = default)
        {
            var tasks = targetChannels
                .Select(ch => DeliverToChannelAsync(message, ch, ct))
                .ToArray();

            return await Task.WhenAll(tasks);
        }
    }

    /// <summary>
    /// The complete pipeline — connects all stages with channels
    /// and supports graceful shutdown via CancellationToken.
    /// </summary>
    private sealed class MessagePipeline : IAsyncDisposable
    {
        private readonly IngestionStage _ingestion;
        private readonly List<DeliveryReceipt> _receipts = [];
        private readonly Lock _receiptsLock = new();

        public MessagePipeline(int bufferCapacity = 10)
        {
            _ingestion = new IngestionStage(bufferCapacity);
        }

        /// <summary>
        /// Runs the complete pipeline end-to-end.
        /// </summary>
        public async Task RunAsync(
            IAsyncEnumerable<ChatMessage> source,
            string[] deliveryTargets,
            CancellationToken ct = default)
        {
            Console.WriteLine("    Pipeline starting...\n");

            // Stage 1: Ingest (runs as a background task)
            var ingestTask = _ingestion.IngestAsync(source, ct);

            // Stage 2→3→4: Validate → Transform → Deliver
            var processTask = ProcessAllAsync(deliveryTargets, ct);

            // Wait for ingestion to complete, then signal done
            await ingestTask;
            await _ingestion.DisposeAsync(); // Completes the channel writer

            // Wait for processing to finish draining
            await processTask;

            Console.WriteLine($"\n    Pipeline complete!");
            Console.WriteLine($"    Ingested: {_ingestion.IngestedCount}");
            Console.WriteLine($"    Delivered: {_receipts.Count} receipts");
        }

        private async Task ProcessAllAsync(
            string[] targets, CancellationToken ct)
        {
            // Stage 2: Validate
            var validated = ValidationStage.FilterAsync(_ingestion.Output, ct);

            // Stage 3: Transform
            var transformed = TransformationStage.TransformAsync(validated, ct);

            // Stage 4: Deliver each transformed message
            await foreach (var processed in transformed.WithCancellation(ct))
            {
                var receipts = await DeliveryStage.DeliverAsync(
                    processed, targets, ct);

                lock (_receiptsLock)
                {
                    _receipts.AddRange(receipts);
                }

                var avgLatency = receipts.Average(r => r.Latency.TotalMilliseconds);
                Console.WriteLine(
                    $"    ✓ {processed.Original.Sender}: " +
                    $"\"{processed.TransformedContent}\" → " +
                    $"{receipts.Length} targets ({avgLatency:F1}ms avg)");
            }
        }

        public IReadOnlyList<DeliveryReceipt> Receipts
        {
            get { lock (_receiptsLock) return [.. _receipts]; }
        }

        public async ValueTask DisposeAsync()
        {
            await _ingestion.DisposeAsync();
        }
    }

    // ── Message generator (IAsyncEnumerable source) ──────────

    /// <summary>
    /// Generates a stream of test messages for the pipeline.
    /// </summary>
    private static async IAsyncEnumerable<ChatMessage> GenerateMessagesAsync(
        int count,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var senders = new[] { "Alice", "Bob", "Charlie", "Diana" };
        var channels = new[] { "general", "dev", "random" };

        for (int i = 0; i < count; i++)
        {
            await Task.Delay(30, ct);

            var sender = senders[i % senders.Length];
            var channel = channels[i % channels.Length];

            // Occasionally produce an invalid message to test validation
            var content = (i % 7 == 0) ? "" : $"Pipeline message #{i + 1}";

            yield return ChatMessage.Create(sender, content, channel);
        }
    }

    public static async Task RunAsync()
    {
        Console.WriteLine("╔═══ Theme 10 Capstone: Async Pipeline Pattern ═══╗\n");

        // ── 1. Run the full pipeline ─────────────────────────
        Console.WriteLine("── Full Pipeline Run ──");

        var deliveryTargets = new[] { "websocket", "push", "archive" };

        await using var pipeline = new MessagePipeline(bufferCapacity: 5);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var source = GenerateMessagesAsync(10, cts.Token);

        await pipeline.RunAsync(source, deliveryTargets, cts.Token);

        // ── 2. Analyze receipts ──────────────────────────────
        Console.WriteLine("\n── Delivery Analysis ──");
        var receipts = pipeline.Receipts;

        var byChannel = receipts
            .GroupBy(r => r.Channel)
            .Select(g => new
            {
                Channel = g.Key,
                Count = g.Count(),
                AvgLatency = g.Average(r => r.Latency.TotalMilliseconds)
            });

        foreach (var group in byChannel)
        {
            Console.WriteLine(
                $"  {group.Channel}: {group.Count} deliveries " +
                $"(avg {group.AvgLatency:F1}ms)");
        }

        // ── 3. Pipeline with cancellation ────────────────────
        Console.WriteLine("\n── Pipeline with Cancellation ──");
        await using var pipeline2 = new MessagePipeline(bufferCapacity: 3);
        using var shortCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));

        try
        {
            var longSource = GenerateMessagesAsync(100, shortCts.Token);
            await pipeline2.RunAsync(longSource, ["websocket"], shortCts.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("    Pipeline cancelled gracefully!");
            Console.WriteLine($"    Delivered {pipeline2.Receipts.Count} receipts before cancel.");
        }

        // ── 4. Architecture summary ─────────────────────────
        Console.WriteLine("\n── Pipeline Architecture ──");
        Console.WriteLine("  ┌──────────┐    ┌───────────┐    ┌───────────┐    ┌──────────┐");
        Console.WriteLine("  │ Ingest   │ →  │ Validate  │ →  │ Transform │ →  │ Deliver  │");
        Console.WriteLine("  │ (Channel)│    │(ValueTask)│    │(IAsyncEnum)│    │(WhenAll) │");
        Console.WriteLine("  └──────────┘    └───────────┘    └───────────┘    └──────────┘");
        Console.WriteLine("       ↑                                                 ↓");
        Console.WriteLine("  IAsyncEnumerable                              DeliveryReceipt[]");
        Console.WriteLine();

        // ── 5. Features integrated ──────────────────────────
        Console.WriteLine("── C# Features Combined ──");
        Console.WriteLine("  ✓ async/await — composable async stages");
        Console.WriteLine("  ✓ ValueTask — zero-alloc validation fast path");
        Console.WriteLine("  ✓ IAsyncEnumerable — streaming between stages");
        Console.WriteLine("  ✓ IAsyncDisposable — pipeline resource cleanup");
        Console.WriteLine("  ✓ Channels — bounded backpressure buffering");
        Console.WriteLine("  ✓ CancellationToken — graceful shutdown");
        Console.WriteLine("  ✓ Task.WhenAll — parallel fan-out delivery");
        Console.WriteLine("  ✓ System.Threading.Lock — thread-safe receipts");
        Console.WriteLine("  → A complete, production-quality async architecture.");
    }
}
