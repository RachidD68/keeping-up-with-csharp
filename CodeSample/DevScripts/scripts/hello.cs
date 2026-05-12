// ╔══════════════════════════════════════════════════════════╗
// ║  File-Based App Example (C# 14)                         ║
// ║  Run directly: dotnet run hello.cs                      ║
// ║  No .csproj needed!                                     ║
// ╚══════════════════════════════════════════════════════════╝
//
// This is a C# 14 file-based application. It runs without a
// project file — the SDK creates a temporary project behind
// the scenes. Perfect for quick scripts, utilities, and
// one-off tasks.
//
// To run: dotnet run hello.cs
// (from the scripts/ directory or with the full path)

Console.WriteLine("╔══════════════════════════════════════╗");
Console.WriteLine("║  Hello from a C# 14 file-based app! ║");
Console.WriteLine("╚══════════════════════════════════════╝");
Console.WriteLine();
Console.WriteLine($"  Runtime: {Environment.Version}");
Console.WriteLine($"  OS: {Environment.OSVersion}");
Console.WriteLine($"  Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
Console.WriteLine($"  Args: [{string.Join(", ", args)}]");
Console.WriteLine();

// File-based apps support everything top-level statements do:
// - async/await
// - local functions
// - types defined at the end of the file

var greeting = GenerateGreeting(args);
Console.WriteLine($"  {greeting}");

// Local function
static string GenerateGreeting(string[] args)
{
    var name = args.Length > 0 ? args[0] : "World";
    return $"Greetings, {name}! This script required zero project setup.";
}
