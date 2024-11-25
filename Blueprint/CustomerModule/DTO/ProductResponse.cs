using System.Diagnostics.CodeAnalysis;

namespace Blueprint.CustomerModule.DTO;

[ExcludeFromCodeCoverage]
public class ProductResponse
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Brand { get; set; }
    public string? Category { get; set; }
    public string? Thumbnail { get; set; }
}