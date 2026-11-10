namespace TeachingAPI.Models;

/// <summary>
/// The domain entity - what we "store" in our in-memory database.
/// </summary>
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;   // = string.Empty avoids null warnings
    public string? Description { get; set; }            // "?" means this one is allowed to be null
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
