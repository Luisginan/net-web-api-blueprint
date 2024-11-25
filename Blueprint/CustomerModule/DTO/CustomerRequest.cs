using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Blueprint.CustomerModule.DTO;

[ExcludeFromCodeCoverage]
public class CustomerRequest
{
    [Required]
    [MaxLength(100)]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [MinLength(10)]
    [EmailAddress]
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [JsonPropertyName("phone_number")]
    public string Phone { get; set; } = string.Empty;
}