using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FluentAssertions;
using Horizon.Application.AzureKeyVault;
using Horizon.Application.Kubernetes;
using Horizon.Application.UseCases;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using Xunit;

namespace Horizon.Application.Unit.Tests.UseCases;

public class AzureKeyVaultSecretNewVersionCreatedHandlerTests
{
    private readonly SubscriptionsStore _store = new();
    private readonly Mock<IKeyVaultSecretReader> _secretReaderMock = new();
    private readonly Mock<IKubernetesSecretWriter> _secretWriterMock = new();
    private readonly FakeLogger<AzureKeyVaultSecretNewVersionCreatedHandler> _logger = new();

    [Fact]
    public async Task HandleAsync_ShouldPatchSecretsForAllKubernetesBundles_WhenNewVersionCreated()
    {
        // Arrange
        var request = new AzureKeyVaultSecretNewVersionCreatedRequest("vaultName", "secretName");
        var secretBundle = new SecretBundle("secretName", "secretValue");
        var kubernetesBundles = new List<KubernetesBundle>
        {
            new("secret1", null, "namespace1"),
            new("secret2", null,"namespace2")
        };

        _secretReaderMock.Setup(x => x.LoadSingleSecretAsync(request.VaultName, request.SecretName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(secretBundle);
        _store.AddSubscription(request.VaultName, kubernetesBundles[0]);
        _store.AddSubscription(request.VaultName, kubernetesBundles[1]);
        _secretWriterMock.Setup(x => x.PatchAsync(It.IsAny<string>(), It.IsAny<string>(), secretBundle, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var handler = new AzureKeyVaultSecretNewVersionCreatedHandler(
            _store,
            _logger,
            _secretReaderMock.Object,
            _secretWriterMock.Object);

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.False(result.IsError);

        foreach (var kubernetesBundle in kubernetesBundles)
        {
            _secretWriterMock.Verify(x => x.PatchAsync(kubernetesBundle.KubernetesSecretName, kubernetesBundle.Namespace, secretBundle, It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    [Fact]
    public async Task HandleAsync_ShouldLog_WhenKeyVaultIsNotConfigured()
    {
        // Arrange
        var request = new AzureKeyVaultSecretNewVersionCreatedRequest("vaultName", "secretName");

        var handler = new AzureKeyVaultSecretNewVersionCreatedHandler(
            _store,
            _logger,
            _secretReaderMock.Object,
            _secretWriterMock.Object);

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.False(result.IsError);
        _logger.LatestRecord.Message.Should().Be("NoAzureKeyVaultSubscriptionConfigForKeyVault");
    }
}
