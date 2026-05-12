namespace ChatStream.Theme5_Safety;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: Exception Filters  (C# 6)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  PROBLEM
//  -------
//  Before C# 6, you had to catch an exception, inspect it
//  inside the catch block, and re-throw if it didn't match.
//  This unwound the stack and destroyed diagnostic context.
//
//  SOLUTION
//  --------
//  The `when` clause on catch lets you filter exceptions
//  without catching them.  The CLR evaluates the filter
//  while the stack is still intact — perfect for logging
//  and selective handling.
//
//  WHY IT MATTERS
//  ──────────────
//  You can write precise error-handling pipelines that only
//  catch what they know how to handle.  Stack traces remain
//  pristine for unhandled cases.
//
//  TRY IT
//  ──────
//  1. Add a filter for TimeoutException with a specific message.
//  2. Chain two `when` filters on the same exception type.
//  3. Try logging in a `when` clause that always returns false.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Demonstrates exception filters for smart error handling in chat operations.
/// </summary>
public static class SmartErrorHandlerDemo
{
    // ── Custom exception types for the chat domain ──────────

    /// <summary>Chat-domain exception with an error code.</summary>
    private sealed class ChatException(string message, int errorCode)
        : Exception(message)
    {
        public int ErrorCode { get; } = errorCode;
    }

    /// <summary>Thrown when a connection attempt fails.</summary>
    private sealed class ConnectionException(string message, bool isTransient)
        : Exception(message)
    {
        public bool IsTransient { get; } = isTransient;
    }

    // ── Before: catch-and-rethrow anti-pattern ──────────────

    private static string HandleErrorOldWay(Action action)
    {
        try
        {
            action();
            return "Success";
        }
        catch (ChatException ex)
        {
            // Must catch ALL ChatExceptions, then re-throw
            // the ones we can't handle. This unwinds the stack.
            if (ex.ErrorCode == 404)
                return $"Not Found: {ex.Message}";

            throw; // Stack trace is preserved with throw; but
                   // the filter never ran at the CLR level.
        }
    }

    // ── After: exception filters with `when` ────────────────

    private static string HandleErrorNewWay(Action action)
    {
        try
        {
            action();
            return "Success";
        }
        // The `when` clause is evaluated BEFORE the stack unwinds.
        // If the filter returns false, the exception propagates
        // as if the catch block didn't exist.
        catch (ChatException ex) when (ex.ErrorCode == 404)
        {
            return $"Not Found: {ex.Message}";
        }
        catch (ChatException ex) when (ex.ErrorCode == 429)
        {
            return $"Rate Limited: {ex.Message}";
        }
        catch (ChatException ex) when (ex.ErrorCode >= 500)
        {
            return $"Server Error ({ex.ErrorCode}): {ex.Message}";
        }
    }

    /// <summary>
    /// Demonstrates filtering transient vs permanent connection errors.
    /// Only transient errors are retried.
    /// </summary>
    private static string HandleConnectionError(Action action, int maxRetries = 3)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                action();
                return $"Connected on attempt {attempt}";
            }
            // Only catch transient errors for retry.
            // Permanent errors fall through to the outer catch.
            catch (ConnectionException ex) when (ex.IsTransient && attempt < maxRetries)
            {
                Console.WriteLine($"    Transient error (attempt {attempt}/{maxRetries}): {ex.Message}");
                // In real code: await Task.Delay(backoff);
            }
        }
        return "Failed after all retries";
    }

    /// <summary>
    /// Uses a when-filter for logging without catching.
    /// The filter always returns false, so the exception propagates,
    /// but the side effect (logging) still runs.
    /// </summary>
    private static bool LogFilter(Exception ex)
    {
        // This runs while the stack is still intact!
        Console.WriteLine($"    [LOG] Exception in flight: {ex.GetType().Name} — {ex.Message}");
        return false; // Never actually catch it
    }

    public static void Run()
    {
        Console.WriteLine("╔═══ Exception Filters ═══╗\n");

        // ── 1. Filtering by error code ───────────────────────
        Console.WriteLine("── Filter by Error Code ──");
        Console.WriteLine($"  404: {HandleErrorNewWay(
            () => throw new ChatException("Channel #deleted not found", 404))}");
        Console.WriteLine($"  429: {HandleErrorNewWay(
            () => throw new ChatException("Slow down!", 429))}");
        Console.WriteLine($"  503: {HandleErrorNewWay(
            () => throw new ChatException("Service unavailable", 503))}");

        // ── 2. Transient vs permanent errors ─────────────────
        Console.WriteLine("\n── Transient Error Retry ──");
        int callCount = 0;
        var result = HandleConnectionError(() =>
        {
            callCount++;
            if (callCount < 3)
                throw new ConnectionException(
                    $"Connection refused (attempt {callCount})",
                    isTransient: true);
            // Third attempt succeeds
        });
        Console.WriteLine($"  Result: {result}");

        // ── 3. Permanent error — not retried ─────────────────
        Console.WriteLine("\n── Permanent Error (No Retry) ──");
        try
        {
            HandleConnectionError(() =>
                throw new ConnectionException(
                    "Invalid credentials",
                    isTransient: false));
        }
        catch (ConnectionException ex)
        {
            Console.WriteLine($"  Permanent error propagated: {ex.Message}");
        }

        // ── 4. Log-only filter (never catches) ──────────────
        Console.WriteLine("\n── Log-Only Filter ──");
        try
        {
            try
            {
                throw new InvalidOperationException("Chat room is full");
            }
            catch (Exception ex) when (LogFilter(ex))
            {
                // This block never executes because LogFilter returns false.
                Console.WriteLine("  This line never runs.");
            }
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"  Caught after logging: {ex.Message}");
        }

        // ── 5. Combining multiple when clauses ──────────────
        Console.WriteLine("\n── Combined Filters ──");
        var msg = ChatMessage.Create("Alice", "Hello!", "general");
        try
        {
            throw new ChatException("Delivery failed for important message", 500);
        }
        catch (ChatException ex) when (ex.ErrorCode >= 500 && msg.Channel == "general")
        {
            Console.WriteLine($"  Caught server error in #{msg.Channel}: {ex.Message}");
        }
    }
}
