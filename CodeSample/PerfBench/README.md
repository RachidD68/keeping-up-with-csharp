# PerfBench

**Keeping Up with C# — Companion Project 5**

A performance-oriented project exploring **memory management, zero-allocation patterns, low-level programming, and native interop** — Themes 6, 7, and 10 of the book.

## Quick Start

```bash
cd src/PerfBench
dotnet run              # Interactive menu
dotnet run -- --all     # Run all demos sequentially
dotnet run -c Release -- --bench                       # Run all benchmarks
dotnet run -c Release -- --bench --filter *Span*       # Run specific benchmarks
dotnet run -c Release -- --bench --list flat            # List available benchmarks
dotnet run -c Release -- --bench --filter *AsyncMemoryProcessor*   # Async Memory<T> benchmarks
dotnet run -c Release -- --bench --anyCategories MemoryVsSpan      # Category filters (MemoryVsSpan/Pooling/Async)
```

## Themes & Features

### Theme 6 — Memory & Allocation (10 demos + 8 benchmark classes)

| # | Feature | File | C# Version |
|---|---------|------|------------|
| 1 | Creating Span\<T\> — 9 Patterns | `SpanAndMemory/CreatingSpanDemo.cs` | C# 7.2 |
| 2 | Span\<T\> — Zero-Copy Slicing | `SpanAndMemory/BufferProcessor.cs` | C# 7.2 |
| 3 | Span\<T\> API — Properties & Methods | `SpanAndMemory/SpanApiDemo.cs` | C# 7.2 |
| 4 | Memory\<T\> & MemoryPool\<T\> | `MemoryT/AsyncMemoryProcessor.cs` | C# 7.2 / .NET 6+ |
| 3 | stackalloc in Safe Contexts | `Stackalloc/StackAllocDemo.cs` | C# 7.2+ |
| 4 | ArrayPool\<T\> | `ArrayPool/PooledProcessing.cs` | .NET Core |
| 5 | ref Returns & ref Locals | `RefReturns/RefAccessors.cs` | C# 7 |
| 6 | Interpolated String Handlers | `StringInterpolation/ZeroAllocFormatter.cs` | C# 10 |
| 7 | FrozenDictionary / FrozenSet | `FrozenCollections/ImmutableLookup.cs` | .NET 8 |
| 8 | ref Fields in ref Structs | `RefFields/RefStructContainers.cs` | C# 11 |

#### Benchmarks (BenchmarkDotNet)

| Benchmark | File | What It Measures |
|-----------|------|-----------------|
| SpanBenchmarks | `Benchmarks/SpanBenchmarks.cs` | Span slice vs Array.Copy; Span parse vs Substring; Array reverse vs Span.Reverse |
| DateParsingBenchmarks | `Benchmarks/SpanBenchmarks.cs` | Substring vs String.Split vs Span date parsing (3-way) |
| MemoryPoolBenchmarks | `Benchmarks/MemoryPoolBenchmarks.cs` | MemoryPool vs ArrayPool vs raw alloc; async overhead |
| StackallocBenchmarks | `Benchmarks/StackallocBenchmarks.cs` | stackalloc vs heap scratch buffers |
| ArrayPoolBenchmarks | `Benchmarks/ArrayPoolBenchmarks.cs` | ArrayPool vs heap for sensor batches |
| StringHandlerBenchmarks | `Benchmarks/StringHandlerBenchmarks.cs` | Handler short-circuit vs old-style interpolation |
| FrozenCollectionBenchmarks | `Benchmarks/FrozenCollectionBenchmarks.cs` | FrozenDictionary/Set vs Dictionary/HashSet |
| AsyncMemoryProcessorBenchmarks | `Benchmarks/AsyncMemoryProcessorBenchmarks.cs` | Span vs Memory awaits; ArrayPool vs MemoryPool; pooled allocation loops |

### Theme 7 — Low-Level & Interop (5 features)

| # | Feature | File | C# Version |
|---|---------|------|------------|
| 1 | Unsafe Code & Pointers | `UnsafeCode/PointerArithmetic.cs` | Classic |
| 2 | Unsafe Utilities Class | `UnsafeClass/UnsafeUtilities.cs` | .NET Core |
| 3 | P/Invoke & LibraryImport | `PInvoke/NativeInterop.cs` | C# 12 |
| 4 | StructLayout & Binary Protocols | `StructLayout/BinaryProtocol.cs` | Classic |
| 5 | Function Pointers | `FunctionPointers/HighPerfCallbacks.cs` | C# 9 |

### Theme 10 — Capstone Patterns (2 patterns)

| Pattern | File | Features Combined |
|---------|------|-------------------|
| Zero-Allocation Pipeline | `Theme10_Capstone/ZeroAllocPipeline.cs` | Span, stackalloc, ArrayPool, ref struct, ref returns |
| Native Interop Wrapper | `Theme10_Capstone/NativeInteropWrapper.cs` | StructLayout, MemoryMarshal, Unsafe, Span, ref struct |

## Domain Model

```
Pixel          — 32-bit RGBA unmanaged struct (blittable, StructLayout.Sequential)
Matrix         — 2D matrix backed by ArrayPool<double> with Span accessors
SensorReading  — Unmanaged sensor data struct (Sequential, Pack=1)
PacketHeader   — Network packet header (Explicit layout, 16 bytes)
MemoryRegion   — Allocation tracking record for diagnostics
```

## Folder Structure

```
PerfBench/
├── PerfBench.sln
├── .editorconfig
├── README.md
└── src/PerfBench/
    ├── PerfBench.csproj
    ├── GlobalUsings.cs
    ├── Program.cs
    ├── Models/
    │   ├── Pixel.cs
    │   ├── Matrix.cs
    │   ├── SensorReading.cs
    │   ├── PacketHeader.cs
    │   └── MemoryRegion.cs
    ├── Theme6_Memory/
    │   ├── _ThemeIntro.cs
    │   ├── SpanAndMemory/
    │   │   ├── BufferProcessor.cs
    │   │   ├── SpanApiDemo.cs
    │   │   └── CreatingSpanDemo.cs
    │   ├── MemoryT/AsyncMemoryProcessor.cs
    │   ├── Stackalloc/StackAllocDemo.cs
    │   ├── ArrayPool/PooledProcessing.cs
    │   ├── RefReturns/RefAccessors.cs
    │   ├── StringInterpolation/ZeroAllocFormatter.cs
    │   ├── FrozenCollections/ImmutableLookup.cs
    │   ├── RefFields/RefStructContainers.cs
    │   └── Benchmarks/
    │       ├── SpanBenchmarks.cs
    │       ├── MemoryPoolBenchmarks.cs
    │       ├── StackallocBenchmarks.cs
    │       ├── ArrayPoolBenchmarks.cs
    │       ├── StringHandlerBenchmarks.cs
    │       └── FrozenCollectionBenchmarks.cs
    ├── Theme7_LowLevel/
    │   ├── _ThemeIntro.cs
    │   ├── UnsafeCode/PointerArithmetic.cs
    │   ├── UnsafeClass/UnsafeUtilities.cs
    │   ├── PInvoke/NativeInterop.cs
    │   ├── StructLayout/BinaryProtocol.cs
    │   └── FunctionPointers/HighPerfCallbacks.cs
    └── Theme10_Capstone/
        ├── ZeroAllocPipeline.cs
        └── NativeInteropWrapper.cs
```

## Key Concepts

- **Span\<T\>**: Zero-copy views over contiguous memory — arrays, stackalloc, native buffers.
- **stackalloc**: Stack-allocated buffers for small temporary data — no GC pressure.
- **ArrayPool\<T\>**: Rent/return pattern for large buffers — amortized allocation cost.
- **ref Returns**: Return references to struct elements — modify in-place, avoid copies.
- **MemoryMarshal**: Zero-copy serialization of structs to/from byte spans.
- **Function Pointers**: `delegate*` for zero-allocation callbacks in hot paths.

## Exercises

1. Build a zero-alloc CSV parser using `ReadOnlySpan<char>` and `stackalloc`
2. Implement a ring buffer using `ArrayPool<T>` with Span-based read/write
3. Create a pixel convolution filter operating entirely on `Span<Pixel>`
4. Write a binary file format reader using `MemoryMarshal.Read<T>`
5. Compare `Func<T>` vs `delegate*` performance with BenchmarkDotNet

## Requirements

- .NET 10 SDK (Preview)
- BenchmarkDotNet (for `--bench` mode)
