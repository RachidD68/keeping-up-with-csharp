namespace ChatStream.Theme8_Async;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: Async State Machine (Compiler Internals)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  PROBLEM
//  -------
//  Many developers use async/await without understanding
//  what the compiler generates.  This leads to performance
//  anti-patterns like unnecessary async methods, captured
//  variables, and excessive allocations.
//
//  SOLUTION
//  --------
//  Understanding the generated state machine reveals WHY
//  certain patterns are faster (ValueTask, eliding async),
//  and WHY others are slower (unnecessary async wrapper,
//  capturing large structs).
//
//  WHY IT MATTERS
//  ──────────────
//  Knowledge of the state machine helps you:
//  • Know when to use ValueTask vs Task
//  • Avoid unnecessary async wrappers
//  • Understand ConfigureAwait behavior
//  • Debug async code in the debugger
//
//  TRY IT
//  ──────
//  1. Decompile an async method with ILSpy to see the state machine.
//  2. Compare the IL of an async method vs a sync method.
//  3. Use [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))].
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Explores how the compiler transforms async methods into state machines.
/// </summary>
public static class StateMachineExplorerDemo
{
    // ╔══════════════════════════════════════════════════════════╗
    // ║  What the compiler generates for an async method:        ║
    // ║                                                          ║
    // ║  async Task<string> GetGreetingAsync(string name)        ║
    // ║  {                                                       ║
    // ║      await Task.Delay(100);                              ║
    // ║      return $"Hello, {name}!";                           ║
    // ║  }                                                       ║
    // ║                                                          ║
    // ║  Becomes roughly:                                        ║
    // ║                                                          ║
    // ║  [CompilerGenerated]                                     ║
    // ║  struct <GetGreetingAsync>d__0 : IAsyncStateMachine      ║
    // ║  {                                                       ║
    // ║      public int <>1__state;         // -1=start, 0=await ║
    // ║      public AsyncTaskMethodBuilder<string> <>t__builder; ║
    // ║      public string name;            // captured param    ║
    // ║      private TaskAwaiter <>u__1;     // awaiter          ║
    // ║                                                          ║
    // ║      void IAsyncStateMachine.MoveNext()                  ║
    // ║      {                                                   ║
    // ║          switch (<>1__state)                              ║
    // ║          {                                                ║
    // ║              case -1: // Initial                         ║
    // ║                  var awaiter = Task.Delay(100)            ║
    // ║                      .GetAwaiter();                      ║
    // ║                  if (!awaiter.IsCompleted)                ║
    // ║                  {                                       ║
    // ║                      <>1__state = 0;                     ║
    // ║                      <>u__1 = awaiter;                   ║
    // ║                      <>t__builder.AwaitUnsafe...         ║
    // ║                      return; // SUSPEND                  ║
    // ║                  }                                       ║
    // ║                  goto case 0;                             ║
    // ║              case 0: // Resume after await               ║
    // ║                  <>u__1.GetResult();                     ║
    // ║                  <>t__builder.SetResult(                  ║
    // ║                      $"Hello, {name}!");                 ║
    // ║                  break;                                   ║
    // ║          }                                                ║
    // ║      }                                                   ║
    // ║  }                                                       ║
    // ╚══════════════════════════════════════════════════════════╝

    /// <summary>
    /// A simple async method — the compiler transforms this
    /// into the state machine shown in the comment above.
    /// </summary>
    private static async Task<ChatMessage> ProcessMessageAsync(
        string sender, string content, string channel)
    {
        // State -1 (initial): Everything before the first await
        var message = ChatMessage.Create(sender, content, channel);
        Console.WriteLine($"    [State 0] Created message: {message.Id}");

        // First await — potential suspension point (state 0)
        await Task.Delay(50);
        Console.WriteLine("    [State 1] After first await — validation complete");

        // Second await — another suspension point (state 1)
        await Task.Delay(50);
        Console.WriteLine("    [State 2] After second await — delivery complete");

        // Final: SetResult called on the builder
        return message;
    }

    /// <summary>
    /// Anti-pattern: unnecessary async wrapper.
    /// The async keyword forces a state machine allocation
    /// even though there's only one await at the end.
    /// </summary>
    private static async Task<ChatMessage> UnnecessaryAsyncWrapper(
        string sender, string content, string channel)
    {
        // BAD: This creates a state machine for no reason.
        // The Task from CreateMessageAsync could be returned directly.
        return await Task.FromResult(
            ChatMessage.Create(sender, content, channel));
    }

    /// <summary>
    /// Better: elide async when just forwarding a task.
    /// No state machine generated — the Task passes through.
    /// </summary>
    private static Task<ChatMessage> ElideAsyncWrapper(
        string sender, string content, string channel)
    {
        // GOOD: No async keyword → no state machine → no allocation.
        // The Task is returned directly to the caller.
        return Task.FromResult(
            ChatMessage.Create(sender, content, channel));
    }

    /// <summary>
    /// Shows when you SHOULD keep async even for simple forwarding:
    /// when you need try/catch or using/finally.
    /// </summary>
    private static async Task<ChatMessage> KeepAsyncForSafety(
        string sender, string content, string channel)
    {
        // KEEP async here because we need try/catch.
        // Without async, exceptions from the task wouldn't be
        // caught by this try/catch block.
        try
        {
            await Task.Delay(10);
            return ChatMessage.Create(sender, content, channel);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"    Error: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Demonstrates multiple awaits creating multiple states.
    /// </summary>
    private static async Task<string> MultiStateMethodAsync(string channel)
    {
        // Each await creates a new state in the state machine.
        // The compiler generates a switch statement that jumps
        // to the right state on each MoveNext() call.

        Console.WriteLine("    State -1: Starting...");
        await Task.Delay(30); // Transition to state 0

        Console.WriteLine("    State  0: Connecting...");
        await Task.Delay(30); // Transition to state 1

        Console.WriteLine("    State  1: Authenticating...");
        await Task.Delay(30); // Transition to state 2

        Console.WriteLine("    State  2: Joining channel...");
        await Task.Delay(30); // Transition to state 3

        Console.WriteLine("    State  3: Ready!");
        return $"Connected to #{channel}";
    }

    public static async Task RunAsync()
    {
        Console.WriteLine("╔═══ Async State Machine Explorer ═══╗\n");

        // ── 1. Basic state machine flow ──────────────────────
        Console.WriteLine("── State Machine Flow ──");
        var msg = await ProcessMessageAsync("Alice", "Hello!", "general");
        Console.WriteLine($"  Result: {msg}\n");

        // ── 2. Multi-state method ────────────────────────────
        Console.WriteLine("── Multiple States ──");
        var status = await MultiStateMethodAsync("general");
        Console.WriteLine($"  Result: {status}\n");

        // ── 3. Async elision ─────────────────────────────────
        Console.WriteLine("── Async Elision ──");
        Console.WriteLine("  Unnecessary wrapper: creates a state machine for nothing.");
        var wrapped = await UnnecessaryAsyncWrapper("Bob", "Hi!", "general");
        Console.WriteLine($"  Result: {wrapped}");

        Console.WriteLine("  Elided wrapper: no state machine, no allocation.");
        var elided = await ElideAsyncWrapper("Bob", "Hi!", "general");
        Console.WriteLine($"  Result: {elided}\n");

        // ── 4. When to keep async ────────────────────────────
        Console.WriteLine("── When to Keep async ──");
        Console.WriteLine("  Keep async when you need:");
        Console.WriteLine("    • try/catch around awaited operations");
        Console.WriteLine("    • using statements with IAsyncDisposable");
        Console.WriteLine("    • finally blocks for cleanup");
        Console.WriteLine("    • Multiple awaits (obvious — can't elide)");

        var safe = await KeepAsyncForSafety("Alice", "Safe!", "general");
        Console.WriteLine($"  Result: {safe}\n");

        // ── 5. State machine structure ───────────────────────
        Console.WriteLine("── State Machine Anatomy ──");
        Console.WriteLine("  struct <Method>d__0 : IAsyncStateMachine");
        Console.WriteLine("  {");
        Console.WriteLine("      int state;         // Which await are we at?");
        Console.WriteLine("      Builder builder;   // Manages the Task lifecycle");
        Console.WriteLine("      T param;           // Captured parameters");
        Console.WriteLine("      Awaiter awaiter;   // Current awaiter");
        Console.WriteLine("  }");
        Console.WriteLine();
        Console.WriteLine("  Key points:");
        Console.WriteLine("  • Struct in .NET Core (class in .NET Framework)");
        Console.WriteLine("  • Boxed to heap on first real suspension");
        Console.WriteLine("  • If the task completes synchronously, no boxing!");
        Console.WriteLine("  • This is why ValueTask can avoid allocation.");
    }
}
