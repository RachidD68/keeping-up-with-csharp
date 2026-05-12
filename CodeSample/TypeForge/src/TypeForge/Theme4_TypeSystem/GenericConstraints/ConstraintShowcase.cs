namespace TypeForge.Theme4_TypeSystem;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: Generic Constraints (evolved through C# 13)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  PROBLEM
//  -------
//  Unconstrained generics are too permissive — you can't
//  call methods on T because the compiler knows nothing
//  about it.  Over-constrained generics are too restrictive.
//
//  SOLUTION
//  --------
//  C# provides a rich constraint vocabulary:
//  • where T : class / struct / unmanaged / notnull
//  • where T : new() / Interface / BaseClass
//  • where T : INumber<T> (generic math, C# 11)
//  • where T : allows ref struct (C# 13)
//
//  WHY IT MATTERS
//  ──────────────
//  Precise constraints make generic code both flexible AND
//  safe.  The compiler can verify more, IntelliSense can
//  suggest more, and runtime errors become compile-time errors.
//
//  TRY IT
//  ──────
//  1. Create a generic Cloneable<T> where T : ICloneable.
//  2. Use `allows ref struct` with a span-based method.
//  3. Combine INumber<T> with IComparable<T> constraints.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Showcases the full constraint vocabulary available in modern C#.
/// </summary>
public static class ConstraintShowcaseDemo
{
    // ── class constraint ─────────────────────────────────────

    /// <summary>Only reference types — no int, double, structs.</summary>
    private static T? FindFirst<T>(IReadOnlyList<T> items, Func<T, bool> predicate)
        where T : class
    {
        foreach (var item in items)
            if (predicate(item)) return item;
        return null; // Only valid because T : class (nullable ref type)
    }

    // ── struct constraint ────────────────────────────────────

    /// <summary>Only value types — enables default(T) as sentinel.</summary>
    private static T ValueOrDefault<T>(T? value) where T : struct =>
        value ?? default;

    // ── notnull constraint ───────────────────────────────────

    /// <summary>Accepts both class and struct, but never null.</summary>
    private static Dictionary<TKey, List<TValue>> GroupBy<TKey, TValue>(
        IEnumerable<TValue> items,
        Func<TValue, TKey> keySelector)
        where TKey : notnull
    {
        var groups = new Dictionary<TKey, List<TValue>>();
        foreach (var item in items)
        {
            var key = keySelector(item);
            if (!groups.TryGetValue(key, out var list))
            {
                list = [];
                groups[key] = list;
            }
            list.Add(item);
        }
        return groups;
    }

    // ── new() constraint ─────────────────────────────────────

    /// <summary>Requires a parameterless constructor.</summary>
    private static List<T> CreateMany<T>(int count) where T : new()
    {
        var list = new List<T>(count);
        for (int i = 0; i < count; i++)
            list.Add(new T());
        return list;
    }

    // ── Multiple interface constraints ───────────────────────

    /// <summary>Constrains T to be both numeric and comparable.</summary>
    private static T Median<T>(T[] values)
        where T : INumber<T>, IComparable<T>
    {
        if (values.Length == 0)
            return T.Zero;

        var sorted = values.OrderBy(v => v).ToArray();
        return sorted[sorted.Length / 2];
    }

    // ── Base class constraint ────────────────────────────────

    /// <summary>Only TypeNode or its derived types.</summary>
    private static string FormatTypeNode<T>(T node) where T : TypeNode =>
        $"Node<{typeof(T).Name}>: {node.Format()}";

    // ── Combined constraints ─────────────────────────────────

    /// <summary>
    /// Requires: reference type, implements IPlugin, has factory.
    /// This is the maximum constraint power.
    /// </summary>
    private static TPlugin CreateAndRun<TPlugin>(string input)
        where TPlugin : class, IPlugin, IPluginFactory<TPlugin>
    {
        var plugin = TPlugin.Create();
        plugin.Initialize();
        var result = plugin.Execute(input);
        Console.WriteLine($"    {TPlugin.PluginId}: \"{result}\"");
        return plugin;
    }

    // ── Validator using generic constraints ───────────────────

    /// <summary>Creates a range validator using IComparable.</summary>
    private static Validator<T> CreateRangeValidator<T>(T min, T max, string fieldName)
        where T : IComparable<T>
    {
        return new Validator<T>()
            .AddRule(
                $"{fieldName} range check",
                value => value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0,
                $"{fieldName} must be between {min} and {max}");
    }

    // ── A type for demonstrating new() constraint ────────────

    private sealed class PluginSlot
    {
        public string Status { get; set; } = "Empty";
        public override string ToString() => $"Slot({Status})";
    }

    public static void Run()
    {
        Console.WriteLine("╔═══ Generic Constraints Showcase ═══╗\n");

        // ── 1. class constraint ──────────────────────────────
        Console.WriteLine("── where T : class ──");
        var names = new[] { "Alice", "Bob", "Charlie" };
        var found = FindFirst(names, n => n.StartsWith("B"));
        var notFound = FindFirst(names, n => n.StartsWith("Z"));
        Console.WriteLine($"  Found 'B*': {found ?? "(null)"}");
        Console.WriteLine($"  Found 'Z*': {notFound ?? "(null)"}");

        // ── 2. struct constraint ─────────────────────────────
        Console.WriteLine("\n── where T : struct ──");
        int? someInt = 42;
        int? noInt = null;
        Console.WriteLine($"  ValueOrDefault(42):   {ValueOrDefault(someInt)}");
        Console.WriteLine($"  ValueOrDefault(null): {ValueOrDefault(noInt)}");

        // ── 3. notnull constraint ────────────────────────────
        Console.WriteLine("\n── where TKey : notnull ──");
        var shapes = new IShape[]
        {
            new Circle(5), new Rectangle(3, 4), new Circle(10), new Rectangle(1, 1)
        };
        var grouped = GroupBy<string, IShape>(shapes, s => s.GetType().Name);
        foreach (var (key, items) in grouped)
            Console.WriteLine($"  {key}: {items.Count} shapes");

        // ── 4. new() constraint ──────────────────────────────
        Console.WriteLine("\n── where T : new() ──");
        var slots = CreateMany<PluginSlot>(3);
        Console.WriteLine($"  Created {slots.Count} slots: {string.Join(", ", slots)}");

        // ── 5. INumber<T> + IComparable<T> ───────────────────
        Console.WriteLine("\n── where T : INumber<T>, IComparable<T> ──");
        Console.WriteLine($"  Median ints:    {Median([5, 2, 8, 1, 9])}");
        Console.WriteLine($"  Median doubles: {Median([3.14, 2.71, 1.41, 1.73])}");

        // ── 6. Base class constraint ─────────────────────────
        Console.WriteLine("\n── where T : TypeNode ──");
        var intType = new PrimitiveType("int", 4);
        var arrayType = new ArrayType(intType);
        Console.WriteLine($"  {FormatTypeNode(intType)}");
        Console.WriteLine($"  {FormatTypeNode(arrayType)}");

        // ── 7. Validator with IComparable ────────────────────
        Console.WriteLine("\n── Validator<T> where T : IComparable ──");
        var ageValidator = CreateRangeValidator(0, 150, "Age");
        Console.WriteLine($"  Age 25:  {(ageValidator.Validate(25).IsValid ? "✓" : "✗")}");
        Console.WriteLine($"  Age 200: {(ageValidator.Validate(200).IsValid ? "✓" : "✗")}");

        // ── 8. Constraint vocabulary ─────────────────────────
        Console.WriteLine("\n── Full Constraint Vocabulary ──");
        Console.WriteLine("  where T : class            — reference type only");
        Console.WriteLine("  where T : struct           — value type only");
        Console.WriteLine("  where T : unmanaged        — unmanaged value type");
        Console.WriteLine("  where T : notnull          — non-nullable");
        Console.WriteLine("  where T : new()            — has parameterless ctor");
        Console.WriteLine("  where T : BaseClass        — inherits from base");
        Console.WriteLine("  where T : IInterface       — implements interface");
        Console.WriteLine("  where T : INumber<T>       — generic math (C# 11)");
        Console.WriteLine("  where T : allows ref struct — accept ref structs (C# 13)");
    }
}
