using System.Diagnostics.CodeAnalysis;

namespace Core.Config;
[ExcludeFromCodeCoverage]
public class TracerConfig
{
    public string Exporter { get; init; } = "";
    public string ExporterHost { get; init; } = "";
    public string ExporterPort { get; init; } = "";
    
    public bool ExportToConsole { get; init; }
}