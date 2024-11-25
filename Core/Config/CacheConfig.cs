using System.Diagnostics.CodeAnalysis;

namespace Core.Config;

[ExcludeFromCodeCoverage]
public class CacheConfig
{
    public string Server { get; init; } = "";
    public string Port { get; init; } = "";
    public string Database { get; init; } = "";
    public int DurationMinutes { get; init; }
    public string Provider { get; init; } = "";
}