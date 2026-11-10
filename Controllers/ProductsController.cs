using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachingAPI.Dtos;
using TeachingAPI.Models;
using TeachingAPI.Services;

namespace TeachingAPI.Controllers;

[ApiController]
[Route("api/ankita/[controller]")]   // -> /api/ankita/products
// Every action below requires a valid JWT unless it says otherwise.
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _products;

    public ProductsController(IProductService products) => _products = products;

    /// <summary>GET /api/products - READ all. Public, no token needed.</summary>
    [HttpGet]
    [AllowAnonymous]   // overrides the controller-level [Authorize]
    public ActionResult<IEnumerable<Product>> GetAll() => Ok(_products.GetAll());

    /// <summary>GET /api/products/5 - READ one. "{id:int}" only matches integers.</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public ActionResult<Product> GetById(int id)
    {
        var product = _products.GetById(id);
        return product is null ? NotFound() : Ok(product);   // 404 or 200
    }

    /// <summary>POST /api/products - CREATE. Any logged-in user.</summary>
    [HttpPost]
    public ActionResult<Product> Create(ProductRequest request)
    {
        // User.Identity.Name comes from the "name" claim inside the validated token.
        var createdBy = User.Identity?.Name ?? "unknown";
        var product = _products.Create(request, createdBy);

        // 201 Created + a Location header pointing at GET /api/products/{id}.
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    /// <summary>PUT /api/products/5 - UPDATE (replace). Any logged-in user.</summary>
    [HttpPut("{id:int}")]
    public ActionResult<Product> Update(int id, ProductRequest request)
    {
        var product = _products.Update(id, request);
        return product is null ? NotFound() : Ok(product);
    }

    /// <summary>DELETE /api/products/5 - DELETE. Only users with the "Admin" role.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]   // role-based authorization: others get 403 Forbidden
    public IActionResult Delete(int id) => _products.Delete(id) ? NoContent() : NotFound();   // 204 or 404
}
