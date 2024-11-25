using Core.Utils.Security;
using Core.Config;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CoreIntegrationTest.Utils.Security;

public class GoogleSecretManagerTest
{
    [Fact]
    public void TestGetSecret()
    {
        // Arrange
        var mockOptions = new Mock<IOptions<SecretManagerConfig>>();
        mockOptions.Setup(o => o.Value)
            .Returns(new SecretManagerConfig
            {
                ProjectId = "1025285657463",
                Provider = "google"
            });

        var mockLogger = new Mock<ILogger<GoogleSecretManager>>();
        
        var googleSecretManager =
            new GoogleSecretManager(mockOptions.Object, mockLogger.Object);

        // Act
        var secret = googleSecretManager.GetSecret("ConnectionDatabase");

        // Assert
        Assert.Equal("db-oneloan-alpha", secret);
    }
}