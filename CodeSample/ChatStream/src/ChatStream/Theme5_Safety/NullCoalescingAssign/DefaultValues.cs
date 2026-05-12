namespace ChatStream.Theme5_Safety;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: Null-Coalescing Assignment (??=)  (C# 8)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  PROBLEM
//  -------
//  The pattern `if (x == null) x = defaultValue;` appeared
//  everywhere: lazy initialization, default parameter
//  backfilling, cache-miss handling.  It was verbose and
//  easy to get subtly wrong (e.g., using `=` instead of `==`).
//
//  SOLUTION
//  --------
//  The ??= operator assigns the right-hand side only when
//  the left-hand side is null.  It's the assignment form of
//  the null-coalescing operator (??).
//
//  WHY IT MATTERS
//  ──────────────
//  Lazy initialization becomes a one-liner.  Combined with
//  nullable reference types, ??= eliminates a whole class
//  of "forgot to initialize" bugs.
//
//  TRY IT
//  ──────
//  1. Use ??= to lazily create a List<T>.
//  2. Chain ??= with ?. for deeply nested defaults.
//  3. Implement a thread-safe lazy cache with ??= and lock.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Demonstrates null-coalescing assignment in chat configuration.
/// </summary>
public static class DefaultValuesDemo
{
    // ── Before: verbose null-check-and-assign ────────────────

    /// <summary>Old-style configuration defaulting.</summary>
    private sealed class ChatConfigOld
    {
        private string? _displayName;
        private List<string>? _favoriteChannels;

        public string GetDisplayName(string? userName)
        {
            // Verbose: check, then assign
            if (_displayName == null)
                _displayName = userName ?? "Anonymous";
            return _displayName;
        }

        public List<string> GetFavoriteChannels()
        {
            // Verbose: check, then create
            if (_favoriteChannels == null)
                _favoriteChannels = [];
            return _favoriteChannels;
        }
    }

    // ── After: concise ??= assignment ────────────────────────

    /// <summary>Modern configuration with ??= for lazy defaults.</summary>
    private sealed class ChatConfigNew
    {
        private string? _displayName;
        private List<string>? _favoriteChannels;
        private ConnectionInfo? _connection;

        /// <summary>
        /// Lazily initializes display name — ??= assigns only if null.
        /// </summary>
        public string GetDisplayName(string? userName)
        {
            // One line replaces the if-null-then-assign pattern.
            // If _displayName already has a value, userName is never evaluated.
            _displayName ??= userName ?? "Anonymous";
            return _displayName;
        }

        /// <summary>
        /// Lazily creates the collection on first access.
        /// </summary>
        public List<string> GetFavoriteChannels()
        {
            // ??= is perfect for lazy collection initialization.
            _favoriteChannels ??= [];
            return _favoriteChannels;
        }

        /// <summary>
        /// Provides a default connection if none was configured.
        /// </summary>
        public ConnectionInfo GetConnection()
        {
            _connection ??= ConnectionInfo.Default;
            return _connection;
        }
    }

    /// <summary>
    /// A cache that uses ??= for lazy population.
    /// </summary>
    private sealed class UserCache
    {
        private readonly Dictionary<string, User> _cache = new();

        /// <summary>
        /// Gets or creates a user entry — ??= works with indexers too.
        /// </summary>
        public User GetOrCreate(string name)
        {
            // If the key doesn't exist, the indexer throws.
            // We use TryGetValue + ??= instead.
            if (!_cache.TryGetValue(name, out var user) || user is null)
            {
                user = User.Create(name);
                _cache[name] = user;
            }
            return user;
        }

        /// <summary>
        /// Demonstrates ??= in a LINQ-like pipeline with defaults.
        /// </summary>
        public User? FindOrDefault(string name) =>
            _cache.GetValueOrDefault(name);

        public int Count => _cache.Count;
    }

    public static void Run()
    {
        Console.WriteLine("╔═══ Null-Coalescing Assignment (??=) ═══╗\n");

        // ── 1. Before vs After comparison ────────────────────
        Console.WriteLine("── Before vs After ──");

        var oldConfig = new ChatConfigOld();
        var newConfig = new ChatConfigNew();

        Console.WriteLine($"  Old display name: {oldConfig.GetDisplayName("Alice")}");
        Console.WriteLine($"  New display name: {newConfig.GetDisplayName("Alice")}");

        // Second call — both should return the cached value
        Console.WriteLine($"  Old (cached):     {oldConfig.GetDisplayName("Bob")}");   // Still "Alice"
        Console.WriteLine($"  New (cached):     {newConfig.GetDisplayName("Bob")}");   // Still "Alice"

        // ── 2. Lazy collection initialization ────────────────
        Console.WriteLine("\n── Lazy Collection Init ──");
        var config = new ChatConfigNew();
        var channels = config.GetFavoriteChannels();
        channels.Add("general");
        channels.Add("random");

        // Same list instance returned on subsequent calls
        var same = config.GetFavoriteChannels();
        Console.WriteLine($"  Same instance: {ReferenceEquals(channels, same)}");
        Console.WriteLine($"  Channels: {string.Join(", ", same)}");

        // ── 3. Default connection ────────────────────────────
        Console.WriteLine("\n── Default Connection ──");
        var conn = config.GetConnection();
        Console.WriteLine($"  Endpoint: {conn.Endpoint}");
        Console.WriteLine($"  Timeout:  {conn.Timeout}");

        // ── 4. Variable-level ??= ────────────────────────────
        Console.WriteLine("\n── Variable-Level ??= ──");
        string? greeting = null;
        Console.WriteLine($"  Before:  {greeting ?? "(null)"}");

        greeting ??= "Hello, ChatStream!";
        Console.WriteLine($"  After:   {greeting}");

        greeting ??= "This won't replace";  // Already non-null
        Console.WriteLine($"  Stable:  {greeting}");

        // ── 5. Chained ??= with multiple fallbacks ───────────
        Console.WriteLine("\n── Chained Fallbacks ──");
        string? primary = null;
        string? secondary = null;
        string? tertiary = "fallback-server";

        // Each ??= only assigns if still null
        primary ??= secondary;   // Still null (secondary is null)
        primary ??= tertiary;    // Now "fallback-server"
        primary ??= "last-resort"; // Skipped (already assigned)

        Console.WriteLine($"  Resolved endpoint: {primary}");

        // ── 6. Cache pattern ─────────────────────────────────
        Console.WriteLine("\n── Cache Pattern ──");
        var cache = new UserCache();
        var alice = cache.GetOrCreate("Alice");
        var bob = cache.GetOrCreate("Bob");
        var alice2 = cache.GetOrCreate("Alice"); // Returns cached

        Console.WriteLine($"  Cache size: {cache.Count}");
        Console.WriteLine($"  Same Alice: {ReferenceEquals(alice, alice2)}");
        Console.WriteLine($"  Alice: {alice.Name} ({alice.Status})");
        Console.WriteLine($"  Bob:   {bob.Name} ({bob.Status})");

        // ── 7. Nullable value types ──────────────────────────
        Console.WriteLine("\n── Nullable Value Types ──");
        int? messageCount = null;
        messageCount ??= 0;  // Works with nullable value types too
        Console.WriteLine($"  Message count: {messageCount}");

        DateTimeOffset? lastSeen = null;
        lastSeen ??= DateTimeOffset.UtcNow;
        Console.WriteLine($"  Last seen: {lastSeen:HH:mm:ss}");
    }
}
