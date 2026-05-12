namespace TypeForge.Models;

// ╔══════════════════════════════════════════════════════════╗
// ║  ValidationRule — a composable validation system using  ║
// ║  interfaces, generics, and the type system to enforce   ║
// ║  correctness at compile time.                           ║
// ╚══════════════════════════════════════════════════════════╝

/// <summary>
/// A validation result — either success or failure with reasons.
/// </summary>
public record ValidationResult(
    bool IsValid,
    IReadOnlyList<string> Errors)
{
    public static readonly ValidationResult Success = new(true, []);

    public static ValidationResult Failure(params string[] errors) =>
        new(false, errors);

    public static ValidationResult Combine(params ValidationResult[] results)
    {
        var allErrors = results
            .Where(r => !r.IsValid)
            .SelectMany(r => r.Errors)
            .ToList();

        return allErrors.Count == 0 ? Success : new(false, allErrors);
    }
}

/// <summary>
/// A validation rule that checks a value of type T.
/// </summary>
public interface IValidationRule<in T>
{
    /// <summary>The human-readable description of this rule.</summary>
    string Description { get; }

    /// <summary>Validates the given value.</summary>
    ValidationResult Validate(T value);
}

/// <summary>
/// A composable validator that applies multiple rules.
/// </summary>
public sealed class Validator<T>
{
    private readonly List<IValidationRule<T>> _rules = [];

    /// <summary>Adds a rule to this validator.</summary>
    public Validator<T> AddRule(IValidationRule<T> rule)
    {
        _rules.Add(rule);
        return this;
    }

    /// <summary>Adds a rule using a lambda.</summary>
    public Validator<T> AddRule(
        string description,
        Func<T, bool> predicate,
        string errorMessage)
    {
        _rules.Add(new LambdaRule<T>(description, predicate, errorMessage));
        return this;
    }

    /// <summary>Validates a value against all rules.</summary>
    public ValidationResult Validate(T value) =>
        ValidationResult.Combine(
            [.. _rules.Select(r => r.Validate(value))]);

    public int RuleCount => _rules.Count;
}

/// <summary>
/// A validation rule defined by a lambda.
/// </summary>
file sealed class LambdaRule<T>(
    string description,
    Func<T, bool> predicate,
    string errorMessage) : IValidationRule<T>
{
    public string Description => description;

    public ValidationResult Validate(T value) =>
        predicate(value)
            ? ValidationResult.Success
            : ValidationResult.Failure(errorMessage);
}
