// ╔══════════════════════════════════════════════════════════╗
// ║  Feature: File-Scoped Namespaces                        ║
// ║  Introduced: C# 10                                      ║
// ║  Theme: 1 — Expressiveness & Boilerplate Reduction      ║
// ╚══════════════════════════════════════════════════════════╝
//
// THIS FILE IS THE DEMO. Notice the namespace declaration below
// uses a semicolon instead of braces — saving one level of
// indentation for the entire file.

namespace DevScripts.Theme1_Expressiveness.FileScoped;
// ↑ Semicolon, not braces! Every type in this file belongs to this namespace.

/// <summary>
/// Demonstrates file-scoped namespaces — the file itself IS the demo.
/// Every type in this file is in the <c>DevScripts.Theme1_Expressiveness.FileScoped</c>
/// namespace without any braces or extra indentation.
/// </summary>
public static class NamespaceDemoClass
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — How we did it before (C# 9.0)
    // ──────────────────────────────────────────────────────────

    public static void BeforeFileScoped()
    {
        Console.WriteLine("  BEFORE (C# 9.0 — Block-scoped namespaces):");
        Console.WriteLine();
        Console.WriteLine("""
                // namespace DevScripts.Theme1_Expressiveness.FileScoped
                // {                                              ← extra brace
                //     public static class NamespaceDemo          ← indented
                //     {                                          ← indented
                //         public static void Run()               ← double indented
                //         {
                //             // Your code here                  ← triple indented
                //         }
                //     }
                // }                                              ← closing brace
            """);
        Console.WriteLine("    ⚠ Every line in the file indented one extra level");
        Console.WriteLine("    ⚠ Opening/closing braces add visual noise");
        Console.WriteLine();
    }

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — The modern way (C# 10)
    // ──────────────────────────────────────────────────────────

    public static void WithFileScoped()
    {
        Console.WriteLine("  AFTER (C# 10 — File-scoped namespace):");
        Console.WriteLine();
        Console.WriteLine("""
                // namespace DevScripts.Theme1_Expressiveness.FileScoped;
                //                                                ↑ semicolon!
                // public static class NamespaceDemo              ← no extra indent
                // {
                //     public static void Run()                   ← one less indent
                //     {
                //         // Your code here                      ← one less indent
                //     }
                // }
            """);
        Console.WriteLine();
        Console.WriteLine("    This very file uses file-scoped namespace!");
        Console.WriteLine($"    Namespace: {typeof(NamespaceDemoClass).Namespace}");
        Console.WriteLine();
        Console.WriteLine("    ✓ One less indentation level everywhere");
        Console.WriteLine("    ✓ No opening/closing brace pair for namespace");
        Console.WriteLine("    ✓ Encouraged by .editorconfig: file_scoped:warning");
    }

    // ──────────────────────────────────────────────────────────
    // STEP 3: WHY IT MATTERS
    // ──────────────────────────────────────────────────────────

    // WHY THIS MATTERS:
    // This seems trivial, but indentation levels matter. Each level
    // pushes code further right, reducing the usable line width.
    // Since 99% of files contain exactly one namespace, the braces
    // around it were pure ceremony. File-scoped namespaces reclaim
    // horizontal space across every line in every file.

    // GOING DEEPER:
    // File-scoped namespaces are enforced via .editorconfig:
    //   csharp_style_namespace_declarations = file_scoped:warning
    // This means dotnet format will flag block-scoped namespaces.
    // All 5 projects in this book use file-scoped namespaces.
    // Limitation: only one namespace per file (which is already
    // the best practice — one type per file, one namespace per file).

    // ──────────────────────────────────────────────────────────
    // STEP 4: TRY IT — Hands-on exercise
    // ──────────────────────────────────────────────────────────

    // TODO: If you have an older project, try converting it to
    // file-scoped namespaces. Use: dotnet format --diagnostics IDE0161

    /// <summary>Runs the complete file-scoped namespace demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: File-Scoped Namespaces (C# 10)");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        BeforeFileScoped();
        WithFileScoped();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
