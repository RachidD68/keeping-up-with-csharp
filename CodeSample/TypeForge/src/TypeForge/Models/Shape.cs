namespace TypeForge.Models;

// ╔══════════════════════════════════════════════════════════╗
// ║  Shape hierarchy — the classic OOP domain, evolved      ║
// ║  with modern C# features: abstract statics, default     ║
// ║  interface methods, and generic math.                    ║
// ╚══════════════════════════════════════════════════════════╝

/// <summary>
/// Base interface for all shapes — uses static abstract members (C# 11)
/// to enforce factory methods and metadata at the type level.
/// </summary>
public interface IShape
{
    /// <summary>The human-readable name of this shape type.</summary>
    static virtual string ShapeName => "Shape";

    /// <summary>Calculates the area of this shape.</summary>
    double Area { get; }

    /// <summary>Calculates the perimeter of this shape.</summary>
    double Perimeter { get; }

    /// <summary>A description including dimensions.</summary>
    string Describe();
}

/// <summary>
/// A circle defined by its radius.
/// </summary>
public readonly record struct Circle(double Radius) : IShape
{
    public static string ShapeName => "Circle";
    public double Area => Math.PI * Radius * Radius;
    public double Perimeter => 2 * Math.PI * Radius;

    public string Describe() =>
        $"Circle(r={Radius:F2}, area={Area:F2})";
}

/// <summary>
/// A rectangle defined by width and height.
/// </summary>
public readonly record struct Rectangle(double Width, double Height) : IShape
{
    public static string ShapeName => "Rectangle";
    public double Area => Width * Height;
    public double Perimeter => 2 * (Width + Height);

    public string Describe() =>
        $"Rectangle({Width:F2}x{Height:F2}, area={Area:F2}, perimeter={Perimeter:F2})";
}

/// <summary>
/// A triangle defined by three side lengths.
/// Uses Heron's formula for area.
/// </summary>
public readonly record struct Triangle(double A, double B, double C) : IShape
{
    public static string ShapeName => "Triangle";

    public double Perimeter => A + B + C;

    public double Area
    {
        get
        {
            var s = Perimeter / 2;
            return Math.Sqrt(s * (s - A) * (s - B) * (s - C));
        }
    }

    public string Describe() =>
        $"Triangle({A:F2},{B:F2},{C:F2}, area={Area:F2}, perimeter={Perimeter:F2})";
}
