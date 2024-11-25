using System.Diagnostics.CodeAnalysis;

namespace Core.Config;
[ExcludeFromCodeCoverage]
public class ApiConfig
{
    public string Protocol { get; set; } = "rest";
}