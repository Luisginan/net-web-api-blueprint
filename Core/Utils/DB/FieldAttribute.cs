using System.Diagnostics.CodeAnalysis;

namespace Core.Utils.DB;

[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Property)]
public class FieldAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}