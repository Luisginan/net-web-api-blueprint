using System.Diagnostics.CodeAnalysis;
using Microsoft.IdentityModel.Tokens;

namespace Core.Utils.Security;
[ExcludeFromCodeCoverage]
public class VaultManager(ISecretManager secretManager) : IVault
{

    public T RevealSecret<T>(T config)
    {
        var properties = config?.GetType().GetProperties();
        if (properties == null) return config;
        
        foreach (var property in properties)
        {
            if (property.PropertyType != typeof(string)) continue;
            
            var value = property.GetValue(config);
            if (value is not string stringValue || !stringValue.StartsWith("$(") ||
                !stringValue.EndsWith(')')) continue;
            
            var secretName = stringValue.Replace("$(", "");
            secretName = secretName.Replace(")", "");
            var secretValue = secretManager.GetSecret(secretName);
            if (!secretValue.IsNullOrEmpty())
                property.SetValue(config, secretValue);
        }

        return config;
    }
}