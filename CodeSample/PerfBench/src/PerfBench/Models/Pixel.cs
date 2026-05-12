namespace PerfBench.Models;

// ╔══════════════════════════════════════════════════════════╗
// ║  Pixel — an unmanaged, blittable color type for         ║
// ║  demonstrating Span<T>, stackalloc, and unsafe code.    ║
// ╚══════════════════════════════════════════════════════════╝

/// <summary>
/// A 32-bit RGBA pixel — unmanaged struct, suitable for
/// Span, stackalloc, and native interop scenarios.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly record struct Pixel(byte R, byte G, byte B, byte A = 255)
{
    public static readonly Pixel Black = new(0, 0, 0);
    public static readonly Pixel White = new(255, 255, 255);
    public static readonly Pixel Red = new(255, 0, 0);
    public static readonly Pixel Green = new(0, 255, 0);
    public static readonly Pixel Blue = new(0, 0, 255);
    public static readonly Pixel Transparent = new(0, 0, 0, 0);

    /// <summary>Packs RGBA into a single 32-bit integer.</summary>
    public uint PackedValue =>
        (uint)(R | (G << 8) | (B << 16) | (A << 24));

    /// <summary>Unpacks a 32-bit integer into a Pixel.</summary>
    public static Pixel FromPacked(uint packed) =>
        new((byte)(packed & 0xFF),
            (byte)((packed >> 8) & 0xFF),
            (byte)((packed >> 16) & 0xFF),
            (byte)((packed >> 24) & 0xFF));

    /// <summary>Linearly interpolates between two pixels.</summary>
    public static Pixel Lerp(Pixel a, Pixel b, float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        return new(
            (byte)(a.R + (b.R - a.R) * t),
            (byte)(a.G + (b.G - a.G) * t),
            (byte)(a.B + (b.B - a.B) * t),
            (byte)(a.A + (b.A - a.A) * t));
    }

    /// <summary>Brightness (0.0 – 1.0) using luminance formula.</summary>
    public float Brightness =>
        (0.299f * R + 0.587f * G + 0.114f * B) / 255f;

    public override string ToString() =>
        $"#{R:X2}{G:X2}{B:X2}{(A < 255 ? $"{A:X2}" : "")}";
}
