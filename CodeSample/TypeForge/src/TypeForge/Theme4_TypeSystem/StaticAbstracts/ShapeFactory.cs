namespace TypeForge.Theme4_TypeSystem;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: Static Abstract / Virtual Members  (C# 11)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  PROBLEM
//  -------
//  Before C# 11, interfaces couldn't require static members.
//  Factory methods, type metadata, and operator support
//  couldn't be expressed as interface contracts.  You had
//  to rely on conventions, attributes, or reflection.
//
//  SOLUTION
//  --------
//  Static abstract/virtual members let interfaces define
//  contracts for static members — including operators,
//  factory methods, and metadata properties.  Generic code
//  can then call these through type parameters.
//
//  WHY IT MATTERS
//  ──────────────
//  This is the foundation of Generic Math and the "curiously
//  recurring template pattern" in C#.  Your generic methods
//  can now require types to have factories, operators, or
//  metadata — all enforced at compile time.
//
//  TRY IT
//  ──────
//  1. Add a static abstract Parse method to IShape.
//  2. Create a generic ShapeComparer<T> using static abstracts.
//  3. Build a type registry using IPluginFactory<TSelf>.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Demonstrates static abstract members for type-level contracts.
/// </summary>
public static class ShapeFactoryDemo
{
    // ── Before: reflection-based factory ─────────────────────

    /// <summary>
    /// Old way: use reflection to get a static property.
    /// Fragile, no compile-time safety, and slow.
    /// </summary>
    private static string GetShapeNameViaReflection(Type shapeType)
    {
        var prop = shapeType.GetProperty("ShapeName",
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.Static);
        return prop?.GetValue(null)?.ToString() ?? "Unknown";
    }

    // ── After: generic method with static abstract constraint ─

    /// <summary>
    /// New way: static abstract ShapeName is available through
    /// the type parameter — compile-time safe, zero reflection.
    /// </summary>
    private static string GetShapeName<T>() where T : IShape =>
        T.ShapeName;

    /// <summary>
    /// Creates a shape description using only static members.
    /// No instance needed — the type itself provides metadata.
    /// </summary>
    private static string DescribeShapeType<T>() where T : IShape =>
        $"Shape type: {T.ShapeName}";

    /// <summary>
    /// Generic shape processor — works with any IShape.
    /// Static abstract ShapeName is accessed through T.
    /// </summary>
    private static void ProcessShape<T>(T shape) where T : IShape
    {
        Console.WriteLine($"    Processing {T.ShapeName}:");
        Console.WriteLine($"      Area:      {shape.Area:F4}");
        Console.WriteLine($"      Perimeter: {shape.Perimeter:F4}");
        Console.WriteLine($"      {shape.Describe()}");
    }

    /// <summary>
    /// Finds the largest shape by area in a collection.
    /// Generic method using static abstracts for display.
    /// </summary>
    private static T? FindLargest<T>(IReadOnlyList<T> shapes) where T : IShape
    {
        if (shapes.Count == 0) return default;

        var largest = shapes[0];
        foreach (var shape in shapes.Skip(1))
        {
            if (shape.Area > largest.Area)
                largest = shape;
        }
        Console.WriteLine($"    Largest {T.ShapeName}: {largest.Describe()}");
        return largest;
    }

    /// <summary>
    /// Demonstrates IPluginFactory — static abstract factory method.
    /// </summary>
    private sealed class GreetPlugin : IPlugin, IPluginFactory<GreetPlugin>
    {
        public string Id => PluginId;
        public string Name => "Greeter";
        public string Version => "1.0.0";

        // Static abstract implementation
        public static string PluginId => "greet";
        public static GreetPlugin Create() => new();

        public void Initialize() { }
        public string Execute(string input) => $"Hello, {input}!";
    }

    /// <summary>
    /// Generic plugin loader — uses static abstract Create().
    /// </summary>
    private static T LoadPlugin<T>() where T : IPluginFactory<T>, IPlugin
    {
        Console.WriteLine($"    Loading plugin: {T.PluginId}");
        var plugin = T.Create();
        plugin.Initialize();
        return plugin;
    }

    public static void Run()
    {
        Console.WriteLine("╔═══ Static Abstract / Virtual Members ═══╗\n");

        // ── 1. Static abstract ShapeName ─────────────────────
        Console.WriteLine("── Static Abstract Property ──");
        Console.WriteLine($"  Circle name:    {GetShapeName<Circle>()}");
        Console.WriteLine($"  Rectangle name: {GetShapeName<Rectangle>()}");
        Console.WriteLine($"  Triangle name:  {GetShapeName<Triangle>()}");

        // ── 2. Type-level description ────────────────────────
        Console.WriteLine("\n── Type-Level Metadata ──");
        Console.WriteLine($"  {DescribeShapeType<Circle>()}");
        Console.WriteLine($"  {DescribeShapeType<Rectangle>()}");

        // ── 3. Generic shape processing ──────────────────────
        Console.WriteLine("\n── Generic Processing ──");
        ProcessShape(new Circle(5.0));
        ProcessShape(new Rectangle(3.0, 4.0));
        ProcessShape(new Triangle(3.0, 4.0, 5.0));

        // ── 4. Find largest in typed collection ──────────────
        Console.WriteLine("\n── Find Largest ──");
        var circles = new Circle[]
        {
            new(1.0), new(3.0), new(2.0), new(5.0)
        };
        FindLargest(circles);

        var rectangles = new Rectangle[]
        {
            new(2, 3), new(10, 1), new(4, 4)
        };
        FindLargest(rectangles);

        // ── 5. Static abstract factory ───────────────────────
        Console.WriteLine("\n── Static Abstract Factory ──");
        var plugin = LoadPlugin<GreetPlugin>();
        Console.WriteLine($"  Plugin: {plugin.Name} v{plugin.Version}");
        Console.WriteLine($"  Result: {plugin.Execute("TypeForge")}");

        // ── 6. Before vs After comparison ────────────────────
        Console.WriteLine("\n── Before vs After ──");
        Console.WriteLine("  Before: GetShapeNameViaReflection(typeof(Circle))");
        Console.WriteLine($"    = {GetShapeNameViaReflection(typeof(Circle))} (reflection, slow)");
        Console.WriteLine("  After:  GetShapeName<Circle>()");
        Console.WriteLine($"    = {GetShapeName<Circle>()} (compile-time, fast)");

        // ── 7. Key insight ───────────────────────────────────
        Console.WriteLine("\n── Key Insight ──");
        Console.WriteLine("  static abstract = contract for TYPE-level behavior");
        Console.WriteLine("  Regular abstract = contract for INSTANCE-level behavior");
        Console.WriteLine("  Combined, they give you the complete type contract.");
    }
}
