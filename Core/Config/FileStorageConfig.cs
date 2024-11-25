using System.Diagnostics.CodeAnalysis;

namespace Core.Config;

[ExcludeFromCodeCoverage]
public class FileStorageConfig
{
    public string BucketName { get; init; } = "";
    public string Provider { get; init; } = "";
}