// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: Partial Members (Constructors & Events)       ║
// ║  Introduced: C# 13 (properties) / C# 14 (ctors, events) ║
// ║  Theme: 9 — Compiler & Tooling                          ║
// ╚══════════════════════════════════════════════════════════╝

namespace DevScripts.Theme9_CompilerTooling.PartialMembers;

/// <summary>
/// Demonstrates partial properties (C# 13), and explains partial
/// constructors and events (C# 14) — essential for source generator
/// scenarios where generated code needs to interact with user code.
/// </summary>
public static class GeneratedEntityDemo
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — Source generators need partial members
    // ──────────────────────────────────────────────────────────

    public static void BeforePartialMembers()
    {
        Console.WriteLine("  BEFORE — Limited partial member support:");
        Console.WriteLine();
        Console.WriteLine("""
                // C# has had partial methods since C# 3.0, but they were
                // limited: must return void, can't have out parameters,
                // and if not implemented, the call is removed.
                //
                // C# 9 expanded partial methods (can return values, have
                // out params, but must be implemented).
                //
                // But source generators also need:
                // - Partial properties (for generated backing logic)
                // - Partial constructors (for generated initialization)
                // - Partial events (for generated event wiring)
            """);
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — C# 13/14 partial members
    // ──────────────────────────────────────────────────────────

    public static void WithPartialMembers()
    {
        Console.WriteLine("  AFTER (C# 13/14 — Partial properties, constructors, events):");
        Console.WriteLine();

        // Partial properties (C# 13) — working now
        var entity = new UserEntity { Name = "Alice", Email = "alice@example.com" };
        Console.WriteLine($"    UserEntity: {entity.Name}, {entity.Email}");
        Console.WriteLine($"    DisplayName: {entity.DisplayName}");

        Console.WriteLine();
        Console.WriteLine("    Partial properties (C# 13):");
        Console.WriteLine("""
                // User file:
                //   public partial class UserEntity
                //   {
                //       public partial string DisplayName { get; }  // declaration
                //   }
                //
                // Generated file:
                //   public partial class UserEntity
                //   {
                //       public partial string DisplayName =>         // implementation
                //           $"{Name} <{Email}>";
                //   }
            """);

        Console.WriteLine();
        Console.WriteLine("    Partial constructors & events (C# 14 preview):");
        Console.WriteLine("""
                // Requires C# 14 preview. Uncomment when targeting .NET 10 RC or later.
                //
                // User file:
                //   public partial class Entity
                //   {
                //       public partial Entity(string id);     // declaration
                //       public partial event Action? Changed;  // declaration
                //   }
                //
                // Generated file:
                //   public partial class Entity
                //   {
                //       public partial Entity(string id)       // implementation
                //       {
                //           Id = id;
                //           _createdAt = DateTime.UtcNow;
                //       }
                //
                //       public partial event Action? Changed
                //       {
                //           add => _handlers.Add(value);
                //           remove => _handlers.Remove(value);
                //       }
                //   }
            """);

        Console.WriteLine();
        Console.WriteLine("    ✓ Partial properties (C# 13) — split declaration/implementation");
        Console.WriteLine("    ✓ Partial constructors (C# 14) — generated initialization");
        Console.WriteLine("    ✓ Essential for source generators that need to add behavior");
    }

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // Source generators produce code in a separate partial class file.
    // They can add new members, but they need to implement members
    // that the user's code declares. Partial properties and constructors
    // complete this story: the user declares "I want a DisplayName
    // property" and the generator implements it with whatever logic
    // is needed (database mapping, computed values, etc.).

    // GOING DEEPER:
    // The partial member pattern enables a clean contract between
    // user code and generated code:
    // - User declares: "I need this member to exist"
    // - Generator implements: "Here's how it works"
    // This is the same pattern EF Core uses for navigation properties,
    // and MVVM frameworks use for INotifyPropertyChanged. Partial
    // members make it a first-class language feature.

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: Create a partial class with a partial property declaration.
    // In a second file (simulating a generator), implement the property.
    // Verify that both files compile into a single working class.

    /// <summary>
    /// A partial class demonstrating partial properties.
    /// In a real scenario, the implementation would be in a
    /// source-generator-produced file.
    /// </summary>
    public partial class UserEntity
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";

        // Partial property declaration — implementation below
        // In production, the implementation would be in a generated file
        public partial string DisplayName { get; }
    }

    // "Generated" implementation of the partial property
    // (In production, this would be in a separate generated .cs file)
    public partial class UserEntity
    {
        public partial string DisplayName => $"{Name} <{Email}>";
    }

    /// <summary>Runs the complete partial members demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: Partial Members (C# 13/14)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforePartialMembers();
        WithPartialMembers();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
