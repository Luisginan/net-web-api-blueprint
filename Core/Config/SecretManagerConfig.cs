using System.Diagnostics.CodeAnalysis;

namespace Core.Config;

[ExcludeFromCodeCoverage]
public class SecretManagerConfig
{
    public string ProjectId { get; init; } = "";
    public string Server { get; init; } = "";
    public string Token { get; init; } = "";
    public string SecretPath { get; init; }= "";
    public string Provider { get;init; } = "";
}