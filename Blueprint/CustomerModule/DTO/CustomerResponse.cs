using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Blueprint.CustomerModule.DTO;

[ExcludeFromCodeCoverage]
public class CustomerResponse
{
    [JsonPropertyName("id")]
    [DataMember]
    public int Id { get; init; }
    [JsonPropertyName("name")]
    [DataMember]
    public string? Name { get; init; }
    [JsonPropertyName("address")]
    [DataMember]
    public string? Address { get; init; }
    [JsonPropertyName("email")]
    [DataMember]
    public string? Email { get; init; }
    [JsonPropertyName("phone_number")]
    [DataMember]
    public string? Phone { get; init; }
}