using System.Diagnostics.CodeAnalysis;

namespace Core.Config;

[ExcludeFromCodeCoverage]
public class LogDbConfig
{
    public string Server { get; init; } = "";
    public string Database { get; init; } = "";
    public string Port { get; init; } = "";
    public string User { get; init; } = "";
    public string Password { get; init; } = "";
    public string Provider { get; init; } = "";
}