using System.Text.Json.Serialization;

namespace CoreIntegrationTest.Utils.Messaging;

public class TopModel
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = "";
    [JsonPropertyName("message")]
    public string Message { get; set; } = "";
}