using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFramework.WithNoLock;

/// <summary>
/// Provides extension methods for executing Entity Framework Core queries with NOLOCK (ReadUncommitted) isolation level.
/// </summary>
public static class WithNoLockExtensions
{
    /// <summary>
    /// Executes the query as a list asynchronously with NOLOCK (ReadUncommitted) isolation level.
    /// </summary>
    public static Task<List<T>> ToListWithNoLockAsync<T>(
        this IQueryable<T> query,
        DbContext dbContext,
        Expression<Func<T, bool>>? expression = null,
        CancellationToken cancellationToken = default)
    {
        ValidateDbObjects(query, dbContext);
        return query.WithNoLockAsync(
            dbContext,
            static (q, ct) => q.ToListAsync(ct),
            expression,
            cancellationToken);
    }

    /// <summary>
    /// Executes the query and returns the first element or a default value asynchronously with NOLOCK (ReadUncommitted) isolation level.
    /// </summary>
    public static Task<T?> FirstOrDefaultWithNoLockAsync<T>(
        this IQueryable<T> query,
        DbContext dbContext,
        Expression<Func<T, bool>>? expression = null,
        CancellationToken cancellationToken = default)
    {
        ValidateDbObjects(query, dbContext);
        return query.WithNoLockAsync(
            dbContext,
            static (q, ct) => q.FirstOrDefaultAsync(ct),
            expression,
            cancellationToken);
    }

    /// <summary>
    /// Executes the query and returns the count of elements asynchronously with NOLOCK (ReadUncommitted) isolation level.
    /// </summary>
    public static Task<int> CountWithNoLockAsync<T>(
        this IQueryable<T> query,
        DbContext dbContext,
        Expression<Func<T, bool>>? expression = null,
        CancellationToken cancellationToken = default)
    {
        ValidateDbObjects(query, dbContext);
        return query.WithNoLockAsync(
            dbContext,
            static (q, ct) => q.CountAsync(ct),
            expression,
            cancellationToken);
    }

    /// <summary>
    /// Converts the query to a dictionary asynchronously with NOLOCK (ReadUncommitted) isolation level.
    /// </summary>
    public static Task<Dictionary<TKey, T>> ToDictionaryWithNoLockAsync<TKey, T>(
        this IQueryable<T> query,
        DbContext dbContext,
        Func<T, TKey> keySelector,
        CancellationToken cancellationToken = default)
        where TKey : notnull
    {
        ValidateDbObjects(query, dbContext);
        ArgumentNullException.ThrowIfNull(keySelector);

        return query.WithNoLockAsync(
            dbContext,
            (q, ct) => q.ToDictionaryAsync(keySelector, ct),
            expression: null,
            cancellationToken);
    }

    /// <summary>
    /// Converts the query to a dictionary asynchronously with NOLOCK (ReadUncommitted) isolation level, allowing a value selector and optional comparer.
    /// </summary>
    public static Task<Dictionary<TKey, TValue>> ToDictionaryWithNoLockAsync<TKey, T, TValue>(
        this IQueryable<T> query,
        DbContext dbContext,
        Func<T, TKey> keySelector,
        Func<T, TValue> valueSelector,
        IEqualityComparer<TKey>? comparer = null,
        CancellationToken cancellationToken = default)
        where TKey : notnull
    {
        ValidateDbObjects(query, dbContext);
        ArgumentNullException.ThrowIfNull(keySelector);
        ArgumentNullException.ThrowIfNull(valueSelector);

        return query.WithNoLockAsync(
            dbContext,
            (q, ct) => comparer is null
                ? q.ToDictionaryAsync(keySelector, valueSelector, ct)
                : q.ToDictionaryAsync(keySelector, valueSelector, comparer, ct),
            expression: null,
            cancellationToken);
    }

    /// <summary>
    /// Core helper that executes the provided query function under a ReadUncommitted transaction when safe.
    /// If there's an existing ambient/EF transaction, it will execute without attempting to override the isolation level.
    /// </summary>
    private static async Task<TResult> WithNoLockAsync<T, TResult>(
        this IQueryable<T> query,
        DbContext dbContext,
        Func<IQueryable<T>, CancellationToken, Task<TResult>> execute,
        Expression<Func<T, bool>>? expression,
        CancellationToken cancellationToken)
    {
        ValidateDbObjects(query, dbContext);
        ArgumentNullException.ThrowIfNull(execute);

        if (expression is not null)
        {
            query = query.Where(expression);
        }

        var strategy = dbContext.Database.CreateExecutionStrategy();

        // If there is already an ambient/EF transaction, do not try to override its isolation level.
        if (dbContext.Database.CurrentTransaction is not null || System.Transactions.Transaction.Current is not null)
        {
            return await strategy.ExecuteAsync(
                async ct => await execute(query, ct).ConfigureAwait(false),
                cancellationToken).ConfigureAwait(false);
        }

        return await strategy.ExecuteAsync(
            async ct =>
            {
                await using var tx = await dbContext.Database.BeginTransactionAsync(ct).ConfigureAwait(false);
                // Set isolation level to ReadUncommitted for this transaction
                await dbContext.Database.ExecuteSqlRawAsync("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED", ct).ConfigureAwait(false);
                var result = await execute(query, ct).ConfigureAwait(false);
                await tx.CommitAsync(ct).ConfigureAwait(false);
                return result;
            },
            cancellationToken).ConfigureAwait(false);
    }

    private static void ValidateDbObjects<T>(IQueryable<T> query, DbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(dbContext);
    }
}