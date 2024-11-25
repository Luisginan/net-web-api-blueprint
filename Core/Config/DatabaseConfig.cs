using System.Diagnostics.CodeAnalysis;

namespace Core.Config;

[ExcludeFromCodeCoverage]
public class DatabaseConfig
{
    public string Type { get; init; } = "";
    public string Server { get; init; } = "";
    public string Database { get; init; } = "";
    public string User { get; init; } = "";

    public string Password { get; init; } = "";

    public string CommandTimeout { get; init; }  = "";

    public string Port { get; init; } = "";

    public string PoolSize { get; init; } = "";
    public string ConnectTimeout { get; init; } = "";
    public string Provider { get; init; } = "";
}