using FluentAssertions;
using Moq;
using System.Threading.Tasks;
using Azure.Security.KeyVault.Secrets;
using Azure;
using Xunit;
using Horizon.Infrastructure.AzureKeyVault;

namespace Horizon.Infrastructure.Unit.Tests.AzureKeyVault;

public class ParallelSecretLoaderTests
{
    [Fact]
    public async Task WaitForAllAsync_ShouldReturnAllSecrets()
    {
        // Arrange
        var secretName1 = "testSecret1";
        var secretName2 = "testSecret2";
        var secret1 = new KeyVaultSecret(secretName1, "value1");
        var secret2 = new KeyVaultSecret(secretName2, "value2");
        var response1 = Response.FromValue(secret1, Mock.Of<Response>());
        var response2 = Response.FromValue(secret2, Mock.Of<Response>());
        var clientMock = new Mock<SecretClient>();
        clientMock.SetupSequence(x => x.GetSecretAsync(It.IsAny<string>(), null, default))
            .ReturnsAsync(response1)
            .ReturnsAsync(response2);
        var loader = new ParallelSecretLoader(clientMock.Object);
        loader.AddSecretToLoad(secretName1);
        loader.AddSecretToLoad(secretName2);

        // Act
        var secrets = await loader.WaitForAllAsync();

        // Assert
        secrets.Should().HaveCount(2);
        secrets[0].Value.Name.Should().Be(secretName1);
        secrets[0].Value.Value.Should().Be("value1");
        secrets[1].Value.Name.Should().Be(secretName2);
        secrets[1].Value.Value.Should().Be("value2");
    }
}
