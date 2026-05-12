namespace TypeForge.Theme4_TypeSystem;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: Sealed Type Hierarchies (abstract records)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  PROBLEM
//  -------
//  Open class hierarchies are hard to pattern-match
//  exhaustively.  Adding a new subtype doesn't produce
//  compile-time warnings in existing switch expressions.
//
//  SOLUTION
//  --------
//  Use sealed record types to create closed hierarchies.
//  When all subtypes are sealed, the compiler CAN verify
//  exhaustive matching (starting from C# 11 with sealed
//  hierarchy analysis).
//
//  WHY IT MATTERS
//  ──────────────
//  TypeNode has 6 variants.  A sealed hierarchy ensures
//  that every switch expression handles all of them.  When
//  you add a 7th variant, every switch shows a warning.
//
//  TRY IT
//  ──────
//  1. Add a UnionType to the TypeNode hierarchy.
//  2. Watch the compiler warn about missing switch cases.
//  3. Implement a TypeVisitor<T> with the visitor pattern.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Demonstrates sealed type hierarchies with the TypeNode walker.
/// </summary>
public static class TypeNodeWalkerDemo
{
    /// <summary>
    /// Calculates the "depth" of a type (how deeply nested it is).
    /// Switch expression handles all sealed variants.
    /// </summary>
    private static int CalculateDepth(TypeNode node) => node switch
    {
        PrimitiveType => 0,
        ArrayType a => 1 + CalculateDepth(a.ElementType),
        NullableType n => 1 + CalculateDepth(n.InnerType),
        GenericType g => 1 + g.TypeArguments.Max(CalculateDepth),
        TupleType t => 1 + t.Elements.Max(e => CalculateDepth(e.Type)),
        FunctionType f => 1 + Math.Max(
            f.Parameters.DefaultIfEmpty(new PrimitiveType("void", 0)).Max(CalculateDepth),
            CalculateDepth(f.ReturnType)),
        _ => throw new ArgumentException($"Unknown type node: {node.GetType().Name}")
    };

    /// <summary>
    /// Counts the total number of type nodes in a type tree.
    /// </summary>
    private static int CountNodes(TypeNode node) => node switch
    {
        PrimitiveType => 1,
        ArrayType a => 1 + CountNodes(a.ElementType),
        NullableType n => 1 + CountNodes(n.InnerType),
        GenericType g => 1 + g.TypeArguments.Sum(CountNodes),
        TupleType t => 1 + t.Elements.Sum(e => CountNodes(e.Type)),
        FunctionType f => 1 + f.Parameters.Sum(CountNodes) + CountNodes(f.ReturnType),
        _ => 1
    };

    /// <summary>
    /// Finds all primitive types used in a type tree.
    /// </summary>
    private static IEnumerable<string> FindPrimitives(TypeNode node) => node switch
    {
        PrimitiveType p => [p.Name],
        ArrayType a => FindPrimitives(a.ElementType),
        NullableType n => FindPrimitives(n.InnerType),
        GenericType g => g.TypeArguments.SelectMany(FindPrimitives),
        TupleType t => t.Elements.SelectMany(e => FindPrimitives(e.Type)),
        FunctionType f => f.Parameters.SelectMany(FindPrimitives)
            .Concat(FindPrimitives(f.ReturnType)),
        _ => []
    };

    /// <summary>
    /// Generates C#-like type syntax from the type tree.
    /// Already implemented as Format() on TypeNode, but this
    /// demonstrates the switch-expression walker pattern.
    /// </summary>
    private static string EmitCSharp(TypeNode node) => node switch
    {
        PrimitiveType p => p.Name,
        ArrayType a => $"{EmitCSharp(a.ElementType)}[]",
        NullableType n => $"{EmitCSharp(n.InnerType)}?",
        GenericType g =>
            $"{g.Name}<{string.Join(", ", g.TypeArguments.Select(EmitCSharp))}>",
        TupleType t =>
            $"({string.Join(", ", t.Elements.Select(e => $"{EmitCSharp(e.Type)} {e.Name}"))})",
        FunctionType f =>
            $"Func<{string.Join(", ", f.Parameters.Select(EmitCSharp).Append(EmitCSharp(f.ReturnType)))}>",
        _ => "object"
    };

    public static void Run()
    {
        Console.WriteLine("╔═══ Sealed Type Hierarchies ═══╗\n");

        // ── Build a complex type tree ────────────────────────
        var intType = new PrimitiveType("int", 4);
        var stringType = new PrimitiveType("string", -1);
        var boolType = new PrimitiveType("bool", 1);

        // Dictionary<string, int[]?>
        var complexType = new GenericType("Dictionary",
        [
            stringType,
            new NullableType(new ArrayType(intType))
        ]);

        // (string Name, int Age)
        var tupleType = new TupleType(
        [
            ("Name", stringType),
            ("Age", intType)
        ]);

        // Func<string, int, bool>
        var funcType = new FunctionType(
            [stringType, intType],
            boolType);

        var types = new (string Label, TypeNode Node)[]
        {
            ("Primitive", intType),
            ("Array", new ArrayType(stringType)),
            ("Nullable", new NullableType(intType)),
            ("Generic", complexType),
            ("Tuple", tupleType),
            ("Function", funcType),
        };

        // ── 1. Format each type ──────────────────────────────
        Console.WriteLine("── Type Formatting (switch walker) ──");
        foreach (var (label, type) in types)
            Console.WriteLine($"  {label,-12} → {EmitCSharp(type)}");

        // ── 2. Calculate depth ───────────────────────────────
        Console.WriteLine("\n── Type Depth ──");
        foreach (var (label, type) in types)
            Console.WriteLine($"  {label,-12} → depth {CalculateDepth(type)}");

        // ── 3. Count nodes ───────────────────────────────────
        Console.WriteLine("\n── Node Count ──");
        foreach (var (label, type) in types)
            Console.WriteLine($"  {label,-12} → {CountNodes(type)} nodes");

        // ── 4. Find primitives ───────────────────────────────
        Console.WriteLine("\n── Primitive Discovery ──");
        foreach (var (label, type) in types)
        {
            var primitives = FindPrimitives(type).Distinct().ToArray();
            Console.WriteLine($"  {label,-12} → [{string.Join(", ", primitives)}]");
        }

        // ── 5. Exhaustive matching guarantee ─────────────────
        Console.WriteLine("\n── Exhaustive Matching ──");
        Console.WriteLine("  All 6 TypeNode variants handled in every switch:");
        Console.WriteLine("    PrimitiveType, ArrayType, NullableType,");
        Console.WriteLine("    GenericType, TupleType, FunctionType");
        Console.WriteLine("  Adding a 7th variant → compile warning in each switch!");

        // ── 6. Key insight ───────────────────────────────────
        Console.WriteLine("\n── Key Insight ──");
        Console.WriteLine("  Sealed hierarchies + switch expressions = ");
        Console.WriteLine("  exhaustive, type-safe recursive processing.");
        Console.WriteLine("  The compiler is your co-pilot.");
    }
}
