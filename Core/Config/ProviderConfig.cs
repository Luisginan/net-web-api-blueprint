using System.Diagnostics.CodeAnalysis;

namespace Core.Config;

[ExcludeFromCodeCoverage]
public class ProviderConfig
{
    public string Messaging { get; init; } = "";
    public string LogDb { get; init; } = "";
    public string Cache { get; init; } = "";
    public string Database { get; init; } = "";
    public string FileStorage { get; init; } = "";
    public string SecretManager { get; init; } = "";
    public string SurroundingApi { get; init; } = "";
}