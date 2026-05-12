// Chapter 6 — Safety & Robustness — BASIC
// ----------------------------------------------------------------
// Exercise: Take the ChatConfigOld class from the ??= section and
//   rewrite every lazy-initialization method using ??=. Compare
//   line counts and verify behaviour is identical.
//
// Hint: The field-is-null-so-assign pattern is exactly what ??=
//   replaces.
// ----------------------------------------------------------------

namespace KeepUpCs.Exercises.Ch06;

public record ConnectionInfo(string Host, int Port)
{
    public static ConnectionInfo Default { get; } = new("localhost", 8080);
}

// ── BEFORE (verbose null-check-and-assign) ──────────────────────
public sealed class ChatConfigOld
{
    private string? _displayName;
    private List<string>? _favoriteChannels;
    private ConnectionInfo? _connection;

    public string GetDisplayName(string? userName)
    {
        if (_displayName == null)
            _displayName = userName ?? "Anonymous";
        return _displayName;
    }

    public List<string> GetFavoriteChannels()
    {
        if (_favoriteChannels == null)
            _favoriteChannels = [];
        return _favoriteChannels;
    }

    public ConnectionInfo GetConnection()
    {
        if (_connection == null)
            _connection = ConnectionInfo.Default;
        return _connection;
    }
}

// ── AFTER (??= one-liners) ──────────────────────────────────────
public sealed class ChatConfigNew
{
    private string? _displayName;
    private List<string>? _favoriteChannels;
    private ConnectionInfo? _connection;

    public string GetDisplayName(string? userName) =>
        _displayName ??= userName ?? "Anonymous";

    public List<string> GetFavoriteChannels() =>
        _favoriteChannels ??= [];

    public ConnectionInfo GetConnection() =>
        _connection ??= ConnectionInfo.Default;
}

public static class BasicDemo
{
    public static void Run()
    {
        Console.WriteLine("Ch06 Basic — ChatConfigOld → ??= rewrite");
        Console.WriteLine(new string('─', 60));

        var oldCfg = new ChatConfigOld();
        var newCfg = new ChatConfigNew();

        Console.WriteLine($"  old.GetDisplayName(\"Alice\"): {oldCfg.GetDisplayName("Alice")}");
        Console.WriteLine($"  new.GetDisplayName(\"Alice\"): {newCfg.GetDisplayName("Alice")}");

        oldCfg.GetFavoriteChannels().Add("general");
        newCfg.GetFavoriteChannels().Add("general");
        Console.WriteLine($"  old.GetFavoriteChannels(): [{string.Join(", ", oldCfg.GetFavoriteChannels())}]");
        Console.WriteLine($"  new.GetFavoriteChannels(): [{string.Join(", ", newCfg.GetFavoriteChannels())}]");

        Console.WriteLine($"  old.GetConnection(): {oldCfg.GetConnection()}");
        Console.WriteLine($"  new.GetConnection(): {newCfg.GetConnection()}");

        Console.WriteLine();
        Console.WriteLine("  Old: 3 methods × 4 lines each ≈ 12 lines of body.");
        Console.WriteLine("  New: 3 one-line expression-bodied members ≈ 3 lines.");
    }
}
