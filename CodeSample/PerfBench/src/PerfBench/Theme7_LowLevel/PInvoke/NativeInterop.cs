namespace PerfBench.Theme7_LowLevel;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Feature: P/Invoke and LibraryImport  (.NET 7+)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//
//  PROBLEM
//  -------
//  Calling native C libraries (Win32, libc, custom .so/.dll)
//  required [DllImport] — a runtime-based marshalling system
//  that was opaque, hard to debug, and could not be trimmed
//  by the AOT compiler.  String marshalling was particularly
//  error-prone: wrong CharSet, forgotten MarshalAs attributes,
//  and silent buffer overflows.
//
//  SOLUTION
//  --------
//  [LibraryImport] (introduced in .NET 7) is a source-generated
//  replacement for [DllImport].  The compiler generates visible,
//  debuggable marshalling code at build time.  Benefits:
//  - Compile-time validation of marshalling
//  - AOT/trimmer compatible
//  - Generated code is inspectable in obj/ folder
//  - Partial method syntax for clean separation
//
//  WHY IT MATTERS
//  ──────────────
//  Native interop is essential for calling OS APIs, hardware
//  drivers, cryptographic libraries, and existing C/C++ code.
//  LibraryImport makes it safer, faster, and compatible with
//  modern deployment scenarios (NativeAOT, single-file publish).
//
//  TRY IT
//  ──────
//  1. Change a DllImport example to LibraryImport and rebuild.
//  2. Look at the generated code in obj/Debug/.../Generated/.
//  3. Try calling a real Win32 API (e.g., GetComputerName).
//  4. Experiment with StringMarshalling.Custom.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Demonstrates the evolution from [DllImport] to [LibraryImport]
/// for native interop, using OS-level function calls as examples.
/// </summary>
public static partial class NativeInteropDemo
{
    // ══════════════════════════════════════════════════════════
    // PART A: DllImport — the classic way (runtime marshalling)
    // ══════════════════════════════════════════════════════════

    // ──────────────────────────────────────────────────────────
    // STEP 1: THE PROBLEM — DllImport with runtime marshalling
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Classic DllImport — runtime marshalling, not AOT-friendly.
    /// The CharSet controls how strings are marshalled.
    /// </summary>
    /// <remarks>
    /// This is the traditional pattern. Note the issues:
    /// - Marshalling happens at runtime (invisible, hard to debug)
    /// - Not compatible with AOT compilation
    /// - String marshalling rules are implicit via CharSet
    /// </remarks>
#pragma warning disable SYSLIB1054 // Use LibraryImport instead of DllImport
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern uint GetCurrentProcessId();

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool QueryPerformanceCounter(out long value);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool QueryPerformanceFrequency(out long frequency);
#pragma warning restore SYSLIB1054

    // ══════════════════════════════════════════════════════════
    // PART B: LibraryImport — the modern way (source-generated)
    // ══════════════════════════════════════════════════════════

    // ──────────────────────────────────────────────────────────
    // STEP 2: THE SOLUTION — LibraryImport with compile-time marshalling
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Modern LibraryImport — source-generated, AOT-compatible.
    /// The compiler generates the marshalling code at build time
    /// and places it in a partial method implementation.
    /// </summary>
    /// <remarks>
    /// Key differences from DllImport:
    /// - `partial` method: compiler generates the implementation
    /// - StringMarshalling parameter instead of CharSet
    /// - Generated code is visible in obj/ folder
    /// - Fully AOT and trimmer compatible
    /// </remarks>
    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial uint GetCurrentThreadId();

    [LibraryImport("kernel32.dll", SetLastError = true, EntryPoint = "GetTickCount64")]
    private static partial ulong GetTickCount64Interop();

    [LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    private static partial int GetCurrentDirectoryW(int bufferLength, [Out] char[] buffer);

    // ══════════════════════════════════════════════════════════
    // PART C: Pattern demonstration (syntax comparison)
    // ══════════════════════════════════════════════════════════

    /// <summary>
    /// Shows the DllImport vs LibraryImport syntax side by side
    /// without calling the functions (for cross-platform safety).
    /// </summary>
    private static void ShowSyntaxComparison()
    {
        Console.WriteLine("  SYNTAX COMPARISON — DllImport vs LibraryImport:");
        Console.WriteLine();

        Console.WriteLine("    ── DllImport (classic, runtime marshalling) ──");
        Console.WriteLine("    [DllImport(\"kernel32.dll\", CharSet = CharSet.Unicode, SetLastError = true)]");
        Console.WriteLine("    static extern uint GetCurrentProcessId();");
        Console.WriteLine();
        Console.WriteLine("    Issues:");
        Console.WriteLine("    - Runtime marshalling (invisible, hard to debug)");
        Console.WriteLine("    - Not AOT-compatible");
        Console.WriteLine("    - CharSet enum for string encoding (error-prone)");
        Console.WriteLine("    - `static extern` — no partial, no source generation");
        Console.WriteLine();

        Console.WriteLine("    ── LibraryImport (modern, source-generated) ──");
        Console.WriteLine("    [LibraryImport(\"kernel32.dll\", SetLastError = true)]");
        Console.WriteLine("    private static partial uint GetCurrentThreadId();");
        Console.WriteLine();
        Console.WriteLine("    Benefits:");
        Console.WriteLine("    - Compile-time marshalling (inspectable in obj/ folder)");
        Console.WriteLine("    - Fully AOT and trimmer compatible");
        Console.WriteLine("    - StringMarshalling enum (Utf8, Utf16, Custom)");
        Console.WriteLine("    - `static partial` — compiler generates the body");
        Console.WriteLine("    - Analyzer warnings for common mistakes");
        Console.WriteLine();
    }

    /// <summary>
    /// Shows string marshalling patterns and MarshalAs usage.
    /// </summary>
    private static void ShowStringMarshallingPatterns()
    {
        Console.WriteLine("  STRING MARSHALLING PATTERNS:");
        Console.WriteLine();

        Console.WriteLine("    ── DllImport string marshalling ──");
        Console.WriteLine("    [DllImport(\"lib.dll\", CharSet = CharSet.Ansi)]");
        Console.WriteLine("    static extern int GetNameA([MarshalAs(UnmanagedType.LPStr)] string name);");
        Console.WriteLine();
        Console.WriteLine("    [DllImport(\"lib.dll\", CharSet = CharSet.Unicode)]");
        Console.WriteLine("    static extern int GetNameW([MarshalAs(UnmanagedType.LPWStr)] string name);");
        Console.WriteLine();

        Console.WriteLine("    ── LibraryImport string marshalling ──");
        Console.WriteLine("    [LibraryImport(\"lib.dll\", StringMarshalling = StringMarshalling.Utf16)]");
        Console.WriteLine("    static partial int GetName(string name);   // Utf16 by default");
        Console.WriteLine();
        Console.WriteLine("    [LibraryImport(\"lib.dll\", StringMarshalling = StringMarshalling.Utf8)]");
        Console.WriteLine("    static partial int GetName(string name);   // Cross-platform (Linux)");
        Console.WriteLine();

        Console.WriteLine("    Common MarshalAs types:");
        Console.WriteLine("    - UnmanagedType.LPStr    -> ANSI string (char*)");
        Console.WriteLine("    - UnmanagedType.LPWStr   -> Wide string (wchar_t*)");
        Console.WriteLine("    - UnmanagedType.LPUTF8Str -> UTF-8 string");
        Console.WriteLine("    - UnmanagedType.Bool     -> 4-byte BOOL (Win32)");
        Console.WriteLine("    - UnmanagedType.U1       -> 1-byte bool");
        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates live P/Invoke calls on Windows using both
    /// DllImport and LibraryImport to show they produce the
    /// same results.
    /// </summary>
    private static void DemonstrateLiveCalls()
    {
        Console.WriteLine("  LIVE P/INVOKE CALLS:");
        Console.WriteLine();

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Console.WriteLine("    [Skipped — kernel32.dll calls require Windows]");
            Console.WriteLine("    On Linux/macOS, you would use \"libc\" or \"libSystem.dylib\"");
            Console.WriteLine();
            ShowCrossPlatformPattern();
            return;
        }

        // DllImport call
        uint processId = GetCurrentProcessId();
        Console.WriteLine($"    [DllImport]     GetCurrentProcessId() = {processId}");

        // LibraryImport call
        uint threadId = GetCurrentThreadId();
        Console.WriteLine($"    [LibraryImport] GetCurrentThreadId()  = {threadId}");

        // Performance counter via DllImport
        if (QueryPerformanceCounter(out long counter) && QueryPerformanceFrequency(out long freq))
        {
            double seconds = (double)counter / freq;
            Console.WriteLine($"    [DllImport]     QueryPerfCounter       = {counter:N0} ticks");
            Console.WriteLine($"    [DllImport]     QueryPerfFrequency     = {freq:N0} Hz");
            Console.WriteLine($"    [DllImport]     Uptime                 ~ {seconds:N2} seconds");
        }

        // Tick count via LibraryImport
        ulong ticks = GetTickCount64Interop();
        Console.WriteLine($"    [LibraryImport] GetTickCount64()       = {ticks:N0} ms ({ticks / 1000.0 / 60 / 60:N1} hours)");

        // String output via LibraryImport
        var dirBuffer = new char[260];
        int dirLen = GetCurrentDirectoryW(dirBuffer.Length, dirBuffer);
        if (dirLen > 0)
        {
            string currentDir = new string(dirBuffer, 0, dirLen);
            Console.WriteLine($"    [LibraryImport] GetCurrentDirectory()  = {currentDir}");
        }

        Console.WriteLine();
        Console.WriteLine("    Both DllImport and LibraryImport call the same native functions.");
        Console.WriteLine("    LibraryImport generates the marshalling code at compile time.");
        Console.WriteLine();
    }

    /// <summary>
    /// Shows how to structure cross-platform P/Invoke code.
    /// </summary>
    private static void ShowCrossPlatformPattern()
    {
        Console.WriteLine("  CROSS-PLATFORM P/INVOKE PATTERN:");
        Console.WriteLine();
        Console.WriteLine("    // Use conditional compilation for platform-specific imports:");
        Console.WriteLine("    //");
        Console.WriteLine("    // #if WINDOWS");
        Console.WriteLine("    // [LibraryImport(\"kernel32.dll\")]");
        Console.WriteLine("    // static partial uint GetCurrentProcessId();");
        Console.WriteLine("    // #elif LINUX");
        Console.WriteLine("    // [LibraryImport(\"libc\", EntryPoint = \"getpid\")]");
        Console.WriteLine("    // static partial int GetProcessId();");
        Console.WriteLine("    // #endif");
        Console.WriteLine();
        Console.WriteLine("    // Or use RuntimeInformation for runtime checks:");
        Console.WriteLine("    //");
        Console.WriteLine("    // if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))");
        Console.WriteLine("    //     CallWindowsApi();");
        Console.WriteLine("    // else");
        Console.WriteLine("    //     CallPosixApi();");
        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates struct marshalling for P/Invoke — passing
    /// domain structs to native code.
    /// </summary>
    private static void ShowStructMarshallingPattern()
    {
        Console.WriteLine("  STRUCT MARSHALLING FOR P/INVOKE:");
        Console.WriteLine();

        // Our domain models are already P/Invoke-ready
        Console.WriteLine("    PerfBench domain models are already interop-ready:");
        Console.WriteLine();

        Console.WriteLine($"    Pixel         — [StructLayout(Sequential)]      {Unsafe.SizeOf<Pixel>()} bytes");
        Console.WriteLine($"    PacketHeader  — [StructLayout(Explicit, Size=16)] {Unsafe.SizeOf<PacketHeader>()} bytes");
        Console.WriteLine($"    SensorReading — [StructLayout(Sequential, Pack=1)] {Unsafe.SizeOf<SensorReading>()} bytes");
        Console.WriteLine();

        // Show how a struct would be passed to native code
        Console.WriteLine("    Passing structs to native code:");
        Console.WriteLine("    // By value (copied to native stack):");
        Console.WriteLine("    // [LibraryImport(\"sensor.dll\")]");
        Console.WriteLine("    // static partial int ProcessReading(SensorReading reading);");
        Console.WriteLine();
        Console.WriteLine("    // By reference (pointer to managed memory):");
        Console.WriteLine("    // [LibraryImport(\"sensor.dll\")]");
        Console.WriteLine("    // static partial int ProcessReading(ref SensorReading reading);");
        Console.WriteLine();
        Console.WriteLine("    // Array / span (pointer + length):");
        Console.WriteLine("    // [LibraryImport(\"image.dll\")]");
        Console.WriteLine("    // static partial void ProcessPixels(");
        Console.WriteLine("    //     [MarshalAs(UnmanagedType.LPArray)] Pixel[] pixels, int count);");
        Console.WriteLine();
    }

    // WHY THIS MATTERS:
    // LibraryImport is not just a convenience — it is required
    // for NativeAOT compilation.  DllImport relies on runtime
    // code generation that NativeAOT cannot perform.  If your
    // app targets AOT (MAUI, server, embedded), you must use
    // LibraryImport.

    // GOING DEEPER:
    // The source generator creates a file in:
    //   obj/Debug/net10.0/generated/Microsoft.Interop.LibraryImportGenerator/
    // Open it to see exactly how strings are pinned, buffers
    // are allocated, and error codes are captured.  This is the
    // same code you would have written by hand — but now the
    // compiler writes it for you.

    // ──────────────────────────────────────────────────────────
    // TRY IT
    // ──────────────────────────────────────────────────────────

    // TODO: Convert the DllImport declarations above to LibraryImport
    //       and compare the generated code in obj/.

    // TODO: Call a Linux API (e.g., libc getpid) with LibraryImport
    //       and use StringMarshalling.Utf8.

    // TODO: Create a custom marshaller for Pixel that converts
    //       to/from a native COLORREF (uint).

    /// <summary>Runs the complete P/Invoke demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("  Feature: P/Invoke & LibraryImport");
        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine();

        ShowSyntaxComparison();
        ShowStringMarshallingPatterns();
        DemonstrateLiveCalls();
        ShowStructMarshallingPattern();

        Console.WriteLine("  KEY TAKEAWAY:");
        Console.WriteLine("  Use [LibraryImport] for all new P/Invoke code.");
        Console.WriteLine("  It is source-generated, AOT-compatible, and produces");
        Console.WriteLine("  inspectable marshalling code at compile time.");
        Console.WriteLine();

        Console.WriteLine("───────────────────────────────────────────────────────");
    }
}
