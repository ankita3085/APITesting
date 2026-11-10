using System.ComponentModel.DataAnnotations;

namespace TeachingAPI.Dtos;

// DTO = Data Transfer Object. It describes the JSON shape that goes in/out of the API,
// so clients never see or control internal fields (like Id or CreatedBy) directly.
// [Required], [Range] ... are validation rules; [ApiController] checks them automatically
// and returns 400 Bad Request if they fail.

/// <summary>Body of POST /api/products and PUT /api/products/{id}.</summary>
public class ProductRequest
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Range(0.01, 1_000_000)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }
}
