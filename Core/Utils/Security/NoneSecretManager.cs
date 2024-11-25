using System.Diagnostics.CodeAnalysis;

namespace Core.Utils.Security;
[ExcludeFromCodeCoverage]
public class NoneSecretManager : ISecretManager
{
    public string GetSecret(string secretName)
    {
        return "";
    }
}