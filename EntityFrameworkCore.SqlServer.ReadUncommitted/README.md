# EntityFrameworkCore.SqlServer.ReadUncommitted (SQL Server–focused)

Small EF Core helper extensions to execute queries under the ReadUncommitted isolation level (commonly associated with the NOLOCK hint in SQL Server). The goal is to reduce blocking for read-heavy, high‑concurrency scenarios.

Important
- This library is focused on SQL Server (Microsoft.EntityFrameworkCore.SqlServer).
- It does not inject NOLOCK table hints into SQL. Instead, it uses a short‑lived transaction configured for ReadUncommitted semantics.
- ReadUncommitted/NOLOCK can introduce anomalies (dirty/non-repeatable/phantom reads). Use only where you can tolerate them.

## Requirements

- .NET 9 (net9.0)
- EF Core (recommended with Microsoft.EntityFrameworkCore.SqlServer provider)

## Install

```bash
dotnet add package EntityFrameworkCore.SqlServer.ReadUncommitted
```

## Quick start

```csharp
using EntityFrameworkCore.SqlServer.ReadUncommitted; // extension methods
// using Microsoft.EntityFrameworkCore;

var ct = cancellationToken; // example CancellationToken

// 1) ToList
var users = await dbContext.Users
    .ToListWithNoLockAsync(dbContext, cancellationToken: ct);

// 2) FirstOrDefault with an optional filter
var user = await dbContext.Users
    .FirstOrDefaultWithNoLockAsync(dbContext, u => u.Id == id, cancellationToken: ct);

// 3) Count
var count = await dbContext.Users
    .CountWithNoLockAsync(dbContext, cancellationToken: ct);

// 4) ToDictionary (key selector)
var map = await dbContext.Users
    .ToDictionaryWithNoLockAsync(dbContext, u => u.Id, cancellationToken: ct);
```

### Options and cancellation

You can observe exceptions and keep cancellation flowing end‑to‑end via options:

```csharp
var options = new ReadUncommittedOptions
{
    // Observe/log exceptions without changing the default throw behavior
    OnException = ex => logger.LogError(ex, "WithNoLock query failed")
};

var users = await dbContext.Users
    .ToListWithNoLockAsync(dbContext, options: options, cancellationToken: ct);
```

Notes
- All methods propagate CancellationToken through EF Core calls.
- If `OnException` is provided, it will be invoked when an exception is thrown. By default, the exception is still propagated (re‑thrown).

## How it works

- The library executes your query under ReadUncommitted isolation using a short‑lived transaction.
- It does not rewrite SQL or add table hints.
- Behavior is designed and tested for SQL Server. Other providers may interpret ReadUncommitted differently (or treat it as ReadCommitted).

## Ambient transaction behavior

- If there is an existing ambient transaction (e.g., an outer TransactionScope), the internal TransactionScope created with `TransactionScopeOption.Required` may join it.
- If the ambient transaction uses a higher isolation level, the effective isolation of your query may not be lowered to ReadUncommitted.
- Recommendation: If you need guaranteed ReadUncommitted semantics, call these helpers on code paths that are not already inside another transaction.

Future enhancements may expose more explicit ambient behaviors (e.g., Respect/Suppress/RequiresNew) via options.

## SQL Server specifics

- On SQL Server, ReadUncommitted corresponds to the semantics commonly associated with NOLOCK. This library does not add table hints; it relies on transaction isolation level instead.
- Under heavy update/read workloads, anomalies (dirty reads, phantoms) are expected. Use judiciously.

## Limitations and risks

- No correctness guarantees: you may see dirty reads, non‑repeatable reads, phantom rows, and in rare cases missing/duplicated rows.
- Intended for read operations only; do not use alongside writes.
- Blocking reduction is the goal, but it is not guaranteed in every scenario (especially if you’re already inside an ambient transaction or constrained by other database settings).

## Troubleshooting

- “Reads are still blocking”: You might be inside an ambient transaction, or the plan/locks/escalation patterns still cause waits. Try calling from a path without an outer transaction.
- “Results look inconsistent”: This is an inherent risk of ReadUncommitted. If you need strong consistency, avoid this approach.
- “Can I use AsNoTracking?”: Yes. Combining AsNoTracking with these helpers is common for read‑only scenarios.

## Roadmap

- More configurable ambient transaction behavior (Respect/Suppress/RequiresNew).
- Clear no‑op strategy and warnings for non‑SQL Server providers if support is expanded.

## License

See the LICENSE file at the repository root.