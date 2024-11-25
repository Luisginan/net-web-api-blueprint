using System.Diagnostics.CodeAnalysis;

namespace Core.Utils.DB;

[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Class)]
public class TableAttribute(string name, string primaryKey) : Attribute
{
    public string Name { get; } = name;
    public string PrimaryKey { get; } = primaryKey;
}