namespace ChatStream.Theme5_Safety;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: CallerArgumentExpression  (C# 10)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  PROBLEM
//  -------
//  Guard clauses need the parameter name for error messages.
//  Before C# 10, you either hardcoded the name as a string
//  (fragile under rename refactoring) or used nameof()
//  (verbose).
//
//  SOLUTION
//  --------
//  [CallerArgumentExpression("paramName")] captures the
//  source text of the argument at compile time.  The
//  compiler fills it in automatically — no magic strings.
//
//  WHY IT MATTERS
//  ──────────────
//  Guard methods become self-documenting.  Error messages
//  include the exact expression that was passed, not just
//  a parameter name.  This is how ArgumentNullException
//  .ThrowIfNull works internally.
//
//  TRY IT
//  ──────
//  1. Build Guard.Against.Null(), .Empty(), .OutOfRange().
//  2. Call Guard.Against.Null(user?.Name) — see the expression in the error.
//  3. Add Guard.Against.Predicate(x, v => v < 0, "must be non-negative").
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// A fluent guard clause library using CallerArgumentExpression.
/// This is the showcase file for this feature — build a full
/// Guard.Against pattern with rich, automatic error messages.
/// </summary>
public static class Guard
{
    /// <summary>
    /// Entry point for fluent guard clauses: Guard.Against.Null(value).
    /// </summary>
    public static GuardClause Against => new();
}

/// <summary>
/// Contains all guard clause methods. Each uses
/// CallerArgumentExpression to capture the source expression.
/// </summary>
public readonly struct GuardClause
{
    // ── Null Guards ──────────────────────────────────────────

    /// <summary>
    /// Throws if <paramref name="value"/> is null.
    /// The expression text is captured automatically.
    /// </summary>
    public T Null<T>(
        T? value,
        [CallerArgumentExpression(nameof(value))] string? expression = null)
        where T : class
    {
        if (value is null)
            throw new ArgumentNullException(
                expression,
                $"Value '{expression}' must not be null.");
        return value;
    }

    /// <summary>
    /// Throws if the nullable value type has no value.
    /// </summary>
    public T NullValue<T>(
        T? value,
        [CallerArgumentExpression(nameof(value))] string? expression = null)
        where T : struct
    {
        if (!value.HasValue)
            throw new ArgumentNullException(
                expression,
                $"Value '{expression}' must have a value.");
        return value.Value;
    }

    // ── String Guards ────────────────────────────────────────

    /// <summary>
    /// Throws if the string is null or empty.
    /// </summary>
    public string NullOrEmpty(
        string? value,
        [CallerArgumentExpression(nameof(value))] string? expression = null)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException(
                $"Value '{expression}' must not be null or empty.",
                expression);
        return value;
    }

    /// <summary>
    /// Throws if the string is null, empty, or whitespace.
    /// </summary>
    public string NullOrWhiteSpace(
        string? value,
        [CallerArgumentExpression(nameof(value))] string? expression = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(
                $"Value '{expression}' must not be null, empty, or whitespace.",
                expression);
        return value;
    }

    // ── Range Guards ─────────────────────────────────────────

    /// <summary>
    /// Throws if value is outside the specified range [min..max].
    /// </summary>
    public T OutOfRange<T>(
        T value,
        T min,
        T max,
        [CallerArgumentExpression(nameof(value))] string? expression = null)
        where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
            throw new ArgumentOutOfRangeException(
                expression,
                value,
                $"Value '{expression}' must be between {min} and {max}, but was {value}.");
        return value;
    }

    /// <summary>
    /// Throws if value is negative.
    /// </summary>
    public T Negative<T>(
        T value,
        [CallerArgumentExpression(nameof(value))] string? expression = null)
        where T : IComparable<T>
    {
        if (value.CompareTo(default!) < 0)
            throw new ArgumentOutOfRangeException(
                expression,
                value,
                $"Value '{expression}' must not be negative, but was {value}.");
        return value;
    }

    // ── Collection Guards ────────────────────────────────────

    /// <summary>
    /// Throws if the collection is null or empty.
    /// </summary>
    public IReadOnlyCollection<T> NullOrEmpty<T>(
        IReadOnlyCollection<T>? value,
        [CallerArgumentExpression(nameof(value))] string? expression = null)
    {
        if (value is null or { Count: 0 })
            throw new ArgumentException(
                $"Collection '{expression}' must not be null or empty.",
                expression);
        return value;
    }

    // ── Predicate Guard ──────────────────────────────────────

    /// <summary>
    /// Throws if the predicate is true (guard against a condition).
    /// </summary>
    public T Condition<T>(
        T value,
        Func<T, bool> predicate,
        string message,
        [CallerArgumentExpression(nameof(value))] string? expression = null)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        if (predicate(value))
            throw new ArgumentException(
                $"Guard failed for '{expression}': {message}",
                expression);
        return value;
    }
}

/// <summary>
/// Demonstrates the Guard pattern with CallerArgumentExpression.
/// </summary>
public static class GuardDemo
{
    /// <summary>
    /// Uses guard clauses for a chat message send operation.
    /// </summary>
    private static ChatMessage SendMessage(
        string? sender,
        string? content,
        string? channel,
        int priority = 0)
    {
        // Each guard captures the expression automatically.
        // Error messages will show "sender", "content", "channel"
        // — or whatever expression the caller passed.
        var validSender = Guard.Against.NullOrWhiteSpace(sender);
        var validContent = Guard.Against.NullOrEmpty(content);
        var validChannel = Guard.Against.NullOrWhiteSpace(channel);
        Guard.Against.OutOfRange(priority, min: 0, max: 10);

        return ChatMessage.Create(validSender, validContent, validChannel);
    }

    /// <summary>
    /// Demonstrates guard against a complex expression.
    /// </summary>
    private static User ValidateUser(User? user)
    {
        // When called as ValidateUser(someUser), the error
        // message will include "user" as the expression.
        return Guard.Against.Null(user);
    }

    // ── Before: hardcoded parameter names ────────────────────

    private static void ValidateOldWay(string? sender)
    {
        if (sender is null)
            throw new ArgumentNullException("sender"); // Fragile! Rename won't update this.

        // Better, but verbose:
        if (sender is null)
            throw new ArgumentNullException(nameof(sender)); // Works, but boilerplate.
    }

    public static void Run()
    {
        Console.WriteLine("╔═══ CallerArgumentExpression Guard ═══╗\n");

        // ── 1. Successful validation ─────────────────────────
        Console.WriteLine("── Successful Guards ──");
        var msg = SendMessage("Alice", "Hello!", "general", priority: 5);
        Console.WriteLine($"  Created: {msg}");

        // ── 2. Null guard ────────────────────────────────────
        Console.WriteLine("\n── Null Guard ──");
        try
        {
            SendMessage(null, "Hello!", "general");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"  Caught: {ex.Message}");
            // Output includes the expression "sender" automatically!
        }

        // ── 3. Empty string guard ────────────────────────────
        Console.WriteLine("\n── Empty String Guard ──");
        try
        {
            SendMessage("Alice", "", "general");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"  Caught: {ex.Message}");
        }

        // ── 4. Range guard ───────────────────────────────────
        Console.WriteLine("\n── Range Guard ──");
        try
        {
            SendMessage("Alice", "Hello!", "general", priority: 42);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Console.WriteLine($"  Caught: {ex.Message}");
        }

        // ── 5. Complex expression in guard ───────────────────
        Console.WriteLine("\n── Complex Expression ──");
        try
        {
            User? user = null;
            ValidateUser(user);
        }
        catch (ArgumentNullException ex)
        {
            // The error message includes "user" as the expression
            Console.WriteLine($"  Caught: {ex.Message}");
        }

        // ── 6. Negative guard ────────────────────────────────
        Console.WriteLine("\n── Negative Guard ──");
        try
        {
            int timeout = -1;
            Guard.Against.Negative(timeout);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Console.WriteLine($"  Caught: {ex.Message}");
        }

        // ── 7. Collection guard ──────────────────────────────
        Console.WriteLine("\n── Collection Guard ──");
        try
        {
            IReadOnlyCollection<string>? channels = null;
            Guard.Against.NullOrEmpty(channels);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"  Caught: {ex.Message}");
        }

        // ── 8. Custom predicate guard ────────────────────────
        Console.WriteLine("\n── Custom Predicate Guard ──");
        try
        {
            string channelName = "this-name-is-way-too-long-for-a-channel";
            Guard.Against.Condition(
                channelName,
                name => name.Length > 20,
                "Channel name must be 20 characters or fewer");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"  Caught: {ex.Message}");
        }

        // ── 9. Key insight ───────────────────────────────────
        Console.WriteLine("\n── Key Insight ──");
        Console.WriteLine("  [CallerArgumentExpression] captures the SOURCE TEXT");
        Console.WriteLine("  of the argument at compile time.  No magic strings,");
        Console.WriteLine("  no nameof() boilerplate — just rich error messages.");
    }
}
