using System.Collections.Concurrent;
using TeachingAPI.Dtos;
using TeachingAPI.Models;

namespace TeachingAPI.Services;

/// <summary>
/// "Database" that lives in RAM. Data is lost when the app stops.
/// Registered as a Singleton in Program.cs so every request shares the same dictionary.
/// </summary>
public class InMemoryProductService : IProductService
{
    // ConcurrentDictionary is thread-safe: many requests can run at the same time.
    private readonly ConcurrentDictionary<int, Product> _products = new();
    private int _lastId;

    public InMemoryProductService()
    {
        // Seed a couple of rows so GET has something to return.
        Create(new ProductRequest { Name = "Keyboard", Description = "Mechanical keyboard", Price = 59.99m, Stock = 25 }, "seed");
        Create(new ProductRequest { Name = "Mouse", Description = "Wireless mouse", Price = 24.50m, Stock = 80 }, "seed");
    }

    public IEnumerable<Product> GetAll() => _products.Values.OrderBy(p => p.Id);

    public Product? GetById(int id) => _products.TryGetValue(id, out var product) ? product : null;

    public Product Create(ProductRequest request, string createdBy)
    {
        // Interlocked.Increment gives a unique id even with parallel requests.
        var id = Interlocked.Increment(ref _lastId);
        var product = new Product
        {
            Id = id,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock,
            CreatedBy = createdBy
        };
        _products[id] = product;
        return product;
    }

    public Product? Update(int id, ProductRequest request)
    {
        if (!_products.TryGetValue(id, out var product))
            return null;

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Stock = request.Stock;
        return product;
    }

    public bool Delete(int id) => _products.TryRemove(id, out _);
}
//DELETE FROM USERS WHERE id = 1;
