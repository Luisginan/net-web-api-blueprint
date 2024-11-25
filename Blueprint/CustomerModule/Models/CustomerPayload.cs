using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Blueprint.CustomerModule.Models;

[ExcludeFromCodeCoverage]
public class CustomerPayload
{
    [JsonProperty("id")]
    public string? Id { get; init; }
    [JsonProperty("message")]
    public string? Message { get; init; }
}