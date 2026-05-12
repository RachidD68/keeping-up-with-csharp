// Chapter 2 — Expressiveness & Boilerplate Reduction — INTERMEDIATE
// ----------------------------------------------------------------
// Exercise: BitMaskBuilder defines permission flags with binary
//   literals. Convert those constants into a [Flags] enum named
//   FilePermissions with values None, Read, Write, Execute, Delete,
//   and Admin. Write a helper method Describe(FilePermissions p)
//   that returns a comma-separated string of the set flags, using
//   a collection expression and params ReadOnlySpan<string> where
//   it makes sense.
//
// Hint: Enum.GetValues<T>() plus a HasFlag check gives you the
//   active flags.
// ----------------------------------------------------------------

namespace KeepUpCs.Exercises.Ch02;

[Flags]
public enum FilePermissions
{
    None    = 0,
    Read    = 0b0000_0001,
    Write   = 0b0000_0010,
    Execute = 0b0000_0100,
    Delete  = 0b0000_1000,
    Admin   = 0b0001_0000,
}

public static class FilePermissionsHelper
{
    /// <summary>
    /// Returns a comma-separated list of the flags set on
    /// <paramref name="p"/>, or "None" if no flag is set.
    /// </summary>
    public static string Describe(FilePermissions p)
    {
        if (p == FilePermissions.None)
            return "None";

        // collection expression collects the set flag names
        List<string> set =
        [
            ..from flag in Enum.GetValues<FilePermissions>()
              where flag != FilePermissions.None
              where p.HasFlag(flag)
              select flag.ToString()
        ];

        return JoinFlags(", ", [.. set]);
    }

    /// <summary>
    /// Joins a span of strings with the given separator — uses
    /// params ReadOnlySpan&lt;string&gt; (C# 13) for zero-alloc call sites.
    /// </summary>
    public static string JoinFlags(string separator, params ReadOnlySpan<string> parts)
    {
        if (parts.IsEmpty) return string.Empty;
        // String.Concat on a Span<string> would also work; this
        // makes the loop explicit so readers see what's happening.
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < parts.Length; i++)
        {
            if (i > 0) sb.Append(separator);
            sb.Append(parts[i]);
        }
        return sb.ToString();
    }
}

public static class IntermediateDemo
{
    public static void Run()
    {
        Console.WriteLine("Ch02 Intermediate — FilePermissions [Flags] enum");
        Console.WriteLine(new string('─', 60));

        var none     = FilePermissions.None;
        var reader   = FilePermissions.Read;
        var rw       = FilePermissions.Read | FilePermissions.Write;
        var rwx      = FilePermissions.Read | FilePermissions.Write | FilePermissions.Execute;
        var fullAdmin = rwx | FilePermissions.Delete | FilePermissions.Admin;

        Console.WriteLine($"  None        → {FilePermissionsHelper.Describe(none)}");
        Console.WriteLine($"  Read        → {FilePermissionsHelper.Describe(reader)}");
        Console.WriteLine($"  Read|Write  → {FilePermissionsHelper.Describe(rw)}");
        Console.WriteLine($"  R|W|X       → {FilePermissionsHelper.Describe(rwx)}");
        Console.WriteLine($"  full admin  → {FilePermissionsHelper.Describe(fullAdmin)}");
    }
}
