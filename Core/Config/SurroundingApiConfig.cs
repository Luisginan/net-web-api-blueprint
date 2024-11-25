using System.Diagnostics.CodeAnalysis;

namespace Core.Config;

[ExcludeFromCodeCoverage]
public class SurroundingApiConfig
{
    public string Provider { get; init; } = "";
}