using Azure.Core;
using Azure.Security.KeyVault.Secrets;
using FluentAssertions;
using Horizon.Infrastructure.AzureKeyVault;
using Moq;
using Xunit;

namespace Horizon.Infrastructure.Unit.Tests.AzureKeyVault;

public class SecretClientFactoryTests
{
    [Fact]
    public void CreateClient_ShouldReturnSecretClient()
    {
        // Arrange
        var credentialMock = new Mock<TokenCredential>();
        var factory = new SecretClientFactory(credentialMock.Object);
        var keyVaultName = "myKeyVault";

        // Act
        var client = factory.CreateClient(keyVaultName);

        // Assert
        client.Should().NotBeNull();
        client.Should().BeOfType<SecretClient>();
    }

    [Fact]
    public void CreateClient_ShouldReturnSameClientForSameKeyVaultName()
    {
        // Arrange
        var credentialMock = new Mock<TokenCredential>();
        var factory = new SecretClientFactory(credentialMock.Object);
        var keyVaultName = "myKeyVault";

        // Act
        var client1 = factory.CreateClient(keyVaultName);
        var client2 = factory.CreateClient(keyVaultName);

        // Assert
        client1.Should().BeSameAs(client2);
    }
}
