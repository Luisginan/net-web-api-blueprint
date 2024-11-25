using System.Diagnostics.CodeAnalysis;

namespace Core.Config;

[ExcludeFromCodeCoverage]
public class SchedulerConfig
{
    public int DelayInMs { get; init; } = 10000;
    public int LimitData { get; init; } = 5;
}