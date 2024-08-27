using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Security.KeyVault.Secrets;
using ErrorOr;
using FluentAssertions;
using Horizon.Application;
using Horizon.Infrastructure.AzureKeyVault;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Horizon.Infrastructure.Unit.Tests.AzureKeyVault;

public class KeyVaultSecretReaderTests
{
    private readonly Mock<ILogger<KeyVaultSecretReader>> _loggerMock;
    private readonly Mock<SecretClientFactory> _clientFactoryMock;
    private readonly KeyVaultSecretReader _secretReader;

    public KeyVaultSecretReaderTests()
    {
        _loggerMock = new Mock<ILogger<KeyVaultSecretReader>>();
        _clientFactoryMock = new Mock<SecretClientFactory>();
        _secretReader = new KeyVaultSecretReader(_loggerMock.Object, _clientFactoryMock.Object);
    }

    [Fact]
    public async Task LoadSingleSecretAsync_ShouldReturnSecretBundle_WhenSecretExists()
    {
        // Arrange
        var vaultName = "test-vault";
        var secretName = "test-secret";
        var secretValue = "test-value";
        var secretResponse = new KeyVaultSecret(secretName, secretValue);
        var clientMock = new Mock<SecretClient>();
        clientMock.Setup(c => c.GetSecretAsync(secretName, null, default))
            .ReturnsAsync(Response.FromValue(secretResponse, Mock.Of<Response>()));
        _clientFactoryMock.Setup(f => f.CreateClient(vaultName))
            .Returns(clientMock.Object);

        // Act
        var result = await _secretReader.LoadSingleSecretAsync(vaultName, secretName);

        // Assert
        result.Value.Should().BeEquivalentTo(new SecretBundle(secretName, secretValue));
    }

    [Fact]
    public async Task LoadSingleSecretAsync_ShouldReturnError_WhenSecretDoesNotExist()
    {
        // Arrange
        var vaultName = "test-vault";
        var secretName = "test-secret";
        var exceptionMessage = "Secret not found";
        var exception = new RequestFailedException(404, exceptionMessage);
        var clientMock = new Mock<SecretClient>();
        clientMock.Setup(c => c.GetSecretAsync(secretName, null, default))
            .ThrowsAsync(exception);
        _clientFactoryMock.Setup(f => f.CreateClient(vaultName))
            .Returns(clientMock.Object);

        // Act
        var result = await _secretReader.LoadSingleSecretAsync(vaultName, secretName);

        // Assert
        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task LoadAllSecretsAsync_ShouldReturnSecretBundles_WhenSecretsExist()
    {
        // Arrange
        var vaultName = "test-vault";
        var secretPrefix = "test";
        var secretProperties1 = SecretModelFactory.SecretProperties(name: "test-secret1");
        secretProperties1.Enabled = true;
        var secretProperties2 = SecretModelFactory.SecretProperties(name: "test-secret2");
        secretProperties2.Enabled = true;
        var secretProperties3 = SecretModelFactory.SecretProperties(name: "other-secret");
        secretProperties3.Enabled = true;
        var secret1 = SecretModelFactory.KeyVaultSecret(secretProperties1, "test-value-1");
        var secret2 = SecretModelFactory.KeyVaultSecret(secretProperties2, "test-value-2");
        var secret3 = SecretModelFactory.KeyVaultSecret(secretProperties3, "test-value-3");
        var secrets = new List<SecretProperties> { secretProperties1, secretProperties2, secretProperties3 };
        var clientMock = new Mock<SecretClient>();
        clientMock.Setup(c => c.GetPropertiesOfSecretsAsync(default))
            .Returns(secrets.ToAsyncPageable());
        clientMock.Setup(c => c.GetSecretAsync(secret1.Name, null, default))
            .ReturnsAsync(Response.FromValue(secret1, Mock.Of<Response>()));
        clientMock.Setup(c => c.GetSecretAsync(secret2.Name, null, default))
            .ReturnsAsync(Response.FromValue(secret2, Mock.Of<Response>()));
        clientMock.Setup(c => c.GetSecretAsync(secret3.Name, null, default))
            .ReturnsAsync(Response.FromValue(secret3, Mock.Of<Response>()));
        _clientFactoryMock.Setup(f => f.CreateClient(vaultName))
            .Returns(clientMock.Object);

        // Act
        var result = await _secretReader.LoadAllSecretsAsync(vaultName, secretPrefix);

        // Assert
        result.Value.Should().BeEquivalentTo(new List<SecretBundle>
        {
            new(secret1.Name, secret1.Value),
            new(secret2.Name, secret2.Value)
        });
    }
}
