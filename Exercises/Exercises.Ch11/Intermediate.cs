// Chapter 11 — Modern C# in Practice — Capstone — INTERMEDIATE
// ----------------------------------------------------------------
// Exercise: Take a piece of code that uses runtime reflection in
//   your project (a custom JsonConverter, a manual attribute scan,
//   a Type.GetMethod call) and replace it with the Source Generator
//   Pattern. Start by writing the code you wish the generator
//   would produce, then write the generator backwards from there.
//
// Hint: Chapter 10's AutoToStringGenerator is the template. Strip
//   it down to the smallest version that works, then grow.
// ----------------------------------------------------------------
//
// Like the Basic exercise, this answer is intentionally open: the
// payoff depends on YOUR codebase. The example below shows the
// "write the target code first, then back into a generator"
// approach on a representative shape: a small enum-to-string
// dispatcher that used to do reflection-based lookup.

namespace KeepUpCs.Exercises.Ch11;

public enum OrderStatus { Pending, Approved, Shipped, Cancelled }

// ── BEFORE — runtime reflection ─────────────────────────────────
public static class ReflectionLabel
{
    /// <summary>
    /// Looks up the [Description] attribute on the enum value at
    /// runtime via reflection. Slow on hot paths, AOT-unfriendly.
    /// </summary>
    public static string Label(OrderStatus s)
    {
        var member = typeof(OrderStatus).GetMember(s.ToString())[0];
        var attr   = member.GetCustomAttributes(
            typeof(System.ComponentModel.DescriptionAttribute), inherit: false)
            .OfType<System.ComponentModel.DescriptionAttribute>().FirstOrDefault();
        return attr?.Description ?? s.ToString();
    }
}

// ── STEP 1 — Write the code you wish the generator would produce ─
public static class GeneratedLabel
{
    public static string Label(OrderStatus s) => s switch
    {
        OrderStatus.Pending   => "Pending review",
        OrderStatus.Approved  => "Approved — ready to ship",
        OrderStatus.Shipped   => "Shipped",
        OrderStatus.Cancelled => "Cancelled by customer",
        _                     => s.ToString(),
    };
}

// ── STEP 2 — The generator (sketched in comments) ───────────────
//
// In DevScripts.Generators (a netstandard2.0 csproj):
//
//   [Generator(LanguageNames.CSharp)]
//   public class EnumLabelGenerator : IIncrementalGenerator
//   {
//       public void Initialize(IncrementalGeneratorInitializationContext ctx)
//       {
//           var enumDecls = ctx.SyntaxProvider.ForAttributeWithMetadataName(
//               "KeepUpCs.GenerateLabelsAttribute",
//               predicate: (n, _) => n is EnumDeclarationSyntax,
//               transform: (c, _) => (EnumDeclarationSyntax)c.TargetNode);
//
//           ctx.RegisterSourceOutput(enumDecls, (spc, e) =>
//           {
//               // 1. Walk e.Members; pull [Description] text via SemanticModel.
//               // 2. Emit a static class <EnumName>Labels with a Label(<EnumName>)
//               //    method returning a switch expression — exactly what
//               //    GeneratedLabel above looks like, but for any annotated enum.
//               // 3. spc.AddSource(...) the partial.
//           });
//       }
//   }
//
// Result: no runtime reflection, no AOT trimming pain, no
// MethodNotFound surprises. Same call site (`Labels.Label(s)`),
// pure switch dispatch under the hood.

public static class IntermediateDemo
{
    public static void Run()
    {
        Console.WriteLine("Ch11 Intermediate — Source Generator Pattern (reflection → switch)");
        Console.WriteLine(new string('─', 60));

        foreach (var s in Enum.GetValues<OrderStatus>())
        {
            // ReflectionLabel reads the [Description] attribute at runtime —
            // there's no such attribute on this enum, so it falls back to
            // ToString(). It's the SHAPE that matters here, not the output.
            Console.WriteLine($"  reflection : {s,-9} → \"{ReflectionLabel.Label(s)}\"");
            Console.WriteLine($"  generated  : {s,-9} → \"{GeneratedLabel.Label(s)}\"");
        }

        Console.WriteLine();
        Console.WriteLine("  See the appendix entry for the full IIncrementalGenerator sketch.");
    }
}
