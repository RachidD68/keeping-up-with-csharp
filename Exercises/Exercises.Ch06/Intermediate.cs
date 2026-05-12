// Chapter 6 — Safety & Robustness — INTERMEDIATE
// ----------------------------------------------------------------
// Exercise: Add a Guard.Against.MatchesRegex(string value, string
//   pattern, [CallerArgumentExpression] string? expression = null)
//   method to the GuardClause struct from the CallerArgumentExpression
//   section. Use it to validate that a channelName matches
//   ^[a-z][a-z0-9-]{2,30}$. Test with a few invalid names and
//   verify the error message includes the expression.
//
// Hint: Regex.IsMatch is the check; the guard pattern is the same
//   as the others.
// ----------------------------------------------------------------

using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace KeepUpCs.Exercises.Ch06;

public static class Guard
{
    public static GuardClause Against => new();
}

public readonly struct GuardClause
{
    /// <summary>
    /// Throws if <paramref name="value"/> does not match the supplied regex
    /// <paramref name="pattern"/>. The original source expression is captured
    /// automatically and embedded in the error message.
    /// </summary>
    public string MatchesRegex(
        string value,
        string pattern,
        [CallerArgumentExpression(nameof(value))] string? expression = null)
    {
        ArgumentNullException.ThrowIfNull(value, expression);
        ArgumentNullException.ThrowIfNull(pattern);

        if (!Regex.IsMatch(value, pattern))
            throw new ArgumentException(
                $"Value '{expression}' (\"{value}\") does not match the pattern '{pattern}'.",
                expression);
        return value;
    }
}

public static class IntermediateDemo
{
    private const string ChannelNamePattern = "^[a-z][a-z0-9-]{2,30}$";

    private static void Validate(string channelName)
    {
        try
        {
            Guard.Against.MatchesRegex(channelName, ChannelNamePattern);
            Console.WriteLine($"  ✓ '{channelName}' valid");
        }
        catch (ArgumentException ex)
        {
            // Show the first line only — the [CallerArgumentExpression] value
            // is embedded in the message.
            Console.WriteLine($"  ✗ {ex.Message.Split('\n')[0]}");
        }
    }

    public static void Run()
    {
        Console.WriteLine("Ch06 Intermediate — Guard.Against.MatchesRegex");
        Console.WriteLine(new string('─', 60));

        // Valid names
        Validate("general");
        Validate("channel-42");

        // Invalid names — too short, starts with digit, contains uppercase, etc.
        Validate("ab");              // too short
        Validate("1general");        // starts with digit
        Validate("General");         // uppercase
        Validate("name with spaces");
        Validate(new string('x', 40)); // too long
    }
}
