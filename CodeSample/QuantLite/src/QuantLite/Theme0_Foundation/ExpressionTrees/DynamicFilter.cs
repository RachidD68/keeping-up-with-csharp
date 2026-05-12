// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: Expression Trees                              ║
// ║  Introduced: C# 3.0                                     ║
// ║  Theme: 0 — The Foundation                              ║
// ╚══════════════════════════════════════════════════════════╝

namespace QuantLite.Theme0_Foundation.ExpressionTrees;

/// <summary>
/// Demonstrates expression trees by building dynamic trade filters
/// at runtime — the same mechanism that powers LINQ-to-SQL/EF Core.
/// </summary>
public static class DynamicFilterDemo
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — How we did it before
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Without expression trees, building dynamic queries requires
    /// string concatenation (SQL injection risk) or complex
    /// if/else chains that are hard to maintain.
    /// </summary>
    public static void BeforeExpressionTrees()
    {
        Console.WriteLine("  BEFORE — Dynamic filters via if/else chains:");
        Console.WriteLine();

        var trades = CreateSampleTrades();

        // Simulating dynamic filter criteria
        string? filterTicker = "AAPL";
        decimal? minNotional = 10_000m;
        TradeType? filterType = null; // not filtering by type

        // Chained if/else — grows linearly with filter count
        IEnumerable<TradeRecord> result = trades;
        if (filterTicker is not null)
            result = result.Where(t => t.Ticker == filterTicker);
        if (minNotional.HasValue)
            result = result.Where(t => t.NotionalValue.Amount >= minNotional.Value);
        if (filterType.HasValue)
            result = result.Where(t => t.Type == filterType.Value);

        Console.WriteLine($"    Manual chaining found {result.Count()} trades");
        Console.WriteLine("    ⚠ Works for in-memory, but can't be translated to SQL");
        Console.WriteLine("    ⚠ Each new filter criterion adds another if block");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — The modern way (C# 3.0+)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Expression trees represent code as data. You can inspect,
    /// modify, and compile them at runtime — or translate them
    /// to another language like SQL.
    /// </summary>
    public static void WithExpressionTrees()
    {
        Console.WriteLine("  AFTER (C# 3.0 — Expression trees):");
        Console.WriteLine();

        var trades = CreateSampleTrades();

        // Build a filter expression dynamically
        var filter = BuildFilter(ticker: "AAPL", minNotional: 5_000m);

        // Inspect the expression tree (it's data, not compiled code)
        Console.WriteLine($"    Expression tree: {filter}");
        Console.WriteLine();

        // Compile to executable delegate
        var compiledFilter = filter.Compile();
        var results = trades.Where(compiledFilter).ToList();

        Console.WriteLine($"    Compiled and executed: {results.Count} matches");
        foreach (var trade in results)
            Console.WriteLine($"      {trade.Ticker}: {trade.NotionalValue}");

        Console.WriteLine();

        // Combining expressions
        var tickerFilter = BuildTickerFilter("MSFT");
        var typeFilter = BuildTypeFilter(TradeType.Forward);
        var combined = CombineWithAnd(tickerFilter, typeFilter);

        Console.WriteLine($"    Combined expression: {combined}");
        var combinedResults = trades.Where(combined.Compile()).ToList();
        Console.WriteLine($"    Combined filter matches: {combinedResults.Count}");

        Console.WriteLine();
        Console.WriteLine("    ✓ Code as data — expressions can be inspected and transformed");
        Console.WriteLine("    ✓ Dynamic composition — build queries at runtime");
        Console.WriteLine("    ✓ Translatable — EF Core translates these to SQL");
    }

    /// <summary>
    /// Builds a dynamic filter expression from optional criteria.
    /// </summary>
    private static Expression<Func<TradeRecord, bool>> BuildFilter(
        string? ticker = null, decimal? minNotional = null)
    {
        // Start with the parameter: t =>
        var param = Expression.Parameter(typeof(TradeRecord), "t");
        Expression body = Expression.Constant(true); // start with 'true'

        if (ticker is not null)
        {
            // t.Ticker == ticker
            var tickerProperty = Expression.Property(param, nameof(TradeRecord.Ticker));
            var tickerValue = Expression.Constant(ticker);
            var tickerCheck = Expression.Equal(tickerProperty, tickerValue);
            body = Expression.AndAlso(body, tickerCheck);
        }

        if (minNotional.HasValue)
        {
            // t.NotionalValue.Amount >= minNotional
            var notionalProp = Expression.Property(param, nameof(TradeRecord.NotionalValue));
            var amountProp = Expression.Property(notionalProp, nameof(Money.Amount));
            var threshold = Expression.Constant(minNotional.Value);
            var amountCheck = Expression.GreaterThanOrEqual(amountProp, threshold);
            body = Expression.AndAlso(body, amountCheck);
        }

        return Expression.Lambda<Func<TradeRecord, bool>>(body, param);
    }

    private static Expression<Func<TradeRecord, bool>> BuildTickerFilter(string ticker)
    {
        var param = Expression.Parameter(typeof(TradeRecord), "t");
        var prop = Expression.Property(param, nameof(TradeRecord.Ticker));
        var value = Expression.Constant(ticker);
        var body = Expression.Equal(prop, value);
        return Expression.Lambda<Func<TradeRecord, bool>>(body, param);
    }

    private static Expression<Func<TradeRecord, bool>> BuildTypeFilter(TradeType type)
    {
        var param = Expression.Parameter(typeof(TradeRecord), "t");
        var prop = Expression.Property(param, nameof(TradeRecord.Type));
        var value = Expression.Constant(type);
        var body = Expression.Equal(prop, value);
        return Expression.Lambda<Func<TradeRecord, bool>>(body, param);
    }

    /// <summary>
    /// Combines two expression trees with AND logic.
    /// </summary>
    private static Expression<Func<TradeRecord, bool>> CombineWithAnd(
        Expression<Func<TradeRecord, bool>> left,
        Expression<Func<TradeRecord, bool>> right)
    {
        var param = Expression.Parameter(typeof(TradeRecord), "t");

        // Replace parameters in both expressions with a shared one
        var leftBody = new ParameterReplacer(left.Parameters[0], param).Visit(left.Body);
        var rightBody = new ParameterReplacer(right.Parameters[0], param).Visit(right.Body);

        var combined = Expression.AndAlso(leftBody, rightBody);
        return Expression.Lambda<Func<TradeRecord, bool>>(combined, param);
    }

    /// <summary>Replaces one parameter expression with another in an expression tree.</summary>
    private sealed class ParameterReplacer(ParameterExpression oldParam, ParameterExpression newParam)
        : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) =>
            node == oldParam ? newParam : base.VisitParameter(node);
    }

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // Expression trees are the secret weapon behind EF Core's ability
    // to translate C# lambda expressions into SQL. When you write
    // dbContext.Users.Where(u => u.Age > 18), EF Core receives an
    // expression tree (not a compiled delegate), inspects it, and
    // generates "WHERE Age > 18" SQL. Without expression trees,
    // LINQ-to-SQL simply wouldn't exist.

    // GOING DEEPER:
    // There's a subtle but critical distinction:
    //   Func<TradeRecord, bool> — compiled code, can only be executed
    //   Expression<Func<TradeRecord, bool>> — data structure, can be
    //     inspected, modified, translated, and then compiled.
    // The compiler automatically converts a lambda to an expression
    // tree when the target type is Expression<TDelegate>.
    // See also: Theme 9 — Source Generators (another compile-time
    // code-as-data mechanism).

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: Build a BuildOrFilter method that combines two expression
    // trees with OR logic instead of AND. Use it to find trades that
    // are either Spot OR have notional > 20,000.

    private static List<TradeRecord> CreateSampleTrades() =>
    [
        TradeRecord.Create("AAPL", new Money(150.00m, Currency.USD), 100, TradeType.Spot),
        TradeRecord.Create("MSFT", new Money(420.00m, Currency.USD), 50, TradeType.Forward),
        TradeRecord.Create("GOOGL", new Money(175.00m, Currency.USD), 200, TradeType.Spot),
        TradeRecord.Create("TSLA", new Money(250.00m, Currency.USD), 30, TradeType.Option),
        TradeRecord.Create("AMZN", new Money(185.00m, Currency.USD), 75, TradeType.Swap),
    ];

    /// <summary>Runs the complete expression trees demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: Expression Trees (C# 3.0)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforeExpressionTrees();
        WithExpressionTrees();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
