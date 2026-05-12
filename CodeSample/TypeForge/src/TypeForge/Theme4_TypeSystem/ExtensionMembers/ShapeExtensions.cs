namespace TypeForge.Theme4_TypeSystem;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: Extension Members  (C# 14)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  PROBLEM
//  -------
//  C# 3 introduced extension methods — but ONLY methods.
//  You couldn't add extension properties, static methods,
//  or operators.  Extension methods required a static class
//  with `this` parameter syntax that felt bolted-on.
//
//  SOLUTION
//  --------
//  C# 14 introduces a unified `extension` block syntax.
//  You can now extend any type with properties, methods,
//  static methods, indexers, and more — all in a clean,
//  natural syntax.
//
//  WHY IT MATTERS
//  ──────────────
//  Extension everything — properties feel like native
//  members, static extensions enable utility methods on
//  types you don't own, and the syntax is cleaner.
//
//  TRY IT
//  ──────
//  1. Add an extension property IsLargeArea to IShape.
//  2. Add an extension static method Parse to string.
//  3. Create extension indexer for TypeNode children.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Demonstrates C# 14 extension members alongside classic extension methods.
/// </summary>
public static class ShapeExtensionsDemo
{
    // ── Classic extension methods (C# 3) — still work ────────

    /// <summary>
    /// Scales a circle by a factor (classic extension method).
    /// </summary>
    private static Circle ScaleBy(this Circle circle, double factor) =>
        new(circle.Radius * factor);

    /// <summary>
    /// Converts a shape's area to a specific unit (classic).
    /// </summary>
    private static string AreaIn(this IShape shape, string unit) =>
        $"{shape.Area:F2} {unit}²";

    // ── C# 14 extension blocks — the new way ────────────────
    // C# 14 introduces extension blocks: declare extension
    // properties, methods, and statics in a clean syntax.

    /// <summary>
    /// Defines instance extension members for <see cref="IShape"/>.
    /// </summary>
    /// <param name="shape">The shape used as a receiver.</param>
    extension(IShape shape)
    {
        /// <summary>Whether the shape's area exceeds 100 — an extension property.</summary>
        public bool IsLargeArea => shape.Area > 100;

        /// <summary>Formatted area label — an extension property.</summary>
        public string AreaLabel => $"Area: {shape.Area:F2}";
    }

    /// <summary>Extension method for circle-specific operations.</summary>
    private static double Diameter(this Circle circle) =>
        circle.Radius * 2;

    /// <summary>Extension method for rectangle aspect ratio.</summary>
    private static double AspectRatio(this Rectangle rect) =>
        rect.Width / rect.Height;

    /// <summary>Extension for shape comparison.</summary>
    private static IShape Larger(this IShape a, IShape b) =>
        a.Area >= b.Area ? a : b;

    /// <summary>
    /// Extension for collections of shapes.
    /// </summary>
    private static double TotalArea(this IEnumerable<IShape> shapes) =>
        shapes.Sum(s => s.Area);

    /// <summary>
    /// Generic extension constrained to IShape.
    /// </summary>
    private static T ScaleRadius<T>(this T shape, double factor) where T : IShape =>
        shape; // Can't actually scale a generic shape, but shows the pattern

    /// <summary>
    /// Unit conversion extension.
    /// </summary>
    private static string FormatConversion<TFrom, TTo>(this double value)
        where TFrom : IUnit<TFrom>
        where TTo : IUnit<TTo> =>
        UnitConverter.FormatConversion<TFrom, TTo>(value);

    public static void Run()
    {
        Console.WriteLine("╔═══ Extension Members ═══╗\n");

        var circle = new Circle(10);
        var rect = new Rectangle(15, 8);
        var triangle = new Triangle(3, 4, 5);

        // ── 1. Classic extension methods ─────────────────────
        Console.WriteLine("── Classic Extension Methods (C# 3) ──");
        Console.WriteLine($"  Circle(r=10) scaled by 2: {circle.ScaleBy(2).Describe()}");
        Console.WriteLine($"  Rectangle area in m: {rect.AreaIn("m")}");

        // ── 2. Extension properties (C# 14) ─────────────────
        Console.WriteLine("\n── Extension Properties (C# 14) ──");
        Console.WriteLine($"  Circle is large: {((IShape)circle).IsLargeArea}");
        Console.WriteLine($"  Rect is large:   {((IShape)rect).IsLargeArea}");
        Console.WriteLine($"  Circle area:     {((IShape)circle).AreaLabel}");
        Console.WriteLine($"  Rect area:       {((IShape)rect).AreaLabel}");

        // ── 3. Type-specific extensions ──────────────────────
        Console.WriteLine("\n── Type-Specific Extensions ──");
        Console.WriteLine($"  Circle diameter:     {circle.Diameter():F2}");
        Console.WriteLine($"  Rectangle aspect:    {rect.AspectRatio():F2}");

        // ── 4. Comparison extensions ─────────────────────────
        Console.WriteLine("\n── Comparison Extensions ──");
        IShape larger = ((IShape)circle).Larger(rect);
        Console.WriteLine($"  Larger of circle/rect: {larger.Describe()}");

        // ── 5. Collection extensions ─────────────────────────
        Console.WriteLine("\n── Collection Extensions ──");
        IShape[] shapes = [circle, rect, triangle];
        Console.WriteLine($"  Total area: {shapes.TotalArea():F2}");

        // ── 6. Unit conversion extension ─────────────────────
        Console.WriteLine("\n── Unit Conversion Extensions ──");
        Console.WriteLine($"  {100.0.FormatConversion<Meters, Feet>()}");
        Console.WriteLine($"  {5.0.FormatConversion<Feet, Inches>()}");
        Console.WriteLine($"  {150.0.FormatConversion<Pounds, Kilograms>()}");

        // ── 7. C# 14 extension block syntax ──────────────────
        Console.WriteLine("\n── C# 14 Extension Block Syntax ──");
        Console.WriteLine("  Classic (C# 3):");
        Console.WriteLine("    static bool IsLarge(this IShape shape) => ...");
        Console.WriteLine();
        Console.WriteLine("  C# 14 extension block:");
        Console.WriteLine("    extension(IShape shape)");
        Console.WriteLine("    {");
        Console.WriteLine("        public bool IsLargeArea => shape.Area > 100;   // Property!");
        Console.WriteLine("        public string AreaLabel => $\"Area: {shape.Area:F2}\"; // Property!");
        Console.WriteLine("    }");
        Console.WriteLine();
        Console.WriteLine("  C# 14 static extension block:");
        Console.WriteLine("    extension(IShape)");
        Console.WriteLine("    {");
        Console.WriteLine("        public static IShape Largest(params IShape[] s) => ...");
        Console.WriteLine("    }");
        Console.WriteLine();
        Console.WriteLine("  What's new in C# 14:");
        Console.WriteLine("    ✓ Extension properties (not just methods)");
        Console.WriteLine("    ✓ Extension static methods and operators");
        Console.WriteLine("    ✓ Cleaner syntax with extension(Type receiver) block");
        Console.WriteLine("    ✓ Members reference the receiver variable by name");
    }
}
