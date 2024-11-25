using System.Diagnostics.CodeAnalysis;
using Core.Config;
using Microsoft.Extensions.Options;
using ThirdParty.Json.LitJson;
using Vault;
using Vault.Client;

namespace Core.Utils.Security;
[ExcludeFromCodeCoverage]
public class HashicorpSecretManager(ILogger<HashicorpSecretManager> logger, IOptions<SecretManagerConfig> options) : ISecretManager
{
    private readonly SecretManagerConfig _config = options.Value;
    public string GetSecret(string secretName)
    {
        try
        {
            var client = new VaultClient(new VaultConfiguration(_config.Server));
            client.SetToken(_config.Token);

            var secret = client.Read<object>(_config.SecretPath);
       
            var secretData = JsonMapper.ToObject(secret.Data.ToString());
            var value = secretData["data"][secretName].ToString(); 
            logger.LogDebug("HashicorpSecretManager GetSecret: success");
            return value;
        }
        catch (Exception e)
        {
            logger.LogError("HashicorpSecretManager GetSecret Error: {info}", e.Message);
            return "";
        }
    }
}