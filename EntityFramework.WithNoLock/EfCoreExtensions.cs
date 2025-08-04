using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace EntityFramework.WithNoLock;

/// <summary>
/// Provides extension methods for executing Entity Framework Core queries with NOLOCK (ReadUncommitted) isolation level.
/// </summary>
public static class WithNoLockExtensions
{
    /// <summary>
    /// Executes the query as a list asynchronously with NOLOCK (ReadUncommitted) isolation level.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the query.</typeparam>
    /// <param name="query">The queryable sequence to execute.</param>
    /// <param name="dbContext">The database context associated with the query.</param>
    /// <param name="expression">An optional filter expression to apply to the query.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the list of elements.</returns>
    public static Task<List<T>> ToListWithNoLockAsync<T>(this IQueryable<T> query, DbContext dbContext, Expression<Func<T, bool>> expression = null,
                                                         CancellationToken cancellationToken = default)
    {
        ValidateDbObjects(query, dbContext);
        return query.WithNoLockAsync(dbContext, q => q.ToListAsync(cancellationToken), expression);
    }

    /// <summary>
    /// Executes the query and returns the first element or a default value asynchronously with NOLOCK (ReadUncommitted) isolation level.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the query.</typeparam>
    /// <param name="query">The queryable sequence to execute.</param>
    /// <param name="dbContext">The database context associated with the query.</param>
    /// <param name="expression">An optional filter expression to apply to the query.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the first element or the default value.</returns>
    public static Task<T> FirstOrDefaultWithNoLockAsync<T>(this IQueryable<T> query, DbContext dbContext, Expression<Func<T, bool>> expression = null,
                                                           CancellationToken cancellationToken = default)
    {
        ValidateDbObjects(query, dbContext);
        return query.WithNoLockAsync(dbContext, q => q.FirstOrDefaultAsync(cancellationToken), expression);
    }

    /// <summary>
    /// Executes the query and returns the count of elements asynchronously with NOLOCK (ReadUncommitted) isolation level.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the query.</typeparam>
    /// <param name="query">The queryable sequence to execute.</param>
    /// <param name="dbContext">The database context associated with the query.</param>
    /// <param name="expression">An optional filter expression to apply to the query.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the count of elements.</returns>
    public static Task<int> CountWithNoLockAsync<T>(this IQueryable<T> query, DbContext dbContext, Expression<Func<T, bool>> expression = null,
                                                    CancellationToken cancellationToken = default)
    {
        ValidateDbObjects(query, dbContext);
        return query.WithNoLockAsync(dbContext, q => q.CountAsync(cancellationToken), expression);
    }

    /// <summary>
    /// Validates that the query and database context are not null.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the query.</typeparam>
    /// <param name="query">The queryable sequence to validate.</param>
    /// <param name="dbContext">The database context to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if the query or dbContext is null.</exception>
    private static void ValidateDbObjects<T>(IQueryable<T> query, DbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
    }

    /// <summary>
    /// Asynchronously converts the queryable sequence to a dictionary while applying the NOLOCK query hint.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the resulting dictionary.</typeparam>
    /// <typeparam name="T">The type of the elements in the query.</typeparam>
    /// <param name="query">The queryable sequence to execute.</param>
    /// <param name="dbContext">The database context associated with the query.</param>
    /// <param name="keySelector">A function to extract a key from each element.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the dictionary of elements.</returns>
    public static Task<Dictionary<TKey, T>> ToDictionaryWithNoLockAsync<TKey, T>(this IQueryable<T> query, DbContext dbContext,
                                                                                 Func<T, TKey> keySelector,
                                                                                 CancellationToken cancellationToken = default)
    {
        ValidateDbObjects(query, dbContext);
        ArgumentNullException.ThrowIfNull(keySelector, nameof(keySelector));

        return query.WithNoLockAsync(dbContext, q => q.ToDictionaryAsync(keySelector, cancellationToken));
    }

    /// <summary>
    /// Executes the specified query asynchronously within a transaction scope that uses the ReadUncommitted isolation level (NOLOCK).
    /// </summary>
    /// <typeparam name="T">The type of the elements in the query.</typeparam>
    /// <typeparam name="TResult">The type of the result returned by the query.</typeparam>
    /// <param name="query">The queryable sequence to execute.</param>
    /// <param name="dbContext">The database context associated with the query.</param>
    /// <param name="func">A function that defines the query execution logic.</param>
    /// <param name="expression">An optional filter expression to apply to the query.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the result of the query execution.</returns>
    private static async Task<TResult> WithNoLockAsync<T, TResult>(this IQueryable<T> query, DbContext dbContext,
                                                                   Func<IQueryable<T>, Task<TResult>> func,
                                                                   Expression<Func<T, bool>> expression = null)
    {
        ValidateDbObjects(query, dbContext);
        ArgumentNullException.ThrowIfNull(func, nameof(func));

        var strategy = dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            using var scope = new TransactionScope(TransactionScopeOption.Required,
                                                   new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted },
                                                   TransactionScopeAsyncFlowOption.Enabled);

            if (expression is not null)
            {
                query = query.Where(expression);
            }

            var result = await func(query);
            scope.Complete();
            return result;
        });
    }
}