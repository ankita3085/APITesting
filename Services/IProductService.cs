using TeachingAPI.Dtos;
using TeachingAPI.Models;

namespace TeachingAPI.Services;

/// <summary>
/// The contract for product data access. Controllers depend on this interface, not on a
/// concrete class, so we can swap the in-memory version for a real database later
/// without touching the controller (Dependency Inversion).
/// </summary>
public interface IProductService
{
    IEnumerable<Product> GetAll();
    Product? GetById(int id);                              // null when not found
    Product Create(ProductRequest request, string createdBy);
    Product? Update(int id, ProductRequest request);       // null when not found
    bool Delete(int id);                                   // false when not found
}
