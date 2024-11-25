using System.Diagnostics.CodeAnalysis;

namespace Core.Base;

[ExcludeFromCodeCoverage]
public class Partner
{
    public string AppId { get; init; } = "";
    public string PrincipleName { get; init; } = "";
    public string PartnerName { get; init; } = "";
    public string Roles { get; init; } = "";
}