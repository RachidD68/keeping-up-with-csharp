namespace TypeForge.Theme4_TypeSystem;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: Interface Hierarchies & Composition  (C# 8+)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  PROBLEM
//  -------
//  Large monolithic interfaces violate ISP (Interface
//  Segregation Principle).  Clients are forced to implement
//  methods they don't need, or interfaces become too granular
//  to be useful.
//
//  SOLUTION
//  --------
//  Compose small, focused interfaces into larger contracts.
//  Default methods provide sensible defaults.  Static
//  abstracts add type-level contracts.  The result: flexible,
//  composable type hierarchies.
//
//  WHY IT MATTERS
//  ──────────────
//  A plugin system needs extensibility WITHOUT forcing every
//  plugin to implement everything.  Interface composition
//  with defaults is the solution.
//
//  TRY IT
//  ──────
//  1. Add an IVersioned interface with MajorVersion/MinorVersion.
//  2. Create a combined IFullPlugin : IPlugin, IVersioned, ICloneable.
//  3. Use default methods so existing plugins don't break.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Demonstrates interface composition and hierarchies for type modeling.
/// </summary>
public static class TypeHierarchyDemo
{
    // ── Small, focused interfaces ────────────────────────────

    /// <summary>Can be rendered as a string.</summary>
    private interface IRenderable
    {
        string Render();
    }

    /// <summary>Can be validated.</summary>
    private interface IValidatable
    {
        ValidationResult Validate();
    }

    /// <summary>Can be serialized to a dictionary.</summary>
    private interface ISerializable
    {
        Dictionary<string, object> Serialize();

        /// <summary>Default: creates a simple dictionary.</summary>
        string ToJson() =>
            $"{{{string.Join(", ", Serialize().Select(kv => $"\"{kv.Key}\": \"{kv.Value}\""))}}}";
    }

    /// <summary>Can be compared by similarity.</summary>
    private interface ISimilar<in T>
    {
        /// <summary>Returns a similarity score between 0 and 1.</summary>
        double SimilarityTo(T other);
    }

    // ── Composed interfaces ──────────────────────────────────

    /// <summary>
    /// A type model that combines multiple capabilities.
    /// This is interface composition — no monolithic interface needed.
    /// </summary>
    private interface ITypeModel : IRenderable, IValidatable, ISerializable
    {
        string Name { get; }

        // Default method using other interface members
        string Summary() =>
            $"{Name}: {(Validate().IsValid ? "✓ valid" : "✗ invalid")} — {Render()}";
    }

    // ── Concrete implementations ─────────────────────────────

    /// <summary>
    /// A field definition in a type — implements the composed interface.
    /// </summary>
    private record FieldModel(string Name, TypeNode Type, bool IsRequired) : ITypeModel
    {
        public string Render() =>
            $"{(IsRequired ? "required " : "")}{Type.Format()} {Name}";

        public ValidationResult Validate()
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(Name))
                errors.Add("Field name cannot be empty");
            if (Name.Length > 100)
                errors.Add("Field name too long (max 100)");
            return errors.Count == 0
                ? ValidationResult.Success
                : ValidationResult.Failure([.. errors]);
        }

        public Dictionary<string, object> Serialize() => new()
        {
            ["name"] = Name,
            ["type"] = Type.Format(),
            ["required"] = IsRequired
        };
    }

    /// <summary>
    /// A composite type with fields — also implements ITypeModel + ISimilar.
    /// </summary>
    private record CompositeModel(
        string Name,
        IReadOnlyList<FieldModel> Fields)
        : ITypeModel, ISimilar<CompositeModel>
    {
        public string Render()
        {
            var fieldsStr = string.Join(", ", Fields.Select(f => f.Render()));
            return $"type {Name} {{ {fieldsStr} }}";
        }

        public ValidationResult Validate() =>
            ValidationResult.Combine(
                [.. Fields.Select(f => f.Validate())]);

        public Dictionary<string, object> Serialize() => new()
        {
            ["name"] = Name,
            ["fieldCount"] = Fields.Count,
            ["fields"] = string.Join(", ", Fields.Select(f => f.Name))
        };

        /// <summary>
        /// Similarity based on shared field names.
        /// </summary>
        public double SimilarityTo(CompositeModel other)
        {
            var myFields = Fields.Select(f => f.Name).ToHashSet();
            var otherFields = other.Fields.Select(f => f.Name).ToHashSet();
            var intersection = myFields.Intersect(otherFields).Count();
            var union = myFields.Union(otherFields).Count();
            return union == 0 ? 1.0 : (double)intersection / union;
        }
    }

    public static void Run()
    {
        Console.WriteLine("╔═══ Interface Hierarchies & Composition ═══╗\n");

        // ── 1. Create type models ────────────────────────────
        Console.WriteLine("── Building Type Models ──");
        var intType = new PrimitiveType("int", 4);
        var stringType = new PrimitiveType("string", -1);

        var userFields = new FieldModel[]
        {
            new("Id", intType, IsRequired: true),
            new("Name", stringType, IsRequired: true),
            new("Email", new NullableType(stringType), IsRequired: false),
        };

        var orderFields = new FieldModel[]
        {
            new("Id", intType, IsRequired: true),
            new("Name", stringType, IsRequired: true),
            new("Total", new PrimitiveType("decimal", 16), IsRequired: true),
        };

        var userModel = new CompositeModel("User", userFields);
        var orderModel = new CompositeModel("Order", orderFields);

        Console.WriteLine($"  {userModel.Render()}");
        Console.WriteLine($"  {orderModel.Render()}");

        // ── 2. IRenderable ───────────────────────────────────
        Console.WriteLine("\n── IRenderable ──");
        foreach (var field in userFields)
            Console.WriteLine($"  Field: {field.Render()}");

        // ── 3. IValidatable ──────────────────────────────────
        Console.WriteLine("\n── IValidatable ──");
        var validResult = userModel.Validate();
        Console.WriteLine($"  User model:  {(validResult.IsValid ? "✓ Valid" : "✗ Invalid")}");

        var invalidField = new FieldModel("", intType, true);
        var invalidResult = invalidField.Validate();
        Console.WriteLine($"  Empty field: {(invalidResult.IsValid ? "✓" : "✗")} {string.Join(", ", invalidResult.Errors)}");

        // ── 4. ISerializable + default ToJson() ──────────────
        Console.WriteLine("\n── ISerializable (with default ToJson) ──");
        // Access ToJson() through the interface — it's a default method
        ISerializable serializable = userModel;
        Console.WriteLine($"  User: {serializable.ToJson()}");

        ISerializable fieldSerializable = userFields[0];
        Console.WriteLine($"  Field: {fieldSerializable.ToJson()}");

        // ── 5. ISimilar<T> ───────────────────────────────────
        Console.WriteLine("\n── ISimilar<T> ──");
        var similarity = userModel.SimilarityTo(orderModel);
        Console.WriteLine($"  User ↔ Order similarity: {similarity:P0}");
        Console.WriteLine($"  (Shared fields: Id, Name)");

        // ── 6. Composed Summary() default method ─────────────
        Console.WriteLine("\n── ITypeModel.Summary() (default method) ──");
        ITypeModel typeModel = userModel;
        Console.WriteLine($"  {typeModel.Summary()}");

        // ── 7. Polymorphism through composed interfaces ──────
        Console.WriteLine("\n── Polymorphic Processing ──");
        IRenderable[] renderables = [userModel, orderModel, .. userFields];
        Console.WriteLine("  All renderables:");
        foreach (var r in renderables)
            Console.WriteLine($"    • {r.Render()}");

        // ── 8. Key insight ───────────────────────────────────
        Console.WriteLine("\n── Key Insight ──");
        Console.WriteLine("  Small interfaces: IRenderable, IValidatable, ISerializable");
        Console.WriteLine("  Composed: ITypeModel : IRenderable, IValidatable, ISerializable");
        Console.WriteLine("  Optional: ISimilar<T> added only where needed");
        Console.WriteLine("  Defaults: ToJson() and Summary() come for free");
        Console.WriteLine("  → Flexible, composable, non-breaking type design.");
    }
}
