namespace ChatStream.Theme8_Async;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: Task Combinators (WhenAll, WhenAny, WhenEach)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  PROBLEM
//  -------
//  You need to coordinate multiple async operations:
//  • Fetch from 3 servers, wait for all results
//  • Race 2 servers, use the fastest response
//  • Process results as they complete, not in order
//
//  SOLUTION
//  --------
//  Task combinators orchestrate multiple tasks:
//  • WhenAll — wait for all, get all results
//  • WhenAny — wait for first, get the winner
//  • WhenEach (.NET 9) — process each as it completes
//
//  WHY IT MATTERS
//  ──────────────
//  Chat apps talk to multiple services simultaneously:
//  message delivery, presence updates, typing indicators.
//  Combinators let you parallelize without threads.
//
//  TRY IT
//  ──────
//  1. Add error handling to WhenAll (AggregateException).
//  2. Implement a timeout with WhenAny + Task.Delay.
//  3. Use WhenEach to build a progress indicator.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Demonstrates Task combinators for parallel chat operations.
/// </summary>
public static class ParallelFetchDemo
{
    /// <summary>Simulates fetching messages from a server with variable latency.</summary>
    private static async Task<List<ChatMessage>> FetchFromServerAsync(
        string server, int latencyMs, int count, CancellationToken ct = default)
    {
        Console.WriteLine($"    [{server}] Starting fetch ({latencyMs}ms latency)...");
        await Task.Delay(latencyMs, ct);

        var messages = Enumerable.Range(1, count)
            .Select(i => ChatMessage.Create(server, $"Msg {i} from {server}", "sync"))
            .ToList();

        Console.WriteLine($"    [{server}] Completed — {count} messages.");
        return messages;
    }

    /// <summary>Simulates a health check with variable response time.</summary>
    private static async Task<(string Server, bool Healthy, int LatencyMs)> HealthCheckAsync(
        string server, int latencyMs, bool healthy, CancellationToken ct = default)
    {
        await Task.Delay(latencyMs, ct);
        return (server, healthy, latencyMs);
    }

    public static async Task RunAsync()
    {
        Console.WriteLine("╔═══ Task Combinators ═══╗\n");

        // ── 1. Task.WhenAll — parallel fetch ─────────────────
        Console.WriteLine("── Task.WhenAll ──");
        Console.WriteLine("  Fetching from 3 servers in parallel:");

        var fetchTasks = new[]
        {
            FetchFromServerAsync("US-East", 100, 3),
            FetchFromServerAsync("EU-West", 150, 2),
            FetchFromServerAsync("AP-South", 80, 4),
        };

        // WhenAll runs all tasks concurrently.
        // Total time ≈ max(100, 150, 80) = ~150ms, not 330ms!
        var allResults = await Task.WhenAll(fetchTasks);
        var totalMessages = allResults.Sum(r => r.Count);
        Console.WriteLine($"  Total messages: {totalMessages} from {allResults.Length} servers\n");

        // ── 2. Task.WhenAll with error handling ──────────────
        Console.WriteLine("── WhenAll Error Handling ──");
        try
        {
            await Task.WhenAll(
                FetchFromServerAsync("Good-1", 50, 1),
                Task.Run<List<ChatMessage>>(new Func<List<ChatMessage>>(() =>
                    throw new InvalidOperationException("Server-2 is down!"))),
                FetchFromServerAsync("Good-3", 70, 1)
            );
        }
        catch (InvalidOperationException ex)
        {
            // WhenAll throws the first exception, but ALL exceptions
            // are captured in the returned Task.Exception property.
            Console.WriteLine($"  Caught: {ex.Message}");
            Console.WriteLine("  (WhenAll waits for ALL tasks, even if one fails)\n");
        }

        // ── 3. Task.WhenAny — first responder ───────────────
        Console.WriteLine("── Task.WhenAny (Race) ──");
        Console.WriteLine("  Racing 3 servers — first response wins:");

        var raceTasks = new[]
        {
            FetchFromServerAsync("Slow", 200, 1),
            FetchFromServerAsync("Medium", 100, 1),
            FetchFromServerAsync("Fast", 50, 1),
        };

        var winner = await Task.WhenAny(raceTasks);
        var winnerResult = await winner; // Safe — task is already completed
        Console.WriteLine($"  Winner: {winnerResult[0].Sender} with {winnerResult.Count} messages\n");

        // ── 4. WhenAny for timeout pattern ───────────────────
        Console.WriteLine("── WhenAny Timeout Pattern ──");
        var fetchTask = FetchFromServerAsync("Remote", 300, 1);
        var timeoutTask = Task.Delay(100);

        var completed = await Task.WhenAny(fetchTask, timeoutTask);
        if (completed == timeoutTask)
        {
            Console.WriteLine("  Timed out waiting for Remote server.");
        }
        else
        {
            Console.WriteLine($"  Received: {(await fetchTask).Count} messages.");
        }
        Console.WriteLine();

        // ── 5. Task.WhenEach (.NET 9) ────────────────────────
        Console.WriteLine("── Task.WhenEach (.NET 9) ──");
        Console.WriteLine("  Processing results as they complete:");

        var healthChecks = new[]
        {
            HealthCheckAsync("US-East", 120, true),
            HealthCheckAsync("EU-West", 60, true),
            HealthCheckAsync("AP-South", 180, false),
            HealthCheckAsync("AF-North", 30, true),
        };

        // Task.WhenEach yields tasks in completion order!
        // This is a game-changer vs the old WhenAny-in-a-loop pattern.
        int order = 0;
        await foreach (var completedTask in Task.WhenEach(healthChecks))
        {
            order++;
            var (server, healthy, latency) = await completedTask;
            var status = healthy ? "✓ Healthy" : "✗ Unhealthy";
            Console.WriteLine($"    {order}. [{server}] {status} ({latency}ms)");
        }
        Console.WriteLine();

        // ── 6. Comparison: old WhenAny loop vs WhenEach ──────
        Console.WriteLine("── Old vs New: Processing in Completion Order ──");
        Console.WriteLine("  Old pattern (WhenAny loop):");
        Console.WriteLine("    var remaining = new List<Task<T>>(tasks);");
        Console.WriteLine("    while (remaining.Count > 0)");
        Console.WriteLine("    {");
        Console.WriteLine("        var done = await Task.WhenAny(remaining);");
        Console.WriteLine("        remaining.Remove(done);");
        Console.WriteLine("        Process(await done);  // O(n²) removal!");
        Console.WriteLine("    }");
        Console.WriteLine();
        Console.WriteLine("  New pattern (WhenEach):");
        Console.WriteLine("    await foreach (var task in Task.WhenEach(tasks))");
        Console.WriteLine("        Process(await task);  // Clean and efficient!");

        // ── 7. Key insight ───────────────────────────────────
        Console.WriteLine("\n── Key Insight ──");
        Console.WriteLine("  WhenAll  → Wait for ALL, get ALL results");
        Console.WriteLine("  WhenAny  → Wait for FIRST, get ONE result");
        Console.WriteLine("  WhenEach → Stream results AS THEY COMPLETE");
        Console.WriteLine("  Combine with CancellationToken for robust patterns!");
    }
}
