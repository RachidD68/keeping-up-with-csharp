namespace TypeForge.Theme4_TypeSystem;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: Abstract Classes & Virtual Dispatch (evolved)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  PROBLEM
//  -------
//  Abstract classes provide shared state and behavior, but
//  the classic OOP patterns (Template Method, Visitor) were
//  verbose and required many boilerplate types.
//
//  SOLUTION
//  --------
//  Modern C# streamlines abstract classes with:
//  • Primary constructors (C# 12)
//  • Records (which can be abstract)
//  • Sealed on records prevents derived-type ToString
//  • Covariant return types (C# 9)
//
//  WHY IT MATTERS
//  ──────────────
//  Abstract classes remain essential when you need shared
//  state, protected members, or the Template Method pattern.
//  Modern C# makes them much less verbose.
//
//  TRY IT
//  ──────
//  1. Add a TypeTransformer<T> that converts types.
//  2. Use covariant returns in a virtual Create() method.
//  3. Implement the Visitor pattern for TypeNode.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Demonstrates abstract classes with the Visitor pattern for TypeNode.
/// </summary>
public static class TypeVisitorDemo
{
    // ── Abstract Visitor base ────────────────────────────────

    /// <summary>
    /// Abstract TypeNode visitor — the classic Visitor pattern,
    /// modernized with primary constructors and expression bodies.
    /// </summary>
    private abstract class TypeNodeVisitor<TResult>
    {
        public TResult Visit(TypeNode node) => node switch
        {
            PrimitiveType p => VisitPrimitive(p),
            ArrayType a => VisitArray(a),
            NullableType n => VisitNullable(n),
            GenericType g => VisitGeneric(g),
            TupleType t => VisitTuple(t),
            FunctionType f => VisitFunction(f),
            _ => VisitUnknown(node)
        };

        protected abstract TResult VisitPrimitive(PrimitiveType node);
        protected abstract TResult VisitArray(ArrayType node);
        protected abstract TResult VisitNullable(NullableType node);
        protected abstract TResult VisitGeneric(GenericType node);
        protected abstract TResult VisitTuple(TupleType node);
        protected abstract TResult VisitFunction(FunctionType node);

        protected virtual TResult VisitUnknown(TypeNode node) =>
            throw new NotSupportedException($"Unknown node: {node.GetType().Name}");
    }

    // ── Concrete visitors ────────────────────────────────────

    /// <summary>
    /// Generates TypeScript type syntax from C# TypeNode.
    /// </summary>
    private sealed class TypeScriptEmitter : TypeNodeVisitor<string>
    {
        private static readonly Dictionary<string, string> TypeMap = new()
        {
            ["int"] = "number",
            ["long"] = "number",
            ["double"] = "number",
            ["float"] = "number",
            ["decimal"] = "number",
            ["string"] = "string",
            ["bool"] = "boolean",
            ["void"] = "void",
            ["object"] = "any",
        };

        protected override string VisitPrimitive(PrimitiveType node) =>
            TypeMap.GetValueOrDefault(node.Name, node.Name);

        protected override string VisitArray(ArrayType node) =>
            $"{Visit(node.ElementType)}[]";

        protected override string VisitNullable(NullableType node) =>
            $"{Visit(node.InnerType)} | null";

        protected override string VisitGeneric(GenericType node) => node.Name switch
        {
            "List" or "IList" or "IReadOnlyList" =>
                $"{Visit(node.TypeArguments[0])}[]",
            "Dictionary" or "IDictionary" =>
                $"Record<{Visit(node.TypeArguments[0])}, {Visit(node.TypeArguments[1])}>",
            _ => $"{node.Name}<{string.Join(", ", node.TypeArguments.Select(Visit))}>"
        };

        protected override string VisitTuple(TupleType node) =>
            $"{{ {string.Join("; ", node.Elements.Select(e => $"{e.Name}: {Visit(e.Type)}"))} }}";

        protected override string VisitFunction(FunctionType node) =>
            $"({string.Join(", ", node.Parameters.Select((p, i) => $"p{i}: {Visit(p)}"))}) => {Visit(node.ReturnType)}";
    }

    /// <summary>
    /// Counts memory size requirements for a type tree.
    /// </summary>
    private sealed class SizeCalculator : TypeNodeVisitor<int>
    {
        protected override int VisitPrimitive(PrimitiveType node) =>
            node.SizeBytes > 0 ? node.SizeBytes : 8; // reference = 8 bytes (64-bit)

        protected override int VisitArray(ArrayType node) =>
            8 + 4 + Visit(node.ElementType); // ref + length + element

        protected override int VisitNullable(NullableType node) =>
            1 + Visit(node.InnerType); // bool hasValue + inner

        protected override int VisitGeneric(GenericType node) =>
            8 + node.TypeArguments.Sum(Visit); // ref + type args

        protected override int VisitTuple(TupleType node) =>
            node.Elements.Sum(e => Visit(e.Type));

        protected override int VisitFunction(FunctionType node) =>
            16; // delegate = object ref + function pointer
    }

    // ── Abstract template method pattern ─────────────────────

    /// <summary>
    /// Template Method pattern — abstract class defines the algorithm,
    /// subclasses fill in the steps.
    /// </summary>
    private abstract class TypeProcessor(string name)
    {
        public string Name { get; } = name;

        /// <summary>Template method — fixed algorithm.</summary>
        public string Process(TypeNode type)
        {
            var validated = Validate(type);
            var transformed = Transform(validated);
            return Format(transformed);
        }

        protected abstract TypeNode Validate(TypeNode type);
        protected abstract TypeNode Transform(TypeNode type);
        protected abstract string Format(TypeNode type);
    }

    /// <summary>Concrete processor that simplifies types.</summary>
    private sealed class SimplifyProcessor() : TypeProcessor("Simplifier")
    {
        protected override TypeNode Validate(TypeNode type) => type;

        protected override TypeNode Transform(TypeNode type) => type switch
        {
            // Simplify List<T> to T[]
            GenericType { Name: "List", TypeArguments: [var elem] } => new ArrayType(elem),
            // Simplify Nullable<T> to T?
            GenericType { Name: "Nullable", TypeArguments: [var inner] } => new NullableType(inner),
            _ => type
        };

        protected override string Format(TypeNode type) => type.Format();
    }

    public static void Run()
    {
        Console.WriteLine("╔═══ Abstract Classes & Virtual Dispatch ═══╗\n");

        // Build test types
        var intType = new PrimitiveType("int", 4);
        var stringType = new PrimitiveType("string", -1);
        var boolType = new PrimitiveType("bool", 1);

        var types = new TypeNode[]
        {
            intType,
            new ArrayType(stringType),
            new NullableType(intType),
            new GenericType("Dictionary", [stringType, new ArrayType(intType)]),
            new TupleType([("name", stringType), ("age", intType)]),
            new FunctionType([stringType, intType], boolType),
        };

        // ── 1. Visitor pattern: TypeScript emitter ───────────
        Console.WriteLine("── Visitor: C# → TypeScript ──");
        var tsEmitter = new TypeScriptEmitter();
        foreach (var type in types)
        {
            Console.WriteLine($"  C#: {type.Format(),-40} → TS: {tsEmitter.Visit(type)}");
        }

        // ── 2. Visitor pattern: Size calculator ──────────────
        Console.WriteLine("\n── Visitor: Memory Size ──");
        var sizeCalc = new SizeCalculator();
        foreach (var type in types)
        {
            Console.WriteLine($"  {type.Format(),-40} → {sizeCalc.Visit(type)} bytes");
        }

        // ── 3. Template method pattern ───────────────────────
        Console.WriteLine("\n── Template Method: Simplifier ──");
        var simplifier = new SimplifyProcessor();
        var listInt = new GenericType("List", [intType]);
        var nullableDouble = new GenericType("Nullable", [new PrimitiveType("double", 8)]);

        Console.WriteLine($"  {listInt.Format()} → {simplifier.Process(listInt)}");
        Console.WriteLine($"  {nullableDouble.Format()} → {simplifier.Process(nullableDouble)}");
        Console.WriteLine($"  {intType.Format()} → {simplifier.Process(intType)}");

        // ── 4. Primary constructor in abstract class ─────────
        Console.WriteLine("\n── Primary Constructor on Abstract Class ──");
        Console.WriteLine($"  Processor name: {simplifier.Name}");
        Console.WriteLine("  abstract class TypeProcessor(string name)");
        Console.WriteLine("  → Primary constructor captures shared state");
        Console.WriteLine("  → Subclass: SimplifyProcessor() : TypeProcessor(\"Simplifier\")");

        // ── 5. Key insight ───────────────────────────────────
        Console.WriteLine("\n── Key Insight ──");
        Console.WriteLine("  Abstract classes provide:");
        Console.WriteLine("    ✓ Shared state (fields, auto-properties)");
        Console.WriteLine("    ✓ Template Method pattern (fixed algorithm)");
        Console.WriteLine("    ✓ Visitor pattern (type-safe dispatch)");
        Console.WriteLine("    ✓ Primary constructors (C# 12 — less boilerplate)");
        Console.WriteLine("  Interfaces provide:");
        Console.WriteLine("    ✓ Multiple inheritance");
        Console.WriteLine("    ✓ Default methods");
        Console.WriteLine("    ✓ Static abstracts");
        Console.WriteLine("  Choose based on whether you need state or composition.");
    }
}
