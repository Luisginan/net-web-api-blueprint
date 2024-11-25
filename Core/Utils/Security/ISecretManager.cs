namespace Core.Utils.Security;

public interface ISecretManager
{
    string GetSecret(string secretName);
}