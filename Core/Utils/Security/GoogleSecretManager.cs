using System.Diagnostics.CodeAnalysis;
using Core.Config;
using Google.Cloud.SecretManager.V1;
using Microsoft.Extensions.Options;

namespace Core.Utils.Security;
[ExcludeFromCodeCoverage]
public class GoogleSecretManager(IOptions<SecretManagerConfig> options, ILogger<GoogleSecretManager> logger) : ISecretManager
{
    private readonly string _projectName = options.Value.ProjectId;
    public string GetSecret(string secretName)
    {
        try
        {
            var client = SecretManagerServiceClient.Create();
        
            var name = $"projects/{_projectName}/secrets/{secretName}/versions/latest";
        
            var response = client.AccessSecretVersion(new AccessSecretVersionRequest { Name = name });
            logger.LogDebug("GoogleSecretManager GetSecret: {info}", $"Secret= {secretName}");
        
            return response.Payload.Data.ToStringUtf8();
        }
        catch (Exception e)
        {
            logger.LogError("GoogleSecretManager GetSecret: {info}", $"Error= {e.Message}");
            return "";
        }
       
    }
}