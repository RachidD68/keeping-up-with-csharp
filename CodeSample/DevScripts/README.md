# DevScripts

A developer toolbox of scripts and utilities demonstrating C#'s expressiveness features and compiler tooling innovations. Part of the **Keeping Up with C#** book companion projects.

## Prerequisites

- [.NET 10 SDK](https://dot.net/download) (Preview or RC)

## How to Run

```bash
cd DevScripts/src/DevScripts
dotnet run
```

For non-interactive mode (runs all demos sequentially):

```bash
dotnet run -- --all
```

To run the file-based app example (C# 14):

```bash
dotnet run scripts/hello.cs
```

## Themes Covered

| Theme | Name | Description |
|-------|------|-------------|
| **1** | Expressiveness & Boilerplate Reduction (C# 6–14) | String interpolation, primary constructors, collection expressions, and more |
| **9** | Compiler & Tooling (C# 5–14) | Source generators, caller info, module initializers, interceptors |
| **10** | Capstone | Scripting Pattern and Source Generator Pattern |

## Features Demonstrated

| Feature | C# Version | File |
|---------|-----------|------|
| String Interpolation Evolution | C# 6→11 | `Theme1_Expressiveness/StringInterpolation/TemplateEngine.cs` |
| Expression-Bodied Members | C# 6/7 | `Theme1_Expressiveness/ExpressionBodied/MetricsCalculator.cs` |
| Local Functions | C# 7.0/8.0 | `Theme1_Expressiveness/LocalFunctions/RecursiveParser.cs` |
| Top-Level / File-Based Apps | C# 9/14 | `Theme1_Expressiveness/TopLevelAndFileBased/ScriptingDemo.cs` |
| File-Scoped Namespaces | C# 10 | `Theme1_Expressiveness/FileScoped/NamespaceDemo.cs` |
| Global & Implicit Usings | C# 10 | `Theme1_Expressiveness/GlobalUsings/UsingsDemo.cs` |
| Target-Typed new | C# 9.0 | `Theme1_Expressiveness/TargetTypedNew/CollectionFactory.cs` |
| Digit Separators & Binary | C# 7.0 | `Theme1_Expressiveness/NumericLiterals/BitMaskBuilder.cs` |
| Default Literal | C# 7.1 | `Theme1_Expressiveness/DefaultLiterals/GenericDefaults.cs` |
| Const Interpolated Strings | C# 10 | `Theme1_Expressiveness/ConstInterpolation/ConstantStrings.cs` |
| Primary Constructors | C# 12 | `Theme1_Expressiveness/PrimaryConstructors/ServiceConfig.cs` |
| Collection Expressions | C# 12 | `Theme1_Expressiveness/CollectionExpressions/DataPipeline.cs` |
| params Enhancements | C# 13 | `Theme1_Expressiveness/ParamsEnhancements/FlexibleApi.cs` |
| The field Keyword | C# 14 | `Theme1_Expressiveness/FieldKeyword/SmartProperty.cs` |
| Source Generators | C# 9.0 | `Theme9_CompilerTooling/SourceGenerators/GeneratorConsumer.cs` |
| Caller Info Attributes | C# 5.0 | `Theme9_CompilerTooling/CallerInfo/DiagnosticLogger.cs` |
| Module Initializers | C# 9.0 | `Theme9_CompilerTooling/ModuleInitializers/StartupHook.cs` |
| Interceptors | C# 12 | `Theme9_CompilerTooling/Interceptors/InterceptorDemo.cs` |
| Partial Members | C# 13/14 | `Theme9_CompilerTooling/PartialMembers/GeneratedEntity.cs` |
| **Scripting Pattern** | Capstone | `Theme10_Capstone/ScriptingPattern.cs` |
| **Source Generator Pattern** | Capstone | `Theme10_Capstone/SourceGeneratorPattern.cs` |

## Folder Structure

```
DevScripts/
├── DevScripts.sln
├── src/
│   ├── DevScripts/
│   │   ├── DevScripts.csproj
│   │   ├── GlobalUsings.cs
│   │   ├── Program.cs
│   │   ├── Theme1_Expressiveness/
│   │   │   ├── _ThemeIntro.cs
│   │   │   ├── StringInterpolation/
│   │   │   ├── ExpressionBodied/
│   │   │   ├── LocalFunctions/
│   │   │   ├── TopLevelAndFileBased/
│   │   │   ├── FileScoped/
│   │   │   ├── GlobalUsings/
│   │   │   ├── TargetTypedNew/
│   │   │   ├── NumericLiterals/
│   │   │   ├── DefaultLiterals/
│   │   │   ├── ConstInterpolation/
│   │   │   ├── PrimaryConstructors/
│   │   │   ├── CollectionExpressions/
│   │   │   ├── ParamsEnhancements/
│   │   │   └── FieldKeyword/
│   │   ├── Theme9_CompilerTooling/
│   │   │   ├── _ThemeIntro.cs
│   │   │   ├── SourceGenerators/
│   │   │   ├── CallerInfo/
│   │   │   ├── ModuleInitializers/
│   │   │   ├── Interceptors/
│   │   │   └── PartialMembers/
│   │   ├── Theme10_Capstone/
│   │   │   ├── ScriptingPattern.cs
│   │   │   └── SourceGeneratorPattern.cs
│   │   └── Models/
│   │       └── CodeMetrics.cs
│   └── DevScripts.Generators/
│       ├── DevScripts.Generators.csproj
│       └── AutoToStringGenerator.cs
├── scripts/
│   └── hello.cs
├── README.md
└── .editorconfig
```

## Source Generator

The `DevScripts.Generators` project contains an incremental source generator (`AutoToStringGenerator`) that automatically generates `ToString()` methods for classes decorated with `[AutoToString]`. See the generator source and the consumer demo for the complete pattern.

## Try It Exercises

1. **Raw String Literals**: Create a SQL template with real brackets and interpolation
2. **Local Functions**: Write a recursive HTML tag depth counter
3. **Primary Constructors**: Convert a DI service class to use primary constructors
4. **Collection Expressions**: Build a pipeline using spread operators
5. **Source Generators**: Add a new `[AutoToString]` class and check the generated output
6. **File-Based Apps**: Run `scripts/hello.cs` directly with `dotnet run`
