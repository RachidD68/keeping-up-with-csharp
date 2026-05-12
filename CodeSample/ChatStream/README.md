# ChatStream

**Keeping Up with C# — Companion Project 3**

A real-time chat messaging library exploring **safety, null-handling, async programming, concurrency, and streaming** — Themes 5, 8, and 10 of the book.

## Quick Start

```bash
cd src/ChatStream
dotnet run              # Interactive menu
dotnet run -- --all     # Run all demos sequentially
```

## Themes & Features

### Theme 5 — Safety & Robustness (8 features)

| # | Feature | File | C# Version |
|---|---------|------|------------|
| 1 | Nullable Reference Types | `NullableRefTypes/SafeMessageApi.cs` | C# 8 |
| 2 | Exception Filters | `ExceptionFilters/SmartErrorHandler.cs` | C# 6 |
| 3 | Null-Conditional Operators | `NullConditional/OptionalChaining.cs` | C# 6 |
| 4 | Null-Coalescing Assignment | `NullCoalescingAssign/DefaultValues.cs` | C# 8 |
| 5 | Null-Conditional Assignment | `NullConditionalAssign/ConditionalUpdate.cs` | C# 14 |
| 6 | Checked User-Defined Operators | `CheckedOperators/SafeCounter.cs` | C# 11 |
| 7 | CallerArgumentExpression | `CallerArgExpression/Guard.cs` | C# 10 |
| 8 | System.Threading.Lock | `NewLockObject/ThreadSafeRoom.cs` | C# 13 |

### Theme 8 — Async, Concurrency & Streaming (9 features)

| # | Feature | File | C# Version |
|---|---------|------|------------|
| 1 | async / await | `AsyncAwait/BasicMessaging.cs` | C# 5 |
| 2 | Async State Machine | `AsyncStateMachine/StateMachineExplorer.cs` | C# 5 |
| 3 | ValueTask\<T\> | `ValueTask/CachedLookup.cs` | C# 7 |
| 4 | IAsyncEnumerable\<T\> | `AsyncStreams/MessageStream.cs` | C# 8 |
| 5 | IAsyncDisposable | `AsyncDisposable/ConnectionManager.cs` | C# 8 |
| 6 | ConfigureAwait | `ConfigureAwait/ContextExplorer.cs` | C# 5+ |
| 7 | Cancellation & Shutdown | `Cancellation/GracefulShutdown.cs` | C# 5+ |
| 8 | Task Combinators | `TaskCombinators/ParallelFetch.cs` | C# 5 / .NET 9 |
| 9 | System.Threading.Channels | `Channels/MessageBus.cs` | .NET Core 3.0+ |

### Theme 10 — Capstone Patterns (2 patterns)

| Pattern | File | Features Combined |
|---------|------|-------------------|
| Null-Safe API | `Theme10_Capstone/NullSafeApiPattern.cs` | Nullable refs, ?., ??=, Guards, exception filters, pattern matching |
| Async Pipeline | `Theme10_Capstone/AsyncPipelinePattern.cs` | async/await, ValueTask, IAsyncEnumerable, Channels, WhenAll, cancellation |

## Domain Model

```
ChatMessage    — immutable message record (Id, Sender, Content, Channel, Timestamp)
User           — user record with status and join time
ChatChannel    — Channel<ChatMessage> wrapper with bounded backpressure
ChatRoom       — manages users, history, and async streaming
ConnectionInfo — connection settings record with defaults
MessageFilter  — null-safe multi-criteria filter
```

## Folder Structure

```
ChatStream/
├── ChatStream.sln
├── .editorconfig
├── README.md
└── src/ChatStream/
    ├── ChatStream.csproj
    ├── GlobalUsings.cs
    ├── Program.cs
    ├── Models/
    │   ├── ChatMessage.cs
    │   ├── User.cs
    │   ├── ChatChannel.cs
    │   ├── ChatRoom.cs
    │   ├── ConnectionInfo.cs
    │   └── MessageFilter.cs
    ├── Theme5_Safety/
    │   ├── _ThemeIntro.cs
    │   ├── NullableRefTypes/SafeMessageApi.cs
    │   ├── ExceptionFilters/SmartErrorHandler.cs
    │   ├── NullConditional/OptionalChaining.cs
    │   ├── NullCoalescingAssign/DefaultValues.cs
    │   ├── NullConditionalAssign/ConditionalUpdate.cs
    │   ├── CheckedOperators/SafeCounter.cs
    │   ├── CallerArgExpression/Guard.cs
    │   └── NewLockObject/ThreadSafeRoom.cs
    ├── Theme8_Async/
    │   ├── _ThemeIntro.cs
    │   ├── AsyncAwait/BasicMessaging.cs
    │   ├── AsyncStateMachine/StateMachineExplorer.cs
    │   ├── ValueTask/CachedLookup.cs
    │   ├── AsyncStreams/MessageStream.cs
    │   ├── AsyncDisposable/ConnectionManager.cs
    │   ├── ConfigureAwait/ContextExplorer.cs
    │   ├── Cancellation/GracefulShutdown.cs
    │   ├── TaskCombinators/ParallelFetch.cs
    │   └── Channels/MessageBus.cs
    └── Theme10_Capstone/
        ├── NullSafeApiPattern.cs
        └── AsyncPipelinePattern.cs
```

## Key Concepts

- **Null Safety**: Every API boundary has explicit nullability annotations. The `Guard` class uses `CallerArgumentExpression` for rich error messages.
- **Async Pipelines**: Messages flow through ingestion → validation → transformation → delivery stages connected by `Channel<T>`.
- **Backpressure**: Bounded channels prevent fast producers from overwhelming slow consumers.
- **Graceful Shutdown**: `CancellationToken` propagation and `CreateLinkedTokenSource` enable clean shutdown with timeout.
- **Thread Safety**: `System.Threading.Lock` (C# 13) replaces `lock(object)` for type-safe locking.

## Exercises

1. Add a `MessagePriority` enum and filter high-priority messages in the pipeline
2. Implement a rate limiter using `Channel<T>` with `BoundedChannelFullMode.DropNewest`
3. Add retry logic with exponential backoff to the delivery stage
4. Create a `Guard.Against.Duplicate()` that checks message IDs
5. Build a multi-room chat server using one channel per room

## Requirements

- .NET 10 SDK (Preview)
- No external NuGet packages required
