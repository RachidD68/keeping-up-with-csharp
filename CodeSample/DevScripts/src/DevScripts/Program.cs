// ╔══════════════════════════════════════════════════════════╗
// ║  DevScripts — A Developer Toolbox of Scripts & Utils    ║
// ║  Keeping Up with C#: Themes 1, 9, and 10 (Capstone)    ║
// ╚══════════════════════════════════════════════════════════╝

using DevScripts.Theme1_Expressiveness.StringInterpolation;
using DevScripts.Theme1_Expressiveness.ExpressionBodied;
using DevScripts.Theme1_Expressiveness.LocalFunctions;
using DevScripts.Theme1_Expressiveness.TopLevelAndFileBased;
using DevScripts.Theme1_Expressiveness.FileScoped;
using DevScripts.Theme1_Expressiveness.GlobalUsings;
using DevScripts.Theme1_Expressiveness.TargetTypedNew;
using DevScripts.Theme1_Expressiveness.NumericLiterals;
using DevScripts.Theme1_Expressiveness.DefaultLiterals;
using DevScripts.Theme1_Expressiveness.ConstInterpolation;
using DevScripts.Theme1_Expressiveness.PrimaryConstructors;
using DevScripts.Theme1_Expressiveness.CollectionExpressions;
using DevScripts.Theme1_Expressiveness.ParamsEnhancements;
using DevScripts.Theme1_Expressiveness.FieldKeyword;
using DevScripts.Theme9_CompilerTooling.SourceGenerators;
using DevScripts.Theme9_CompilerTooling.CallerInfo;
using DevScripts.Theme9_CompilerTooling.ModuleInitializers;
using DevScripts.Theme9_CompilerTooling.Interceptors;
using DevScripts.Theme9_CompilerTooling.PartialMembers;

Console.OutputEncoding = System.Text.Encoding.UTF8;

PrintBanner();

var demos = BuildDemoList();

if (args.Length > 0 && args[0] == "--all")
{
    RunAll(demos);
    return;
}

RunInteractiveMenu(demos);

static void PrintBanner()
{
    Console.WriteLine();
    Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
    Console.WriteLine("║                                                          ║");
    Console.WriteLine("║   DevScripts — Developer Toolbox                        ║");
    Console.WriteLine("║   Keeping Up with C#: From C# 5 to C# 14               ║");
    Console.WriteLine("║                                                          ║");
    Console.WriteLine("║   Themes: 1 (Expressiveness), 9 (Compiler & Tooling),   ║");
    Console.WriteLine("║           10 (Capstone)                                 ║");
    Console.WriteLine("║                                                          ║");
    Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
    Console.WriteLine();
}

static List<(string Category, string Name, Action Run)> BuildDemoList() =>
[
    // Theme 1 — Expressiveness
    ("Theme 1: Expressiveness", "String Interpolation Evolution", TemplateEngineDemo.Run),
    ("Theme 1: Expressiveness", "Expression-Bodied Members", MetricsCalculatorDemo.Run),
    ("Theme 1: Expressiveness", "Local Functions", RecursiveParserDemo.Run),
    ("Theme 1: Expressiveness", "Top-Level / File-Based Apps", ScriptingDemoClass.Run),
    ("Theme 1: Expressiveness", "File-Scoped Namespaces", NamespaceDemoClass.Run),
    ("Theme 1: Expressiveness", "Global & Implicit Usings", UsingsDemoClass.Run),
    ("Theme 1: Expressiveness", "Target-Typed new", CollectionFactoryDemo.Run),
    ("Theme 1: Expressiveness", "Digit Separators & Binary Literals", BitMaskBuilderDemo.Run),
    ("Theme 1: Expressiveness", "Default Literal", GenericDefaultsDemo.Run),
    ("Theme 1: Expressiveness", "Const Interpolated Strings", ConstantStringsDemo.Run),
    ("Theme 1: Expressiveness", "Primary Constructors", ServiceConfigDemo.Run),
    ("Theme 1: Expressiveness", "Collection Expressions", DataPipelineDemo.Run),
    ("Theme 1: Expressiveness", "params Enhancements", FlexibleApiDemo.Run),
    ("Theme 1: Expressiveness", "The field Keyword", SmartPropertyDemo.Run),

    // Theme 9 — Compiler & Tooling
    ("Theme 9: Compiler & Tooling", "Source Generators", GeneratorConsumerDemo.Run),
    ("Theme 9: Compiler & Tooling", "Caller Info Attributes", DiagnosticLoggerDemo.Run),
    ("Theme 9: Compiler & Tooling", "Module Initializers", StartupHookDemo.Run),
    ("Theme 9: Compiler & Tooling", "Interceptors", InterceptorDemoClass.Run),
    ("Theme 9: Compiler & Tooling", "Partial Members", GeneratedEntityDemo.Run),

    // Theme 10 — Capstone
    ("Theme 10: Capstone", "Scripting Pattern", ScriptingPatternDemo.Run),
    ("Theme 10: Capstone", "Source Generator Pattern", SourceGeneratorPatternDemo.Run),
];

static void RunInteractiveMenu(List<(string Category, string Name, Action Run)> demos)
{
    while (true)
    {
        Console.WriteLine("  Select a demo to run:");
        Console.WriteLine("  ─────────────────────────────────────────────");
        Console.WriteLine();

        var currentCategory = "";
        for (var i = 0; i < demos.Count; i++)
        {
            if (demos[i].Category != currentCategory)
            {
                currentCategory = demos[i].Category;
                Console.WriteLine($"  {currentCategory}");
            }
            Console.WriteLine($"    [{i + 1,2}] {demos[i].Name}");
        }

        Console.WriteLine();
        Console.WriteLine($"    [ A] Run all demos");
        Console.WriteLine($"    [ Q] Quit");
        Console.WriteLine();
        Console.Write("  Enter choice: ");

        var input = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(input) || input.Equals("Q", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("\n  Thank you for exploring DevScripts!");
            return;
        }

        if (input.Equals("A", StringComparison.OrdinalIgnoreCase))
        {
            RunAll(demos);
            continue;
        }

        if (int.TryParse(input, out var choice) && choice >= 1 && choice <= demos.Count)
        {
            Console.WriteLine();
            try
            {
                demos[choice - 1].Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Error: {ex.Message}");
            }
            Console.WriteLine();
            Console.WriteLine("  Press Enter to continue...");
            Console.ReadLine();
        }
        else
        {
            Console.WriteLine("  Invalid choice. Please try again.");
        }
    }
}

static void RunAll(List<(string Category, string Name, Action Run)> demos)
{
    var currentCategory = "";
    foreach (var (category, name, run) in demos)
    {
        if (category != currentCategory)
        {
            currentCategory = category;
            Console.WriteLine();
            Console.WriteLine($"═══════════════════════════════════════════════════");
            Console.WriteLine($"  {category}");
            Console.WriteLine($"═══════════════════════════════════════════════════");
        }

        Console.WriteLine();
        try
        {
            run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Error in {name}: {ex.Message}");
        }
    }

    Console.WriteLine();
    Console.WriteLine("═══════════════════════════════════════════════════");
    Console.WriteLine("  All demos complete!");
    Console.WriteLine("═══════════════════════════════════════════════════");
}
