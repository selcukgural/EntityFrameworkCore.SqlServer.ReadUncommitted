using System;

namespace EntityFrameworkCore.SqlServer.ReadUncommitted;

/// <summary>
/// Represents configuration options for executing queries with the ReadUncommitted isolation level.
/// </summary>
public sealed class ReadUncommittedOptions
{
    /// <summary>
    /// Specifies the behavior for ambient transactions when executing queries.
    /// Default is <see cref="AmbientTransactionBehavior.Respect"/>.
    /// </summary>
    public AmbientTransactionBehavior AmbientBehavior { get; init; } = AmbientTransactionBehavior.Respect;

    /// <summary>
    /// Specifies an optional timeout for the transaction.
    /// If null, the default timeout is used.
    /// </summary>
    public TimeSpan? TransactionTimeout { get; init; }

    /// <summary>
    /// An optional action to handle exceptions that occur during query execution.
    /// </summary>
    public Action<Exception>? OnException { get; init; }

    /// <summary>
    /// Indicates whether exceptions should be swallowed during query execution.
    /// Default is <c>false</c>.
    /// </summary>
    public bool SwallowExceptions { get; init; } = false;
}

/// <summary>
/// Defines the possible behaviors for handling ambient transactions.
/// </summary>
public enum AmbientTransactionBehavior
{
    /// <summary>
    /// Respect the existing ambient transaction.
    /// </summary>
    Respect = 0,

    /// <summary>
    /// Suppress the existing ambient transaction.
    /// </summary>
    Suppress = 1,

    /// <summary>
    /// Create a new transaction, regardless of any existing ambient transaction.
    /// </summary>
    RequiresNew = 2
}