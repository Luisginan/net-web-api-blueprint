using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Blueprint.HealthCheck.Models;
[ExcludeFromCodeCoverage]
public class HealthCheckPayload
{
    [JsonProperty("audience")]
    public string Audience { get; set; } = "";
    [JsonProperty("input")]
    public string Input { get; set; } = "";
    [JsonProperty("filter")]
    public string Filter { get; set; } = "";
}