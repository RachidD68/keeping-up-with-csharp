namespace TypeForge.Models;

// ╔══════════════════════════════════════════════════════════╗
// ║  TypeNode — a recursive type hierarchy for representing ║
// ║  type systems, demonstrating sealed hierarchies,        ║
// ║  abstract classes, and pattern matching targets.        ║
// ╚══════════════════════════════════════════════════════════╝

/// <summary>
/// Abstract base for type system nodes.
/// Sealed hierarchy ensures exhaustive pattern matching.
/// </summary>
public abstract record TypeNode(string Name)
{
    /// <summary>The formatted display representation.</summary>
    public abstract string Format();
}

/// <summary>Primitive types (int, string, bool, etc.).</summary>
public sealed record PrimitiveType(string Name, int SizeBytes) : TypeNode(Name)
{
    public override string Format() => Name;
}

/// <summary>Array type wrapping an element type.</summary>
public sealed record ArrayType(TypeNode ElementType) : TypeNode($"{ElementType.Name}[]")
{
    public override string Format() => $"{ElementType.Format()}[]";
}

/// <summary>Nullable wrapper around a value type.</summary>
public sealed record NullableType(TypeNode InnerType) : TypeNode($"{InnerType.Name}?")
{
    public override string Format() => $"{InnerType.Format()}?";
}

/// <summary>Generic type with type arguments.</summary>
public sealed record GenericType(
    string Name,
    IReadOnlyList<TypeNode> TypeArguments) : TypeNode(Name)
{
    public override string Format() =>
        $"{Name}<{string.Join(", ", TypeArguments.Select(t => t.Format()))}>";
}

/// <summary>Tuple type with named elements.</summary>
public sealed record TupleType(
    IReadOnlyList<(string Name, TypeNode Type)> Elements) : TypeNode("Tuple")
{
    public override string Format() =>
        $"({string.Join(", ", Elements.Select(e => $"{e.Type.Format()} {e.Name}"))})";
}

/// <summary>Function/delegate type.</summary>
public sealed record FunctionType(
    IReadOnlyList<TypeNode> Parameters,
    TypeNode ReturnType) : TypeNode("Func")
{
    public override string Format()
    {
        var paramStr = string.Join(", ", Parameters.Select(p => p.Format()));
        return $"({paramStr}) -> {ReturnType.Format()}";
    }
}
