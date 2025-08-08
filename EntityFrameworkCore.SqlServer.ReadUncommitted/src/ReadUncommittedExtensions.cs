using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.SqlServer.ReadUncommitted;

/// <summary>
/// Provides extension methods for executing Entity Framework Core queries with NOLOCK (ReadUncommitted) isolation level.
/// </summary>
public static class ReadUncommittedExtensions
{
    /// <summary>
    /// Asynchronously converts the queryable sequence to a list while applying the NOLOCK query hint.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the query.</typeparam>
    /// <param name="query">The queryable sequence to execute.</param>
    /// <param name="dbContext">The database context associated with the query.</param>
    /// <param name="expression">An optional filter expression to apply to the query.</param>
    /// <param name="options">Optional configuration for the ReadUncommitted transaction.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the list of elements in the query.
    /// </returns>
    public static Task<List<T>> ToListWithNoLockAsync<T>(this IQueryable<T> query, DbContext dbContext, Expression<Func<T, bool>>? expression = null,
                                                         ReadUncommittedOptions? options = null, CancellationToken cancellationToken = default)
    {
        return query.WithNoLockAsync(dbContext, (q, ct) => q.ToListAsync(ct), expression, options: options, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Converts the queryable sequence to a list while applying the NOLOCK (ReadUncommitted) query hint.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the query.</typeparam>
    /// <param name="query">The queryable sequence to execute.</param>
    /// <param name="dbContext">The database context associated with the query.</param>
    /// <param name="expression">
    /// An optional filter expression to apply to the query. If null, no additional filtering is applied.
    /// </param>
    /// <param name="options">
    /// Optional configuration for the ReadUncommitted transaction, such as exception handling or transaction timeout.
    /// </param>
    /// <returns>The list of elements in the query.</returns>
    public static List<T> ToListWithNoLock<T>(this IQueryable<T> query, DbContext dbContext, Expression<Func<T, bool>>? expression = null,
                                              ReadUncommittedOptions? options = null)
    {
        return query.WithNoLock(dbContext, q => q.ToList(), expression, options: options);
    }


    /// <summary>
    /// Asynchronously returns the first element of the queryable sequence that satisfies a specified condition, 
    /// or a default value if no such element is found, while applying the NOLOCK query hint.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the query.</typeparam>
    /// <param name="query">The queryable sequence to execute.</param>
    /// <param name="dbContext">The database context associated with the query.</param>
    /// <param name="expression">An optional filter expression to apply to the query.</param>
    /// <param name="options">Optional configuration for the ReadUncommitted transaction.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the first element of the sequence 
    /// that satisfies the condition, or <c>null</c> if no such element is found.
    /// </returns>
    public static Task<T?> FirstOrDefaultWithNoLockAsync<T>(this IQueryable<T> query, DbContext dbContext,
                                                            Expression<Func<T, bool>>? expression = null, ReadUncommittedOptions? options = null,
                                                            CancellationToken cancellationToken = default)
    {
        return query.WithNoLockAsync(dbContext, (q, ct) => q.FirstOrDefaultAsync(ct), expression, options: options,
                                     cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Returns the first element of the queryable sequence that satisfies a specified condition,
    /// or a default value if no such element is found, while applying the NOLOCK (ReadUncommitted) query hint.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the query.</typeparam>
    /// <param name="query">The queryable sequence to execute.</param>
    /// <param name="dbContext">The database context associated with the query.</param>
    /// <param name="expression">
    /// An optional filter expression to apply to the query. If null, no additional filtering is applied.
    /// </param>
    /// <param name="options">
    /// Optional configuration for the ReadUncommitted transaction, such as exception handling or transaction timeout.
    /// </param>
    /// <returns>
    /// The first element of the sequence that satisfies the condition, or <c>null</c> if no such element is found.
    /// </returns>
    public static T? FirstOrDefaultWithNoLock<T>(this IQueryable<T> query, DbContext dbContext, Expression<Func<T, bool>>? expression = null,
                                                 ReadUncommittedOptions? options = null)
    {
        return query.WithNoLock(dbContext, q => q.FirstOrDefault(), expression, options: options);
    }


    /// <summary>
    /// Asynchronously counts the number of elements in the queryable sequence while applying the NOLOCK query hint.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the query.</typeparam>
    /// <param name="query">The queryable sequence to execute.</param>
    /// <param name="dbContext">The database context associated with the query.</param>
    /// <param name="expression">An optional filter expression to apply to the query.</param>
    /// <param name="options">Optional configuration for the ReadUncommitted transaction.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the count of elements in the query.
    /// </returns>
    public static Task<int> CountWithNoLockAsync<T>(this IQueryable<T> query, DbContext dbContext, Expression<Func<T, bool>>? expression = null,
                                                    ReadUncommittedOptions? options = null, CancellationToken cancellationToken = default)
    {
        return query.WithNoLockAsync(dbContext, (q, ct) => q.CountAsync(ct), expression, options: options, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Counts the number of elements in the queryable sequence while applying the NOLOCK (ReadUncommitted) query hint.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the query.</typeparam>
    /// <param name="query">The queryable sequence to execute.</param>
    /// <param name="dbContext">The database context associated with the query.</param>
    /// <param name="expression">
    /// An optional filter expression to apply to the query. If null, no additional filtering is applied.
    /// </param>
    /// <param name="options">
    /// Optional configuration for the ReadUncommitted transaction, such as exception handling or transaction timeout.
    /// </param>
    /// <returns>The count of elements in the query.</returns>
    public static int CountWithNoLock<T>(this IQueryable<T> query, DbContext dbContext, Expression<Func<T, bool>>? expression = null,
                                         ReadUncommittedOptions? options = null)
    {
        return query.WithNoLock(dbContext, q => q.Count(), expression, options: options);
    }

    /// <summary>
    /// Asynchronously converts the queryable sequence to a dictionary while applying the NOLOCK query hint.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the resulting dictionary.</typeparam>
    /// <typeparam name="T">The type of the elements in the query.</typeparam>
    /// <param name="query">The queryable sequence to execute.</param>
    /// <param name="dbContext">The database context associated with the query.</param>
    /// <param name="keySelector">A function to extract a key from each element.</param>
    /// <param name="options">Optional configuration for the ReadUncommitted transaction.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the dictionary of elements.
    /// </returns>
    public static Task<Dictionary<TKey, T>> ToDictionaryWithNoLockAsync<TKey, T>(this IQueryable<T> query, DbContext dbContext,
                                                                                 Func<T, TKey> keySelector, ReadUncommittedOptions? options = null,
                                                                                 CancellationToken cancellationToken = default) where TKey : notnull
    {
        return query.WithNoLockAsync(dbContext, (q, ct) => q.ToDictionaryAsync(keySelector, ct), options: options,
                                     cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Converts the queryable sequence to a dictionary while applying the NOLOCK (ReadUncommitted) query hint.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the resulting dictionary.</typeparam>
    /// <typeparam name="T">The type of the elements in the query.</typeparam>
    /// <param name="query">The queryable sequence to execute.</param>
    /// <param name="dbContext">The database context associated with the query.</param>
    /// <param name="keySelector">A function to extract a key from each element.</param>
    /// <param name="options">
    /// Optional configuration for the ReadUncommitted transaction, such as exception handling or transaction timeout.
    /// </param>
    /// <returns>The dictionary of elements in the query.</returns>
    public static Dictionary<TKey, T> ToDictionaryWithNoLock<TKey, T>(this IQueryable<T> query, DbContext dbContext, Func<T, TKey> keySelector,
                                                                      ReadUncommittedOptions? options = null) where TKey : notnull
    {
        return query.WithNoLock(dbContext, q => q.ToDictionary(keySelector), options: options);
    }

    /// <summary>
    /// Asynchronously converts the queryable sequence to a dictionary while applying the NOLOCK query hint.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the resulting dictionary.</typeparam>
    /// <typeparam name="T">The type of the elements in the query.</typeparam>
    /// <typeparam name="TValue">The type of the values in the resulting dictionary.</typeparam>
    /// <param name="query">The queryable sequence to execute.</param>
    /// <param name="dbContext">The database context associated with the query.</param>
    /// <param name="keySelector">A function to extract a key from each element.</param>
    /// <param name="valueSelector">A function to extract a value from each element.</param>
    /// <param name="comparer">An optional equality comparer to compare keys.</param>
    /// <param name="options">Optional configuration for the ReadUncommitted transaction.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the dictionary of elements.
    /// </returns>
    public static Task<Dictionary<TKey, TValue>> ToDictionaryWithNoLockAsync<TKey, T, TValue>(
        this IQueryable<T> query, DbContext dbContext, Func<T, TKey> keySelector, Func<T, TValue> valueSelector,
        IEqualityComparer<TKey>? comparer = null, ReadUncommittedOptions? options = null, CancellationToken cancellationToken = default)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        ArgumentNullException.ThrowIfNull(valueSelector);

        return query.WithNoLockAsync(
            dbContext,
            (q, ct) => comparer is null
                           ? q.ToDictionaryAsync(keySelector, valueSelector, ct)
                           : q.ToDictionaryAsync(keySelector, valueSelector, comparer, ct), options: options, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Converts the queryable sequence to a dictionary while applying the NOLOCK (ReadUncommitted) query hint.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the resulting dictionary.</typeparam>
    /// <typeparam name="T">The type of the elements in the query.</typeparam>
    /// <typeparam name="TValue">The type of the values in the resulting dictionary.</typeparam>
    /// <param name="query">The queryable sequence to execute.</param>
    /// <param name="dbContext">The database context associated with the query.</param>
    /// <param name="keySelector">A function to extract a key from each element.</param>
    /// <param name="valueSelector">A function to extract a value from each element.</param>
    /// <param name="comparer">An optional equality comparer to compare keys.</param>
    /// <param name="options">
    /// Optional configuration for the ReadUncommitted transaction, such as exception handling or transaction timeout.
    /// </param>
    /// <returns>The dictionary of elements in the query.</returns>
    public static Dictionary<TKey, TValue> ToDictionaryWithNoLock<TKey, T, TValue>(this IQueryable<T> query, DbContext dbContext,
                                                                                   Func<T, TKey> keySelector, Func<T, TValue> valueSelector,
                                                                                   IEqualityComparer<TKey>? comparer = null,
                                                                                   ReadUncommittedOptions? options = null) where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        ArgumentNullException.ThrowIfNull(valueSelector);

        return query.WithNoLock(
            dbContext, q => comparer is null ? q.ToDictionary(keySelector, valueSelector) : q.ToDictionary(keySelector, valueSelector, comparer),
            options: options);
    }

    #region private methods

    /// <summary>
    /// Executes a query asynchronously with the NOLOCK (ReadUncommitted) isolation level.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the query.</typeparam>
    /// <typeparam name="TResult">The type of the result returned by the query execution.</typeparam>
    /// <param name="query">The queryable sequence to execute.</param>
    /// <param name="dbContext">The database context associated with the query.</param>
    /// <param name="func">
    /// The asynchronous function to execute the query, which accepts a queryable sequence and a CancellationToken.
    /// </param>
    /// <param name="expression">
    /// An optional filter expression to apply to the query. If null, no additional filtering is applied.
    /// </param>
    /// <param name="options">
    /// Optional configuration for the ReadUncommitted transaction, such as exception handling or transaction timeout.
    /// </param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the result of the query execution.
    /// </returns>
    private static async Task<TResult> WithNoLockAsync<T, TResult>(this IQueryable<T> query, DbContext dbContext,
                                                                   Func<IQueryable<T>, CancellationToken, Task<TResult>> func,
                                                                   Expression<Func<T, bool>>? expression = null,
                                                                   ReadUncommittedOptions? options = null,
                                                                   CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(func);

        options ??= new ReadUncommittedOptions();


        if (expression is not null)
        {
            query = query.Where(expression);
        }

        var strategy = dbContext.Database.CreateExecutionStrategy();

        try
        {
            return await strategy.ExecuteAsync(async ct =>
            {
                var hasAmbient = dbContext.Database.CurrentTransaction is not null || Transaction.Current is not null;

                if (!hasAmbient)
                {
                    return await ExecuteReadUncommittedAsync(query, dbContext, func, ct).ConfigureAwait(false);
                }

                switch (options.AmbientBehavior)
                {
                    case AmbientTransactionBehavior.Suppress:
                        using (new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
                        {
                            return await ExecuteReadUncommittedAsync(query, dbContext, func, ct).ConfigureAwait(false);
                        }

                    case AmbientTransactionBehavior.RequiresNew:
                        using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew, new TransactionOptions
                               {
                                   IsolationLevel = IsolationLevel.ReadUncommitted,
                                   Timeout = options.TransactionTimeout ?? TransactionManager.DefaultTimeout
                               }, TransactionScopeAsyncFlowOption.Enabled))
                        {
                            var resultNew = await func(query, ct).ConfigureAwait(false);
                            scope.Complete();
                            return resultNew;
                        }

                    default:
                        return await func(query, ct).ConfigureAwait(false);
                }
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException oce)
        {
            options.OnException?.Invoke(oce);

            if (options.SwallowExceptions)
            {
                return default!;
            }

            throw;
        }
        catch (Exception ex)
        {
            options.OnException?.Invoke(ex);

            if (options.SwallowExceptions)
            {
                return default!;
            }

            throw;
        }
    }

    /// <summary>
    /// Executes a query with the NOLOCK (ReadUncommitted) isolation level.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the query.</typeparam>
    /// <typeparam name="TResult">The type of the result returned by the query execution.</typeparam>
    /// <param name="query">The queryable sequence to execute.</param>
    /// <param name="dbContext">The database context associated with the query.</param>
    /// <param name="func">The function to execute the query, which accepts a queryable sequence.</param>
    /// <param name="expression">
    /// An optional filter expression to apply to the query. If null, no additional filtering is applied.
    /// </param>
    /// <param name="options">
    /// Optional configuration for the ReadUncommitted transaction, such as exception handling or transaction timeout.
    /// </param>
    /// <returns>The result of the query execution.</returns>
    private static TResult WithNoLock<T, TResult>(this IQueryable<T> query, DbContext dbContext, Func<IQueryable<T>, TResult> func,
                                                  Expression<Func<T, bool>>? expression = null, ReadUncommittedOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(func);

        options ??= new ReadUncommittedOptions();


        if (expression is not null)
        {
            query = query.Where(expression);
        }

        var strategy = dbContext.Database.CreateExecutionStrategy();

        try
        {
            return strategy.Execute(() =>
            {
                var hasAmbient = dbContext.Database.CurrentTransaction is not null || Transaction.Current is not null;

                if (!hasAmbient)
                {
                    return ExecuteReadUncommitted(query, dbContext, func);
                }

                switch (options.AmbientBehavior)
                {
                    case AmbientTransactionBehavior.Suppress:
                        using (new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
                        {
                            return ExecuteReadUncommitted(query, dbContext, func);
                        }

                    case AmbientTransactionBehavior.RequiresNew:
                        using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew, new TransactionOptions
                               {
                                   IsolationLevel = IsolationLevel.ReadUncommitted,
                                   Timeout = options.TransactionTimeout ?? TransactionManager.DefaultTimeout
                               }, TransactionScopeAsyncFlowOption.Enabled))
                        {
                            var resultNew = func(query);
                            scope.Complete();
                            return resultNew;
                        }

                    default:
                        return func(query);
                }
            });
        }
        catch (OperationCanceledException oce)
        {
            options.OnException?.Invoke(oce);

            if (options.SwallowExceptions)
            {
                return default!;
            }

            throw;
        }
        catch (Exception ex)
        {
            options.OnException?.Invoke(ex);

            if (options.SwallowExceptions)
            {
                return default!;
            }

            throw;
        }
    }

    /// <summary>
    /// Executes a query within a transaction using the ReadUncommitted isolation level asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the query.</typeparam>
    /// <typeparam name="TResult">The type of the result returned by the query execution.</typeparam>
    /// <param name="query">The queryable sequence to execute.</param>
    /// <param name="dbContext">The database context associated with the query.</param>
    /// <param name="func">The asynchronous function to execute the query, which accepts a queryable sequence and a CancellationToken.</param>
    /// <param name="ct">A CancellationToken to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the result of the query execution.
    /// </returns>
    private static async Task<TResult> ExecuteReadUncommittedAsync<T, TResult>(IQueryable<T> query, DbContext dbContext,
                                                                               Func<IQueryable<T>, CancellationToken, Task<TResult>> func,
                                                                               CancellationToken ct)
    {
        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted, ct).ConfigureAwait(false);
        var result = await func(query, ct).ConfigureAwait(false);
        await transaction.CommitAsync(ct).ConfigureAwait(false);
        return result;
    }

    /// <summary>
    /// Executes a query within a transaction using the ReadUncommitted isolation level.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the query.</typeparam>
    /// <typeparam name="TResult">The type of the result returned by the query execution.</typeparam>
    /// <param name="query">The queryable sequence to execute.</param>
    /// <param name="dbContext">The database context associated with the query.</param>
    /// <param name="func">The function to execute the query, which accepts a queryable sequence.</param>
    /// <returns>The result of the query execution.</returns>
    private static TResult ExecuteReadUncommitted<T, TResult>(IQueryable<T> query, DbContext dbContext, Func<IQueryable<T>, TResult> func)
    {
        using var transaction = dbContext.Database.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted);
        var result = func(query);
        transaction.Commit();
        return result;
    }

    #endregion
}