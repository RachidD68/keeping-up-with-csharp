// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: Generics                                      ║
// ║  Introduced: C# 2.0                                     ║
// ║  Theme: 0 — The Foundation                              ║
// ╚══════════════════════════════════════════════════════════╝

namespace QuantLite.Theme0_Foundation.Generics;

/// <summary>
/// Demonstrates generics through a type-safe trade repository.
/// Generics eliminated the need for <c>ArrayList</c> casts and
/// introduced compile-time type safety to collections.
/// </summary>
public static class GenericRepositoryDemo
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — How we did it before (C# 1.0)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Before generics, we used <c>ArrayList</c> which stored everything
    /// as <c>object</c>. This required casting on retrieval and offered
    /// no compile-time type safety — you could add a string to a list
    /// of trades and only discover the error at runtime.
    /// </summary>
    public static void BeforeGenerics()
    {
        Console.WriteLine("  BEFORE (C# 1.0 — ArrayList, no type safety):");
        Console.WriteLine();

        // Using object-based collection (simulating ArrayList behavior)
        var untypedTrades = new System.Collections.ArrayList();

        var trade1 = TradeRecord.Create("AAPL", new Money(150.00m, Currency.USD), 100, TradeType.Spot);
        untypedTrades.Add(trade1);

        // BUG: This compiles fine but is logically wrong!
        untypedTrades.Add("This is not a trade!");

        Console.WriteLine($"    ArrayList has {untypedTrades.Count} items (one is a string!)");

        // Requires explicit cast — runtime error if wrong type
        var retrieved = (TradeRecord)untypedTrades[0]!;
        Console.WriteLine($"    Retrieved with cast: {retrieved.Ticker}");

        // This would throw InvalidCastException at runtime:
        // var bad = (TradeRecord)untypedTrades[1]; // 💥
        Console.WriteLine("    ⚠ untypedTrades[1] is a string — cast would throw at runtime!");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — The modern way (C# 2.0+)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// With generics, <c>List&lt;T&gt;</c> provides compile-time type safety.
    /// The repository below is generic over any entity type, with
    /// constraints ensuring it has the members we need.
    /// </summary>
    public static void WithGenerics()
    {
        Console.WriteLine("  AFTER (C# 2.0 — Generics with type safety):");
        Console.WriteLine();

        var repository = new TradeRepository<TradeRecord>();

        var trade1 = TradeRecord.Create("AAPL", new Money(150.00m, Currency.USD), 100, TradeType.Spot);
        var trade2 = TradeRecord.Create("MSFT", new Money(420.00m, Currency.USD), 50, TradeType.Forward);

        repository.Add(trade1);
        repository.Add(trade2);

        // repository.Add("not a trade"); // ✅ Compile error! Type safety enforced.

        Console.WriteLine($"    Repository has {repository.Count} trades (only TradeRecord allowed)");
        Console.WriteLine($"    First trade: {repository.GetById(trade1.Id)?.Ticker ?? "not found"}");

        // Generic method with type inference
        var allTrades = repository.GetAll();
        Console.WriteLine($"    All trades retrieved: {allTrades.Count} items");

        Console.WriteLine();
        Console.WriteLine("    ✓ Compile-time type safety — no invalid types can be added");
        Console.WriteLine("    ✓ No casting required on retrieval");
        Console.WriteLine("    ✓ Better performance — no boxing for value types");
    }

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // Generics are the single most impactful feature in C# history.
    // They enabled: type-safe collections, LINQ (built on IEnumerable<T>),
    // async/await (Task<T>), and every modern pattern in C#.
    // Without generics, none of the features in Themes 1–10 would exist
    // in their current form.

    // GOING DEEPER:
    // Unlike Java's type erasure, C# generics are "reified" — the type
    // parameter is preserved at runtime. This means:
    // 1. typeof(List<int>) != typeof(List<string>) — true at runtime
    // 2. Value types get specialized JIT code — List<int> uses actual
    //    ints, not boxed objects, giving significant performance gains.
    // 3. You can use reflection on generic types at runtime.

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: Create a generic PortfolioRepository<T> where T : Portfolio
    // that stores portfolios and supports lookup by name.
    // Hint: Add a constraint like "where T : Portfolio" and a
    // GetByName(string name) method.

    /// <summary>Runs the complete generics demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: Generics (C# 2.0)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforeGenerics();
        WithGenerics();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}

/// <summary>
/// A generic, type-safe repository for any entity identified by <see cref="Guid"/>.
/// Demonstrates generic classes with constraints.
/// </summary>
/// <typeparam name="T">The entity type stored in this repository.</typeparam>
public class TradeRepository<T> where T : TradeRecord
{
    private readonly Dictionary<Guid, T> _store = [];

    /// <summary>The number of entities in the repository.</summary>
    public int Count => _store.Count;

    /// <summary>Adds an entity to the repository.</summary>
    public void Add(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _store[entity.Id] = entity;
    }

    /// <summary>Retrieves an entity by its unique ID, or null if not found.</summary>
    public T? GetById(Guid id) =>
        _store.GetValueOrDefault(id);

    /// <summary>Returns all entities as a read-only list.</summary>
    public IReadOnlyList<T> GetAll() =>
        _store.Values.ToList().AsReadOnly();

    /// <summary>Finds entities matching the predicate.</summary>
    public IEnumerable<T> Find(Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _store.Values.Where(predicate);
    }
}
