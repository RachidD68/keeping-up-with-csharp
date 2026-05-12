// Chapter 7 — Memory & Allocation Control — INTERMEDIATE
// ----------------------------------------------------------------
// Exercise: Build a zero-allocation IPv4AddressParser that takes a
//   ReadOnlySpan<char> like "192.168.1.10" and returns a
//   (byte, byte, byte, byte) tuple, throwing on malformed input.
//   Do not call string.Split, Substring, or any API that allocates.
//
// Hint: Use span.IndexOf('.') to find each dot, then byte.TryParse
//   (span.Slice(start, length), out var octet) on the slices.
//   Every step is slicing — no new strings.
// ----------------------------------------------------------------

namespace KeepUpCs.Exercises.Ch07;

public static class IPv4AddressParser
{
    /// <summary>
    /// Parses a dotted-quad IPv4 string from a span without allocating.
    /// </summary>
    public static (byte A, byte B, byte C, byte D) Parse(ReadOnlySpan<char> input)
    {
        Span<byte> octets = stackalloc byte[4];
        int filled = 0;
        var rest = input;

        while (filled < 4)
        {
            int dot = rest.IndexOf('.');
            ReadOnlySpan<char> slice;
            if (filled < 3)
            {
                if (dot < 0)
                    throw new FormatException($"Expected 4 octets but got {filled + 1}.");
                slice = rest[..dot];
                rest  = rest[(dot + 1)..];
            }
            else
            {
                if (dot >= 0)
                    throw new FormatException("Too many octets — found a 5th dot.");
                slice = rest;
            }

            if (slice.IsEmpty)
                throw new FormatException($"Octet {filled + 1} is empty.");

            if (!byte.TryParse(slice, out byte octet))
                throw new FormatException(
                    $"Octet {filled + 1} (\"{slice.ToString()}\") is not a valid byte.");

            octets[filled++] = octet;
        }

        return (octets[0], octets[1], octets[2], octets[3]);
    }
}

public static class IntermediateDemo
{
    private static void TryParse(string s)
    {
        try
        {
            var (a, b, c, d) = IPv4AddressParser.Parse(s);
            Console.WriteLine($"  ✓ \"{s,-20}\" → ({a}, {b}, {c}, {d})");
        }
        catch (FormatException ex)
        {
            Console.WriteLine($"  ✗ \"{s,-20}\" → {ex.Message}");
        }
    }

    public static void Run()
    {
        Console.WriteLine("Ch07 Intermediate — Zero-allocation IPv4 parser");
        Console.WriteLine(new string('─', 60));

        TryParse("192.168.1.10");
        TryParse("10.0.0.1");
        TryParse("255.255.255.255");
        TryParse("0.0.0.0");
        TryParse("1.2.3");                // too few
        TryParse("1.2.3.4.5");             // too many
        TryParse("256.1.1.1");             // octet overflow
        TryParse("a.b.c.d");               // non-numeric
        TryParse("1..2.3");                // empty octet
    }
}
