using System.Diagnostics.CodeAnalysis;

namespace Blueprint.CustomerModule.Models;

[ExcludeFromCodeCoverage]
public class Product
{
    public int Id { get; init; }
    public string Title { get; init; } = "";
    public string Description { get; init; } = "";
    public decimal Price { get; set; }
    public double DiscountPercentage { get; set; }
    public double Rating { get; set; }
    public int Stock { get; set; }
    public string Brand { get; init; } = "";
    public string Category { get; init; } = "";
    public string Thumbnail { get; init; } = "";
}