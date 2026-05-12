# QuantLite

A simplified financial calculator and trade modeler demonstrating foundational C# features, data modeling evolution, and pattern matching techniques. Part of the **Keeping Up with C#** book companion projects.

## Prerequisites

- [.NET 10 SDK](https://dot.net/download) (Preview or RC)

## How to Run

```bash
cd QuantLite/src/QuantLite
dotnet run
```

For non-interactive mode (runs all demos sequentially):

```bash
dotnet run -- --all
```

## Themes Covered

| Theme | Name | Description |
|-------|------|-------------|
| **0** | The Foundation (C# 2–4) | Generics, LINQ, lambdas, extension methods, and other bedrock features |
| **2** | Data Modeling Revolution (C# 7–11) | Tuples, records, init-only, required members, with-expressions |
| **3** | Pattern Matching (C# 7–11) | Type patterns, property patterns, switch expressions, list patterns |
| **10** | Capstone | Immutable Data Pattern combining features across themes |

## Features Demonstrated

| Feature | C# Version | File |
|---------|-----------|------|
| Generics | C# 2.0 | `Theme0_Foundation/Generics/GenericRepository.cs` |
| Nullable Value Types | C# 2.0 | `Theme0_Foundation/NullableValueTypes/OptionalPrice.cs` |
| Iterators (yield) | C# 2.0 | `Theme0_Foundation/Iterators/TradeIterator.cs` |
| LINQ | C# 3.0 | `Theme0_Foundation/Linq/TradeQueries.cs` |
| Extension Methods | C# 3.0 | `Theme0_Foundation/ExtensionMethods/TradeExtensions.cs` |
| Lambda Expressions | C# 3.0 | `Theme0_Foundation/LambdaExpressions/TradePredicate.cs` |
| Expression Trees | C# 3.0 | `Theme0_Foundation/ExpressionTrees/DynamicFilter.cs` |
| Anonymous Types | C# 3.0 | `Theme0_Foundation/AnonymousTypes/TradeProjection.cs` |
| var & Initializers | C# 3.0 | `Theme0_Foundation/VarAndInitializers/TradeCreation.cs` |
| Auto-Properties | C# 3.0+ | `Theme0_Foundation/AutoProperties/SimpleModels.cs` |
| Dynamic Binding | C# 4.0 | `Theme0_Foundation/DynamicBinding/DynamicDispatch.cs` |
| Named & Optional Params | C# 4.0 | `Theme0_Foundation/NamedOptionalParams/OrderBuilder.cs` |
| Generic Variance | C# 4.0 | `Theme0_Foundation/Variance/CovarianceDemo.cs` |
| Tuples (ValueTuple) | C# 7.0 | `Theme2_DataModeling/Tuples/TradeAnalysis.cs` |
| Discards | C# 7.0 | `Theme2_DataModeling/Discards/SelectiveDecon.cs` |
| Records & Record Structs | C# 9/10 | `Theme2_DataModeling/Records/TradeRecordDemo.cs` |
| Init-Only Properties | C# 9.0 | `Theme2_DataModeling/InitOnly/ImmutableConfig.cs` |
| Required Members | C# 11 | `Theme2_DataModeling/RequiredMembers/ValidatedOrder.cs` |
| With-Expressions | C# 9.0 | `Theme2_DataModeling/WithExpressions/TradeAmendment.cs` |
| Readonly Members | C# 8.0 | `Theme2_DataModeling/ReadonlyMembers/MoneyStruct.cs` |
| Basic Type Patterns | C# 7.0 | `Theme3_PatternMatching/BasicPatterns/TypeChecking.cs` |
| Property Patterns | C# 8.0/10 | `Theme3_PatternMatching/PropertyPatterns/TradeValidator.cs` |
| Switch Expressions | C# 8.0 | `Theme3_PatternMatching/SwitchExpressions/RiskClassifier.cs` |
| Relational & Logical | C# 9.0 | `Theme3_PatternMatching/RelationalLogical/FeeCalculator.cs` |
| List Patterns | C# 11 | `Theme3_PatternMatching/ListPatterns/SequenceAnalysis.cs` |
| **Immutable Data Pattern** | Capstone | `Theme10_Capstone/ImmutableDataPattern.cs` |

## Folder Structure

```
QuantLite/
├── QuantLite.sln
├── src/
│   └── QuantLite/
│       ├── QuantLite.csproj
│       ├── GlobalUsings.cs
│       ├── Program.cs
│       ├── Theme0_Foundation/
│       │   ├── _ThemeIntro.cs
│       │   ├── Generics/
│       │   ├── NullableValueTypes/
│       │   ├── Iterators/
│       │   ├── Linq/
│       │   ├── ExtensionMethods/
│       │   ├── LambdaExpressions/
│       │   ├── ExpressionTrees/
│       │   ├── AnonymousTypes/
│       │   ├── VarAndInitializers/
│       │   ├── AutoProperties/
│       │   ├── DynamicBinding/
│       │   ├── NamedOptionalParams/
│       │   └── Variance/
│       ├── Theme2_DataModeling/
│       │   ├── _ThemeIntro.cs
│       │   ├── Tuples/
│       │   ├── Discards/
│       │   ├── Records/
│       │   ├── InitOnly/
│       │   ├── RequiredMembers/
│       │   ├── WithExpressions/
│       │   └── ReadonlyMembers/
│       ├── Theme3_PatternMatching/
│       │   ├── _ThemeIntro.cs
│       │   ├── BasicPatterns/
│       │   ├── PropertyPatterns/
│       │   ├── SwitchExpressions/
│       │   ├── RelationalLogical/
│       │   └── ListPatterns/
│       ├── Theme10_Capstone/
│       │   └── ImmutableDataPattern.cs
│       └── Models/
│           ├── Currency.cs
│           ├── Money.cs
│           ├── TradeType.cs
│           ├── TradeRecord.cs
│           ├── Portfolio.cs
│           ├── TradingSession.cs
│           ├── RiskLevel.cs
│           └── MarketData.cs
├── README.md
└── .editorconfig
```

## Try It Exercises

Each feature file includes a `TODO` exercise at the end. Here are some highlights:

1. **Generics**: Create a `PortfolioRepository<T>` with name-based lookup
2. **LINQ**: Find the ticker with highest total notional across Spot trades
3. **Expression Trees**: Build an OR combinator for dynamic filters
4. **Records**: Create an abstract record hierarchy with polymorphic equality
5. **List Patterns**: Detect candlestick patterns (Spike, Dip, Three White Soldiers)
6. **Switch Expressions**: Add a `when` guard for JPY currency risk override
7. **Immutable Data Pattern**: Add optimistic concurrency using the Version field

## Domain Model

The project models a simplified financial trading system:

- **Currency** — ISO currencies with conversion rates
- **Money** — Amount + Currency with arithmetic operators
- **TradeRecord** — Central domain type (immutable record)
- **Portfolio** — Collection of trades with aggregation
- **TradingSession** — Stateful session managing trades
- **MarketData** — Market snapshots for tickers
