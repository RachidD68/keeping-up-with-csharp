namespace TypeForge.Theme4_TypeSystem;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: File-Scoped Types  (C# 11)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  PROBLEM
//  -------
//  Helper types (comparers, converters, small DTOs) often
//  pollute the namespace.  Even internal types are visible
//  across the entire assembly, leading to naming conflicts
//  and cluttered IntelliSense.
//
//  SOLUTION
//  --------
//  The `file` modifier restricts a type's visibility to the
//  file where it's declared.  It cannot be seen from any
//  other file, even within the same namespace.
//
//  WHY IT MATTERS
//  ──────────────
//  Source generators create file-scoped types to avoid naming
//  collisions.  Application developers use them for one-off
//  helpers that don't deserve a public API.
//
//  TRY IT
//  ──────
//  1. Create a file-scoped comparer for TypeNode.
//  2. Try referencing a `file` type from another file — it won't compile.
//  3. Use `file` for a helper record in a LINQ pipeline.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

// ── file-scoped types — only visible in THIS file ────────

/// <summary>
/// A file-scoped helper for formatting type nodes.
/// Cannot be accessed from any other file.
/// </summary>
file sealed class TypeFormatter
{
    private readonly string _indent;

    public TypeFormatter(string indent = "  ") =>
        _indent = indent;

    public string Format(TypeNode node, int depth = 0) => node switch
    {
        PrimitiveType p => $"{Indent(depth)}Primitive: {p.Name} ({p.SizeBytes} bytes)",
        ArrayType a => $"{Indent(depth)}Array of:\n{Format(a.ElementType, depth + 1)}",
        NullableType n => $"{Indent(depth)}Nullable:\n{Format(n.InnerType, depth + 1)}",
        GenericType g =>
            $"{Indent(depth)}Generic {g.Name}:\n" +
            string.Join("\n", g.TypeArguments.Select(a => Format(a, depth + 1))),
        TupleType t =>
            $"{Indent(depth)}Tuple:\n" +
            string.Join("\n", t.Elements.Select(e =>
                $"{Format(e.Type, depth + 1)} (as {e.Name})")),
        FunctionType f =>
            $"{Indent(depth)}Function:\n" +
            $"{Indent(depth + 1)}Params:\n" +
            string.Join("\n", f.Parameters.Select(p => Format(p, depth + 2))) +
            $"\n{Indent(depth + 1)}Returns:\n{Format(f.ReturnType, depth + 2)}",
        _ => $"{Indent(depth)}Unknown: {node.Name}"
    };

    private string Indent(int depth) =>
        string.Concat(Enumerable.Repeat(_indent, depth));
}

/// <summary>
/// A file-scoped comparison helper.
/// </summary>
file static class TypeComparisons
{
    /// <summary>Compares type nodes by their formatted name length.</summary>
    public static int CompareByComplexity(TypeNode a, TypeNode b) =>
        a.Format().Length.CompareTo(b.Format().Length);
}

/// <summary>
/// A file-scoped record — perfect for intermediate pipeline data.
/// </summary>
file record TypeAnalysis(
    TypeNode Node,
    int Depth,
    int NodeCount,
    string FormattedName);

// ── Public demo class using the file-scoped helpers ──────

/// <summary>
/// Demonstrates file-scoped types for encapsulated helpers.
/// </summary>
public static class InternalHelpersDemo
{
    public static void Run()
    {
        Console.WriteLine("╔═══ File-Scoped Types ═══╗\n");

        var types = new TypeNode[]
        {
            new PrimitiveType("int", 4),
            new ArrayType(new PrimitiveType("string", -1)),
            new NullableType(new PrimitiveType("double", 8)),
            new GenericType("List", [new PrimitiveType("int", 4)]),
        };

        // ── 1. Using file-scoped TypeFormatter ───────────────
        Console.WriteLine("── File-Scoped TypeFormatter ──");
        var formatter = new TypeFormatter("    ");
        foreach (var type in types)
        {
            Console.WriteLine(formatter.Format(type));
            Console.WriteLine();
        }

        // ── 2. Using file-scoped comparison ──────────────────
        Console.WriteLine("── File-Scoped Comparison ──");
        var sorted = types.OrderBy(t => t, Comparer<TypeNode>.Create(
            TypeComparisons.CompareByComplexity));
        Console.WriteLine("  Sorted by complexity (simplest first):");
        foreach (var type in sorted)
            Console.WriteLine($"    {type.Format()}");

        // ── 3. Using file-scoped record in pipeline ──────────
        Console.WriteLine("\n── File-Scoped Pipeline Record ──");
        var analyses = types.Select(t => new TypeAnalysis(
            Node: t,
            Depth: 0, // Simplified for demo
            NodeCount: 1,
            FormattedName: t.Format()));

        foreach (var analysis in analyses)
            Console.WriteLine($"    {analysis.FormattedName} ({analysis.NodeCount} nodes)");

        // ── 4. Visibility demonstration ──────────────────────
        Console.WriteLine("\n── Visibility Rules ──");
        Console.WriteLine("  `file class TypeFormatter` — only visible in THIS file");
        Console.WriteLine("  `file static class TypeComparisons` — same file only");
        Console.WriteLine("  `file record TypeAnalysis` — same file only");
        Console.WriteLine();
        Console.WriteLine("  From another file:");
        Console.WriteLine("    var f = new TypeFormatter();  // ← Compile error!");
        Console.WriteLine("    TypeComparisons.Compare(...); // ← Compile error!");
        Console.WriteLine();
        Console.WriteLine("  Key uses:");
        Console.WriteLine("    • Source generator output (avoid name collisions)");
        Console.WriteLine("    • One-off helpers (keep namespace clean)");
        Console.WriteLine("    • Pipeline intermediates (no public API pollution)");
    }
}
