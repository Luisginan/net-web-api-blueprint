using System.Diagnostics.CodeAnalysis;

namespace Core.Utils.DB;

[ExcludeFromCodeCoverage]
public class CFieldProperty
{
    public string Name { get; set; } = "";

    public Type Type { get; set; } = typeof(string);
}