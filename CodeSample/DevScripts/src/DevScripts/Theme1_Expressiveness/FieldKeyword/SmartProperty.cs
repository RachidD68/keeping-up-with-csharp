// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: The field Keyword                             ║
// ║  Introduced: C# 14                                      ║
// ║  Theme: 1 — Expressiveness & Boilerplate Reduction      ║
// ╚══════════════════════════════════════════════════════════╝

namespace DevScripts.Theme1_Expressiveness.FieldKeyword;

/// <summary>
/// Demonstrates the <c>field</c> keyword — accessing the compiler-
/// generated backing field directly within a property accessor,
/// eliminating the need for explicit backing field declarations.
/// </summary>
public static class SmartPropertyDemo
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — How we did it before (C# 13)
    // ──────────────────────────────────────────────────────────

    public static void BeforeFieldKeyword()
    {
        Console.WriteLine("  BEFORE (C# 13 — Explicit backing field for validation):");
        Console.WriteLine();

        var config = new OldConfig();
        config.MaxRetries = 5;
        Console.WriteLine($"    MaxRetries: {config.MaxRetries}");

        try
        {
            config.MaxRetries = -1; // Should throw
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Console.WriteLine($"    Validation caught: {ex.Message}");
        }

        Console.WriteLine("    ⚠ Explicit backing field '_maxRetries' declared separately");
        Console.WriteLine("    ⚠ Property body references '_maxRetries' — name must stay in sync");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — The modern way (C# 14)
    // ──────────────────────────────────────────────────────────

    public static void WithFieldKeyword()
    {
        Console.WriteLine("  AFTER (C# 14 — The field keyword):");
        Console.WriteLine();

        // The field keyword lets you add logic to a property while
        // still using the auto-generated backing field.

        Console.WriteLine("    The field keyword (C# 14):");
        Console.WriteLine();
        Console.WriteLine("""
                public int MaxRetries
                {
                    get => field;
                    set => field = value >= 0
                        ? value
                        : throw new ArgumentOutOfRangeException(nameof(value));
                }

                ✓ No explicit backing field needed
                ✓ 'field' refers to the auto-generated backing field
                ✓ Validation logic inline, no boilerplate
            """);

        Console.WriteLine();

        var config = new NewConfig();
        config.MaxRetries = 5;
        Console.WriteLine($"    MaxRetries: {config.MaxRetries}");

        config.Timeout = TimeSpan.FromSeconds(30);
        Console.WriteLine($"    Timeout: {config.Timeout}");

        try
        {
            config.MaxRetries = -1;
        }
        catch (ArgumentOutOfRangeException)
        {
            Console.WriteLine("    Validation: Rejected -1 for MaxRetries ✓");
        }

        Console.WriteLine();
        Console.WriteLine("    More field keyword patterns:");
        Console.WriteLine("""
                // Lazy initialization:
                // public string FullPath
                // {
                //     get => field ??= ComputeFullPath();
                // }
                //
                // Change notification:
                // public string Name
                // {
                //     get => field;
                //     set { field = value; OnPropertyChanged(); }
                // }
                //
                // Coercion:
                // public int Priority
                // {
                //     get => field;
                //     set => field = Math.Clamp(value, 0, 10);
                // }
            """);

        Console.WriteLine();
        Console.WriteLine("    ✓ 'field' keyword accesses the auto-generated backing field");
        Console.WriteLine("    ✓ Add validation/logic without declaring a separate field");
        Console.WriteLine("    ✓ Keeps the simplicity of auto-properties with custom logic");
    }

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // The #1 reason developers switch from auto-properties to
    // manual properties is to add validation or change notification.
    // This requires declaring a backing field, which doubles the
    // code and introduces a naming dependency (_name ↔ Name).
    // The field keyword eliminates this friction — you add logic
    // to a property while keeping auto-property simplicity.

    // GOING DEEPER:
    // The field keyword resolves a long-standing design tension:
    // auto-properties are concise but can't have logic, while
    // manual properties have logic but require boilerplate.
    // 'field' provides the middle ground. It's particularly
    // powerful with ??= for lazy initialization:
    //   public string Path { get => field ??= Compute(); }
    // This is a thread-safe, lazy, backing-field-free property.
    // Note: if your class has a member named 'field', you can
    // disambiguate with @field or this.field.

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: When C# 14 is stable, convert these OldConfig
    // properties to use the field keyword. Measure the reduction
    // in lines of code.

    /// <summary>Old-style: explicit backing fields for validation.</summary>
    private sealed class OldConfig
    {
        private int _maxRetries;
        private TimeSpan _timeout = TimeSpan.FromSeconds(10);

        public int MaxRetries
        {
            get => _maxRetries;
            set => _maxRetries = value >= 0
                ? value
                : throw new ArgumentOutOfRangeException(nameof(value));
        }

        public TimeSpan Timeout
        {
            get => _timeout;
            set => _timeout = value > TimeSpan.Zero
                ? value
                : throw new ArgumentOutOfRangeException(nameof(value));
        }
    }

    /// <summary>
    /// C# 14 field keyword — access the compiler-generated backing field
    /// directly within property accessors. No manual backing field needed.
    /// </summary>
    private sealed class NewConfig
    {
        public int MaxRetries
        {
            get => field;
            set => field = value >= 0
                ? value
                : throw new ArgumentOutOfRangeException(nameof(value));
        }

        public TimeSpan Timeout
        {
            get => field;
            set => field = value > TimeSpan.Zero
                ? value
                : throw new ArgumentOutOfRangeException(nameof(value));
        } = TimeSpan.FromSeconds(10);
    }

    /// <summary>Runs the complete field keyword demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: The field Keyword (C# 14)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforeFieldKeyword();
        WithFieldKeyword();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
