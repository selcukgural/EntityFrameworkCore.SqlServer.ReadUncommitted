# EntityFramework.WithNoLock

Small helper extensions to execute EF Core queries using the ReadUncommitted isolation level (commonly known as NOLOCK in SQL Server) to reduce blocking for read-heavy, high-concurrency scenarios.

Important: This library does NOT attempt to rewrite SQL or inject table hints. It uses a short-lived transaction with IsolationLevel.ReadUncommitted, letting the provider handle semantics.

## Install

```bash
dotnet add package EntityFramework.WithNoLock
```

## Usage

```csharp
using EntityFramework.WithNoLock;

// ToList
var users = await dbContext.Users
    .ToListWithNoLockAsync(dbContext, cancellationToken: ct);

// FirstOrDefault with optional filter
var user = await dbContext.Users
    .FirstOrDefaultWithNoLockAsync(dbContext, u => u.Id == id, ct);

// Count
var count = await dbContext.Users
    .CountWithNoLockAsync(dbContext, cancellationToken: ct);

// ToDictionary (key only)
var byId = await dbContext.Users
    .ToDictionaryWithNoLockAsync(dbContext, u => u.Id, ct);

// ToDictionary (key + value + comparer)
var map = await dbContext.Users
    .ToDictionaryWithNoLockAsync(dbContext, u => u.Id, u => u.Email, StringComparer.OrdinalIgnoreCase, ct);
```

### Ambient transaction behavior
If there is an existing transaction (DbContext.Database.CurrentTransaction or an ambient System.Transactions.Transaction), the library will NOT try to override its isolation level. In that case, queries execute under the existing transaction's isolation.

### Provider notes
- Optimized for SQL Server. On some providers (e.g., PostgreSQL), ReadUncommitted may behave as ReadCommitted; consult your provider's documentation.
- Only use for read operations. These helpers never alter writes.

### Risks of NOLOCK / ReadUncommitted
- Dirty reads (uncommitted data)
- Non-repeatable reads
- Phantom reads
- Missing or duplicated rows in edge cases

Use sparingly and only where you can tolerate these anomalies.

## Available Methods

- **`ToListWithNoLockAsync<T>(...)`**: Executes the query with `NOLOCK` and returns the result as a `List<T>`.
- **`FirstOrDefaultWithNoLockAsync<T>(...)`**: Executes the query with `NOLOCK` and returns the first element found or a default value.
- **`CountWithNoLockAsync<T>(...)`**: Executes the query with `NOLOCK` and returns the number of elements (`int`) in the result set.
- **`ToDictionaryWithNoLockAsync<TKey, T>(...)`**: Executes the query with `NOLOCK` and returns the result as a `Dictionary<TKey, T>`.

All methods can optionally take an `Expression<Func<T, bool>>` filter and a `CancellationToken`.

## Important Considerations

- **Dirty Read**: These methods use the `ReadUncommitted` isolation level. This means you can read data that has been added by another transaction but has not yet been committed. If that transaction is rolled back, the data you read will have never existed.
- **When to Use**: Ideal for scenarios where performance is more critical than data consistency, such as reporting, logging, or dashboards where 100% real-time data accuracy is not required.
- **When to Avoid**: Should **not** be used in critical business workflows where absolute data accuracy and consistency are mandatory, such as financial transactions or inventory management.

## Usage Scenarios and Examples

Below is a more detailed example of how to use these extension methods.

### 1. Model and DbContext Definition

First, let's assume we have a model and a `DbContext` class.

```csharp
// Model
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
}

// DbContext
public class AppDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
}
```

### 2. Using the Extension Methods

You can execute your queries with `NOLOCK` through your `DbContext` instance as shown below.

```csharp
using EntityFramework.WithNoLock;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

public class ProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    // Gets the active products as a list
    public async Task<List<Product>> GetActiveProductsAsync()
    {
        // Usage of ToListWithNoLockAsync
        return await _context.Products
                             .Where(p => p.IsActive)
                             .ToListWithNoLockAsync(_context);
    }

    // Gets the first product with the specified ID
    public async Task<Product?> GetProductByIdAsync(int id)
    {
        // Usage of FirstOrDefaultWithNoLockAsync
        return await _context.Products
                             .FirstOrDefaultWithNoLockAsync(_context, p => p.Id == id);
    }

    // Gets the count of inactive products
    public async Task<int> GetInactiveProductCountAsync()
    {
        // Usage of CountWithNoLockAsync
        return await _context.Products
                             .CountWithNoLockAsync(_context, p => !p.IsActive);
    }
    
    // Converts products to a dictionary using their IDs as keys
    public async Task<Dictionary<int, Product>> GetProductsAsDictionaryAsync()
    {
        // Usage of ToDictionaryWithNoLockAsync
        return await _context.Products
                             .Where(p => p.IsActive)
                             .ToDictionaryWithNoLockAsync(_context, p => p.Id);
    }
}
```

## CI and Releases
Publishing to NuGet is automated via GitHub Actions on tags matching `v*.*.*`.

Steps:
1. Create a NuGet API key and add it to the repository secrets as `NUGET_API_KEY`.
2. Bump the version in the .csproj.
3. Tag and push:
   ```bash
   git tag v1.0.0
   git push --tags
   ```
The workflow will pack and push the package to NuGet (skipping duplicates).

## License
The package uses the repository's existing LICENSE file. See the root LICENSE for details.
