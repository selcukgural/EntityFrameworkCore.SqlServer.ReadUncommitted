# EntityFramework.WithNoLock

This project provides extension methods for executing Entity Framework Core queries at the `NOLOCK` (ReadUncommitted) isolation level. The goal is to prevent read operations from being affected by database locks, especially in high-concurrency scenarios.

## Purpose

The main purpose is to prevent read queries made via EF Core from waiting due to locks created by other database operations. By using the `ReadUncommitted` isolation level, a query is allowed to read data that has been modified by other uncommitted transactions. This is known as a "dirty read," but it can significantly improve read performance and reduce the risk of deadlocks.

## Usage

To use these extension methods, simply include the `EfCoreExtensions.cs` file in your project and add the `using EntityFramework.WithNoLock;` directive.

The methods extend the `IQueryable<T>` interface and are used with your existing `DbContext` instance.

```csharp
using EntityFramework.WithNoLock;
using Microsoft.EntityFrameworkCore;

// ...

var productList = await _context.Products
    .Where(p => p.IsActive)
    .ToListWithNoLockAsync(_context);
```

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
    public async Task<Product> GetProductByIdAsync(int id)
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
