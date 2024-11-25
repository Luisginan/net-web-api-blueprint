using System.Diagnostics.CodeAnalysis;

namespace Core.Base;

[ExcludeFromCodeCoverage]
public class Message
{
    public string Key { get; init; } = "";
    public string Value { get; init; } = "";
}