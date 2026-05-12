# TypeForge

**Keeping Up with C# — Companion Project 4**

A type-system playground exploring **interfaces, generics, abstract classes, operators, and modern OOP patterns** — Theme 4 and Theme 10 of the book.

## Quick Start

```bash
cd src/TypeForge
dotnet run              # Interactive menu
dotnet run -- --all     # Run all demos sequentially
```

## Themes & Features

### Theme 4 — Type System & OOP Flexibility (11 features)

| # | Feature | File | C# Version |
|---|---------|------|------------|
| 1 | Default Interface Methods | `DefaultInterfaceMethods/PluginDefaults.cs` | C# 8 |
| 2 | Static Abstract / Virtual Members | `StaticAbstracts/ShapeFactory.cs` | C# 11 |
| 3 | Generic Math (INumber\<T\>) | `GenericMath/DimensionCalculator.cs` | C# 11 |
| 4 | Interface Hierarchies & Composition | `InterfaceHierarchies/TypeHierarchy.cs` | C# 8+ |
| 5 | Sealed Type Hierarchies | `SealedHierarchies/TypeNodeWalker.cs` | C# 9+ |
| 6 | File-Scoped Types | `FileScopedTypes/InternalHelpers.cs` | C# 11 |
| 7 | ref struct Interfaces | `RefStructInterfaces/StackOnlyParser.cs` | C# 13 |
| 8 | Inline Arrays | `InlineArrays/FixedBufferPlugin.cs` | C# 12 |
| 9 | Extension Members | `ExtensionMembers/ShapeExtensions.cs` | C# 14 |
| 10 | Abstract Classes & Virtual Dispatch | `AbstractClasses/TypeVisitor.cs` | Classic + C# 12 |
| 11 | Operator Overloading & Conversions | `OperatorOverloading/UnitArithmetic.cs` | Classic + C# 11 |
| 12 | Generic Constraints Showcase | `GenericConstraints/ConstraintShowcase.cs` | C# 2–13 |

### Theme 10 — Capstone Pattern (1 pattern)

| Pattern | File | Features Combined |
|---------|------|-------------------|
| Extensible Plugin Architecture | `Theme10_Capstone/ExtensiblePluginPattern.cs` | Default methods, static abstracts, interface composition, generic constraints, pattern matching, file-scoped types, records |

## Domain Model

```
IShape          — Shape interface with static abstract ShapeName + Area/Perimeter
Circle          — readonly record struct implementing IShape
Rectangle       — readonly record struct implementing IShape
Triangle        — readonly record struct implementing IShape (Heron's formula)

IPlugin         — Plugin base interface with default Describe() method
IPluginFactory  — Static abstract factory pattern (Create(), PluginId)
PluginInfo      — Plugin metadata record

TypeNode        — Abstract record hierarchy (6 sealed variants)
├── PrimitiveType   (int, string, bool)
├── ArrayType       (wraps element type)
├── NullableType    (wraps inner type)
├── GenericType     (name + type arguments)
├── TupleType       (named element list)
└── FunctionType    (parameters + return type)

Dimension<T>    — Generic numeric dimension with INumber<T> constraint
IUnit<TSelf>    — Unit conversion with static abstract ToBaseFactor
ValidationRule  — Composable validation with IValidationRule<T>
```

## Folder Structure

```
TypeForge/
├── TypeForge.sln
├── .editorconfig
├── README.md
└── src/TypeForge/
    ├── TypeForge.csproj
    ├── GlobalUsings.cs
    ├── Program.cs
    ├── Models/
    │   ├── Shape.cs
    │   ├── Plugin.cs
    │   ├── TypeNode.cs
    │   ├── Dimension.cs
    │   ├── Unit.cs
    │   └── ValidationRule.cs
    ├── Theme4_TypeSystem/
    │   ├── _ThemeIntro.cs
    │   ├── DefaultInterfaceMethods/PluginDefaults.cs
    │   ├── StaticAbstracts/ShapeFactory.cs
    │   ├── GenericMath/DimensionCalculator.cs
    │   ├── InterfaceHierarchies/TypeHierarchy.cs
    │   ├── SealedHierarchies/TypeNodeWalker.cs
    │   ├── FileScopedTypes/InternalHelpers.cs
    │   ├── RefStructInterfaces/StackOnlyParser.cs
    │   ├── InlineArrays/FixedBufferPlugin.cs
    │   ├── ExtensionMembers/ShapeExtensions.cs
    │   ├── AbstractClasses/TypeVisitor.cs
    │   ├── OperatorOverloading/UnitArithmetic.cs
    │   └── GenericConstraints/ConstraintShowcase.cs
    └── Theme10_Capstone/
        └── ExtensiblePluginPattern.cs
```

## Key Concepts

- **Static Abstract Members**: Interface contracts at the type level — factory methods, metadata, operators.
- **Generic Math**: `INumber<T>` enables one generic method for all numeric types (int, double, decimal, etc.).
- **Sealed Hierarchies**: TypeNode's 6 sealed variants enable exhaustive switch expression matching.
- **Interface Composition**: Small interfaces (IRenderable, IValidatable, ISerializable) compose into ITypeModel.
- **Default Methods**: Libraries can evolve interfaces without breaking implementations.
- **File-Scoped Types**: `file class` keeps helpers invisible outside their file.

## Exercises

1. Add a `UnionType` variant to TypeNode and fix all switch warnings
2. Create a `RatioType<T>` that implements `INumber<T>` for rational arithmetic
3. Build a plugin that transforms TypeNode trees (e.g., removing all nullable wrappers)
4. Implement a `TypeDiff` visitor that compares two TypeNode trees
5. Add `IAsyncPlugin` with an `ExecuteAsync` method using default interface methods

## Requirements

- .NET 10 SDK (Preview)
- No external NuGet packages required
