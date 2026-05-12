// Chapter 1 — The Foundation (C# 1–4) — BASIC
// ----------------------------------------------------------------
// Exercise: Take QuantLite's TradeRepository<T> and add a
//   Remove(Guid id) method that returns true if the entity was
//   removed, false otherwise.
//
// Hint: Dictionary<TKey, TValue>.Remove already returns a bool.
// ----------------------------------------------------------------

namespace KeepUpCs.Exercises.Ch01;

// Self-contained copy of the trade type used by the repository.
public record TradeRecord(Guid Id, string Ticker, decimal Price, int Quantity);

// The repository from the chapter, with the new Remove method appended.
public class TradeRepository<T> where T : TradeRecord
{
    private readonly Dictionary<Guid, T> _store = [];

    public int Count => _store.Count;

    public void Add(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _store[entity.Id] = entity;
    }

    public T? GetById(Guid id) => _store.GetValueOrDefault(id);

    public IReadOnlyList<T> GetAll() => _store.Values.ToList().AsReadOnly();

    // ── The answer ──────────────────────────────────────────────
    /// <summary>
    /// Removes the entity with the given id. Returns true if the entity
    /// was present and removed, false if no entity with that id existed.
    /// </summary>
    public bool Remove(Guid id) => _store.Remove(id);
    // ────────────────────────────────────────────────────────────
}

public static class BasicDemo
{
    public static void Run()
    {
        Console.WriteLine("Ch01 Basic — TradeRepository<T>.Remove");
        Console.WriteLine(new string('─', 60));

        var repo = new TradeRepository<TradeRecord>();
        var aapl = new TradeRecord(Guid.NewGuid(), "AAPL", 150m, 100);
        var msft = new TradeRecord(Guid.NewGuid(), "MSFT", 420m, 50);
        repo.Add(aapl);
        repo.Add(msft);

        Console.WriteLine($"  before: Count = {repo.Count}");

        var removedAapl = repo.Remove(aapl.Id);
        Console.WriteLine($"  Remove(AAPL.Id) → {removedAapl}");

        var removedTwice = repo.Remove(aapl.Id);
        Console.WriteLine($"  Remove(AAPL.Id) again → {removedTwice}  (false: already gone)");

        var removedUnknown = repo.Remove(Guid.NewGuid());
        Console.WriteLine($"  Remove(unknown id) → {removedUnknown}  (false)");

        Console.WriteLine($"  after:  Count = {repo.Count}");
    }
}
